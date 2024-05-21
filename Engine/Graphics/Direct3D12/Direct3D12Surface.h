#pragma once
#include "Direct3D12CommonHeaders.h"
#include "Direct3D12Resources.h"

namespace lightning::graphics::direct3d12 {
	class D3D12Surface {
		private:
			struct RenderTargetData {
				ID3D12Resource2* resource{ nullptr };
				DescriptorHandle rtv{};
			};

			IDXGISwapChain4* _swap_chain{ nullptr };
			RenderTargetData _render_target_data[FRAME_BUFFER_COUNT];
			platform::Window _window{};
			mutable u32 _current_bb_index{ 0 };
			D3D12_VIEWPORT _viewport{};
			D3D12_RECT _scissor_rect{};

			void release();
			void finalize();

		public:
			explicit D3D12Surface(platform::Window window) : _window{ window } {
				assert(window.handle());
			}

			~D3D12Surface() {
				release();
			}

			void create_swap_chain(IDXGIFactory7* factory, ID3D12CommandQueue* cmd_queue, DXGI_FORMAT format);
			void present() const;
			void resize();
			constexpr u32 width() const { return (u32)_viewport.Width; }
			constexpr u32 height() const { return (u32)_viewport.Height; }
			constexpr ID3D12Resource2* const back_buffer() const { return _render_target_data[_current_bb_index].resource; }
			constexpr D3D12_CPU_DESCRIPTOR_HANDLE rtv() const{ return _render_target_data[_current_bb_index].rtv.cpu; }
			constexpr const D3D12_VIEWPORT& viewport() const { return _viewport; }
			constexpr const D3D12_RECT& scissor_rect() const { return _scissor_rect; }
	};
}