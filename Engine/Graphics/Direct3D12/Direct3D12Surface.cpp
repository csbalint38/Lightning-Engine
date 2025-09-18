#include "Direct3D12Surface.h"
#include "Direct3D12Core.h"
#include "Direct3D12LightCulling.h"

#include <DirectXPackedVector.h>
#include <wincodec.h>
#include <algorithm>

namespace lightning::graphics::direct3d12 {
	namespace {
		using namespace DirectX;
		using namespace DirectX::PackedVector;

		constexpr DXGI_FORMAT to_non_srgb(DXGI_FORMAT format) {
		if (format == DXGI_FORMAT_R8G8B8A8_UNORM_SRGB) return DXGI_FORMAT_R8G8B8A8_UNORM;
			return format;
		}

		static void save_png(const wchar_t* path, u32 width, u32 height, u32 stride, const u8* rgba8) {
			IWICImagingFactory* factory;
			DXCall(CoCreateInstance(CLSID_WICImagingFactory2, nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&factory)));

			IWICStream* stream;
			factory->CreateStream(&stream);
			stream->InitializeFromFilename(path, GENERIC_WRITE);

			IWICBitmapEncoder* encoder;
			factory->CreateEncoder(GUID_ContainerFormatPng, nullptr, &encoder);
			encoder->Initialize(stream, WICBitmapEncoderNoCache);

			IWICBitmapFrameEncode* frame;
			IPropertyBag2* props{};

			encoder->CreateNewFrame(&frame, &props);
			frame->Initialize(props);
			frame->SetSize(width, height);

			WICPixelFormatGUID format = GUID_WICPixelFormat32bppBGRA;

			frame->SetPixelFormat(&format);

			assert(IsEqualGUID(format, GUID_WICPixelFormat32bppBGRA));

			frame->WritePixels(height, stride, stride * height, const_cast<BYTE*>(rgba8));
			frame->Commit();
			encoder->Commit();

			props->Release();
			frame->Release();
			encoder->Release();
			stream->Release();
			factory->Release();
		}

