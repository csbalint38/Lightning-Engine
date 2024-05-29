#include "Direct3D12GPass.h"
#include "Direct3D12Core.h"
#include "Direct3D12Shaders.h"

namespace lightning::graphics::direct3d12::gpass {
	namespace {

		constexpr DXGI_FORMAT main_buffer_format{ DXGI_FORMAT_R16G16B16A16_FLOAT };
		constexpr DXGI_FORMAT depth_buffer_format{ DXGI_FORMAT_D32_FLOAT };
		constexpr math::u32v2 initial_dimensions{ 100, 100 };

		ID3D12RootSignature* gpass_root_sig{ nullptr };
		ID3D12PipelineState* gpass_pso{ nullptr };

		#if _DEBUG
		constexpr f32 clear_value[4]{ .5f, .5f, .5f, 1.f };
		#else
		constexpr f32 clear_value[4]{};
		#endif

		D3D12RenderTexture gpass_main_buffer{};
		D3D12DepthBuffer gpass_depth_buffer{};
		math::u32v2 dimensions{ initial_dimensions };

		bool create_buffers(math::u32v2 size) {
			assert(size.x && size.y);
			gpass_main_buffer.release();
			gpass_depth_buffer.release();

			D3D12_RESOURCE_DESC desc{};
			desc.Alignment = 0;
			desc.DepthOrArraySize = 1;
			desc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
			desc.Flags = D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET;
			desc.Format = main_buffer_format;
			desc.Width = size.x;
			desc.Height = size.y;
			desc.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
			desc.MipLevels = 0;
			desc.SampleDesc = { 1, 0 };

			{
				D3D12TextureInitInfo info{};
				info.desc = &desc;
				info.initial_state = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
				info.clear_value.Format = desc.Format;
				memcpy(&info.clear_value.Color, &clear_value[0], sizeof(clear_value));
				gpass_main_buffer = D3D12RenderTexture{ info };
			}

			desc.Flags = D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;
			desc.Format = depth_buffer_format;
			desc.MipLevels = 1;

			{
				D3D12TextureInitInfo info{};
				info.desc = &desc;
				info.initial_state = D3D12_RESOURCE_STATE_DEPTH_READ | D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE | D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE;
				info.clear_value.Format = desc.Format;
				info.clear_value.DepthStencil.Depth = 0.f;
				info.clear_value.DepthStencil.Stencil = 1;

				gpass_depth_buffer = D3D12DepthBuffer{ info };

				NAME_D3D12_OBJECT(gpass_main_buffer.resource(), L"GPass Main Buffer");
				NAME_D3D12_OBJECT(gpass_depth_buffer.resource(), L"GPass Depth Buffer");
			}

			return gpass_main_buffer.resource() && gpass_depth_buffer.resource();
		}

		bool create_gpass_root_signature() {
			assert(!gpass_root_sig && !gpass_pso);

			d3dx::D3D12RootParameter parameters[1]{};
			parameters[0].as_constants(1, D3D12_SHADER_VISIBILITY_PIXEL, 1);
			const d3dx::D3D12RootSignatureDesc root_signature{ &parameters[0], _countof(parameters) };
			gpass_root_sig = root_signature.create();
			assert(gpass_root_sig);
			NAME_D3D12_OBJECT(gpass_root_sig, L"GPass Root Signature");

			struct {
				d3dx::d3d12_pipeline_state_subobject_root_signature root_signature{ gpass_root_sig };
				d3dx::d3d12_pipeline_state_subobject_vs vs{ shaders::get_engine_shader(shaders::EngineShader::FULLSCREEN_TRIANGLE_VS) };
				d3dx::d3d12_pipeline_state_subobject_ps ps{ shaders::get_engine_shader(shaders::EngineShader::FILL_COLOR_PS) };
				d3dx::d3d12_pipeline_state_subobject_primitive_topology primitive_topology{ D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE };
				d3dx::d3d12_pipeline_state_subobject_render_target_formats render_target_formats;
				d3dx::d3d12_pipeline_state_subobject_depth_stencil_format depth_stencil_format{ depth_buffer_format };
				d3dx::d3d12_pipeline_state_subobject_rasterizer rasterizer{ d3dx::rasterizer_state.no_cull };
				d3dx::d3d12_pipeline_state_subobject_depth_stencil1 depth{ d3dx::depth_state.disabled };
			} stream;

			D3D12_RT_FORMAT_ARRAY  rtf_array{};
			rtf_array.NumRenderTargets = 1;
			rtf_array.RTFormats[0] = main_buffer_format;

			stream.render_target_formats = rtf_array;

			gpass_pso = d3dx::create_pipeline_state(&stream, sizeof(stream));
			NAME_D3D12_OBJECT(gpass_pso, L"GPass Pipeline State Object");

			return gpass_root_sig && gpass_pso;
		}
	}

	bool initialize() {
		return create_buffers(initial_dimensions) && create_gpass_root_signature();
	}

	void shutdown() {
		gpass_main_buffer.release();
		gpass_depth_buffer.release();

		dimensions = initial_dimensions;

		core::release(gpass_root_sig);
		core::release(gpass_pso);
	}

	void set_size(math::u32v2 size) {
		math::u32v2 d{ dimensions };
		if (size.x > d.x || size.y > d.y) {
			d = { std::max(size.x, d.x), std::max(size.y, d.y) };
			create_buffers(d);
		}
	}

	void depth_prepass(id3d12_graphics_command_lsit* cmd_list, const D3D12FrameInfo& info) {

	}

	void render(id3d12_graphics_command_lsit* cmd_list, const D3D12FrameInfo& info) {
		cmd_list->SetComputeRootSignature(gpass_root_sig);
		cmd_list->SetPipelineState(gpass_pso);
	}
}