		inline f32 linear_to_srgb(f32 x) {
			x = x <= 0.f ? 0.f : x;

			if (x <= .0031308f) return 12.92f * x;

			return 1.055 * powf(x, 1.f / 2.4f) - .055f;
		}
	}

	void D3D12Surface::create_swap_chain(IDXGIFactory7* factory, ID3D12CommandQueue* cmd_queue) {
		assert(factory && cmd_queue);
		release();

		if (SUCCEEDED(factory->CheckFeatureSupport(DXGI_FEATURE_PRESENT_ALLOW_TEARING, &_allow_tearing, sizeof(u32))) && _allow_tearing) {
			_present_flags = DXGI_PRESENT_ALLOW_TEARING;
		}

		DXGI_SWAP_CHAIN_DESC1 desc{};
		desc.AlphaMode = DXGI_ALPHA_MODE_UNSPECIFIED;
		desc.BufferCount = buffer_count;
		desc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
		desc.Flags = _allow_tearing ? DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0;
		desc.Format = to_non_srgb(default_back_buffer_format);
		desc.Width = _window.width();
		desc.Height = _window.height();
		desc.SampleDesc.Count = 1;
		desc.SampleDesc.Quality = 0;
		desc.Scaling = DXGI_SCALING_STRETCH;
		desc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
		desc.Stereo = false;

		IDXGISwapChain1* swap_chain;
		HWND hwnd{ (HWND)_window.handle() };
		DXCall(factory->CreateSwapChainForHwnd(cmd_queue, hwnd, &desc, nullptr, nullptr, &swap_chain));
		DXCall(factory->MakeWindowAssociation(hwnd, DXGI_MWA_NO_ALT_ENTER));
		DXCall(swap_chain->QueryInterface(IID_PPV_ARGS(&_swap_chain)));
		core::release(swap_chain);

		_current_bb_index = _swap_chain->GetCurrentBackBufferIndex();

		for (u32 i{ 0 }; i < buffer_count; ++i) {
			_render_target_data[i].rtv = core::rtv_heap().allocate();
		}

		finalize();

		assert(!id::is_valid(_light_culling_id));
		_light_culling_id = delight::add_culler();
	}

	void D3D12Surface::present() const {
		assert(_swap_chain);
		DXCall(_swap_chain->Present(0, _present_flags));
		_current_bb_index = _swap_chain->GetCurrentBackBufferIndex();
	}

	void D3D12Surface::resize() {
		assert(_swap_chain);
		for (u32 i{ 0 }; i < buffer_count; ++i) {
			core::release(_render_target_data[i].resource);
		}

		const u32 flags{ _allow_tearing ? DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0ul };
		DXCall(_swap_chain->ResizeBuffers(buffer_count, 0, 0, DXGI_FORMAT_UNKNOWN, flags));
		_current_bb_index = _swap_chain->GetCurrentBackBufferIndex();

		finalize();

		DEBUG_OP(OutputDebugString(L"::D3D12 Surface Resized\n"));
	}

	void D3D12Surface::finalize() {
		for (u32 i{ 0 }; i < buffer_count; ++i) {
			RenderTargetData& data{ _render_target_data[i] };
			assert(!data.resource);
			DXCall(_swap_chain->GetBuffer(i, IID_PPV_ARGS(&data.resource)));
			D3D12_RENDER_TARGET_VIEW_DESC desc{};
			desc.Format = default_back_buffer_format;
			desc.ViewDimension = D3D12_RTV_DIMENSION_TEXTURE2D;
			core::device()->CreateRenderTargetView(data.resource, &desc, data.rtv.cpu);
		}

		DXGI_SWAP_CHAIN_DESC1 desc{};
		DXCall(_swap_chain->GetDesc1(&desc));
		const u32 width{ desc.Width };
		const u32 height{ desc.Height };
		assert(_window.width() == width && _window.height() == height);
		_viewport.TopLeftX = 0.f;
		_viewport.TopLeftY = 0.f;
		_viewport.Width = (f32)width;
		_viewport.Height = (f32)height;
		_viewport.MinDepth = 0.f;
		_viewport.MaxDepth = 1.f;

		_scissor_rect = { 0, 0, (s32)width, (s32)height };
	}

	void D3D12Surface::release() {
		if (id::is_valid(_light_culling_id)) {
			delight::remove_culler(_light_culling_id);
		}

		for (u32 i{ 0 }; i < buffer_count; ++i) {
			RenderTargetData& data{ _render_target_data[i] };
			core::release(data.resource);
			core::rtv_heap().free(data.rtv);
		}
		core::release(_swap_chain);
	}

	bool D3D12Surface::request_capture(const wchar_t* path) {
		_capture_state.path = path ? path : L"screenshot.png";
		_capture_state.pending = true;

		return true;
	}

	void D3D12Surface::record_capture(id3d12_graphics_command_list* cmd) {
		if (!_capture_state.pending) return;

		const D3D12_RESOURCE_DESC desc = back_buffer()->GetDesc();

		// TODO: Handle MSAA when its enabled

		u64 total_bytes{ 0 };
		u32 num_rows{ 0 };
		u64 row_size{ 0 };
		D3D12_PLACED_SUBRESOURCE_FOOTPRINT fp{};

		core::device()->GetCopyableFootprints(&desc, 0, 1, 0, &fp, &num_rows, &row_size, &total_bytes);

		D3D12_HEAP_PROPERTIES hp{};
		hp.Type = D3D12_HEAP_TYPE_READBACK;
		
		D3D12_RESOURCE_DESC rb{};
		rb.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
		rb.Width = total_bytes;
		rb.Height = 1;
		rb.DepthOrArraySize = 1;
		rb.MipLevels = 1;
		rb.SampleDesc = { 1, 0 };
		rb.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;

		ID3D12Resource* readback;

		if (FAILED(core::device()->CreateCommittedResource(&hp, D3D12_HEAP_FLAG_NONE, &rb, D3D12_RESOURCE_STATE_COPY_DEST, nullptr, IID_PPV_ARGS(&readback)))) {
			_capture_state.pending = false;

			return;
		}

		d3dx::transition_resource(cmd, back_buffer(), D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATE_COPY_SOURCE);

		D3D12_TEXTURE_COPY_LOCATION src{};
		src.pResource = back_buffer();
		src.Type = D3D12_TEXTURE_COPY_TYPE_SUBRESOURCE_INDEX;
		src.SubresourceIndex = 0;

		D3D12_TEXTURE_COPY_LOCATION dest{};
		dest.pResource = readback;
		dest.Type = D3D12_TEXTURE_COPY_TYPE_PLACED_FOOTPRINT;
		dest.PlacedFootprint = fp;

		cmd->CopyTextureRegion(&dest, 0, 0, 0, &src, nullptr);

		d3dx::transition_resource(cmd, back_buffer(), D3D12_RESOURCE_STATE_COPY_SOURCE, D3D12_RESOURCE_STATE_RENDER_TARGET);

		_capture_state.readback = readback;
		_capture_state.fp = fp;
		_capture_state.width = (u32)desc.Width;
		_capture_state.height = (u32)desc.Height;
		_capture_state.total_bytes = total_bytes;

		if (!_capture_state.fence) {
			core::device()->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&_capture_state.fence));
			_capture_state.fence_value = 0;
		}
	}

	void D3D12Surface::finalize_capture(ID3D12CommandQueue* queue) {
		if (!_capture_state.pending || !_capture_state.readback) return;

		const u64 v = ++_capture_state.fence_value;

		queue->Signal(_capture_state.fence, v);

		if (_capture_state.fence->GetCompletedValue() < v) {
			HANDLE event = CreateEvent(nullptr, FALSE, FALSE, nullptr);
			
			_capture_state.fence->SetEventOnCompletion(v, event);
			WaitForSingleObject(event, INFINITE);
			CloseHandle(event);
		}

		u8* bytes{ nullptr };
		D3D12_RANGE r{ 0, (SIZE_T)_capture_state.total_bytes};

		if (SUCCEEDED(_capture_state.readback->Map(0, &r, reinterpret_cast<void**>(&bytes))) && bytes) {
			const u32 dest_stride = _capture_state.width * 4;
			util::vector<u8> out(dest_stride * _capture_state.height);

			for (u32 y{ 0 }; y < _capture_state.height; ++y) {
				const u16* src_row = reinterpret_cast<const u16*>(bytes + y * _capture_state.fp.Footprint.RowPitch);
				u8* dest_row = out.data() + y * dest_stride;

				for (u32 x{ 0 }; x < _capture_state.width; ++x) {
					XMHALF4 hv{};
					hv.x = src_row[4 * x + 0];
					hv.y = src_row[4 * x + 1];
					hv.z = src_row[4 * x + 2];
					hv.w = src_row[4 * x + 3];

					XMFLOAT4 f;
					XMStoreFloat4(&f, XMLoadHalf4(&hv));

					f32 r = linear_to_srgb(f.x);
					f32 g = linear_to_srgb(f.y);
					f32 b = linear_to_srgb(f.z);

					// NOTE: WIC needs the pixel format to be in BGRA
					dest_row[4 * x + 0] = (u8)(std::clamp(b, 0.f, 1.f) * 255.f + .5f);
					dest_row[4 * x + 1] = (u8)(std::clamp(g, 0.f, 1.f) * 255.f + .5f);
					dest_row[4 * x + 2] = (u8)(std::clamp(r, 0.f, 1.f) * 255.f + .5f);
					dest_row[4 * x + 3] = (u8)(std::clamp(f.w, 0.f, 1.f) * 255.f + .5f);
				}
			}
			D3D12_RANGE w{ 0, 0 };
			_capture_state.readback->Unmap(0, &w);

			save_png(_capture_state.path.c_str(), _capture_state.width, _capture_state.height, dest_stride, out.data());
		}

		_capture_state.readback->Release();
		_capture_state.fence->Release();
		_capture_state = CaptureState{};
	}
}