#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12 {
	class D3D12Surface {
		public:
			constexpr static u32 buffer_count{ 3 };
			constexpr static DXGI_FORMAT default_back_buffer_format{ DXGI_FORMAT_R8G8B8A8_UNORM_SRGB };

			explicit D3D12Surface(platform::Window window) : _window{ window } {
				assert(window.handle());
			}

			#if USE_STL_VECTOR
			DISABLE_COPY(D3D12Surface);
			constexpr D3D12Surface(D3D12Surface&& o) : _swap_chain{ o._swap_chain }, _window{ o._window }, _current_bb_index{ o._current_bb_index }, _viewport{ o._viewport }, _scissor_rect{ o._scissor_rect }, _allow_tearing{ o._allow_tearing }, _present_flags{ o._present_flags } {
				for (u32 i{ 0 }; i < FRAME_BUFFER_COUNT; ++i) {
					_render_target_data[i].resource = o._render_target_data[i].resource;
					_render_target_data[i].rtv = o._render_target_data[i].rtv;
				}
				o.reset();
				}

			constexpr D3D12Surface& operator=(D3D12Surface&& o) {
				assert(this != &o);
				if (this != &o) {
					release();
					move(o);
				}
				return *this;					
			}
			#else
			DISABLE_COPY_AND_MOVE(D3D12Surface)
			#endif

			~D3D12Surface() { release(); }

			void create_swap_chain(IDXGIFactory7* factory, ID3D12CommandQueue* cmd_queue, DXGI_FORMAT format = default_back_buffer_format);
			void present() const;
			void resize();
			constexpr u32 width() const { return (u32)_viewport.Width; }
			constexpr u32 height() const { return (u32)_viewport.Height; }
			constexpr ID3D12Resource* const back_buffer() const { return _render_target_data[_current_bb_index].resource; }
			constexpr D3D12_CPU_DESCRIPTOR_HANDLE rtv() const { return _render_target_data[_current_bb_index].rtv.cpu; }
			constexpr const D3D12_VIEWPORT& viewport() const { return _viewport; }
			constexpr const D3D12_RECT& scissor_rect() const { return _scissor_rect; }

		private:
			struct RenderTargetData {
				ID3D12Resource* resource{ nullptr };
				DescriptorHandle rtv{};
			};

			#if USE_STL_VECTOR
			constexpr void move(D3D12Surface& o) {
				_swap_chain = o._swap_chain;
				for (u32 i{ 0 }; i < FRAME_BUFFER_COUNT; ++i) _render_target_data[i] = o._render_target_data[i];
				_window = o._window;
				_current_bb_index = o._current_bb_index;
				_allow_tearing = o._allow_tearing;
				_present_flags = o._present_flags;
				_viewport = o._viewport;
				_scissor_rect = o._scissor_rect;

				o.reset();
			}

			constexpr void reset() {
				_swap_chain = nullptr;
				for (u32 i{ 0 }; i < FRAME_BUFFER_COUNT; ++i) _render_target_data[i] = {};
				_window = {};
				_current_bb_index = 0;
				_allow_tearing = 0;
				_present_flags = 0;
				_viewport = {};
				_scissor_rect = {};
			}
			#endif

			void finalize();
			void release();

			IDXGISwapChain4* _swap_chain{ nullptr };
			RenderTargetData _render_target_data[buffer_count]{};
			platform::Window _window{};
			DXGI_FORMAT _format{ default_back_buffer_format };
			mutable u32 _current_bb_index{ 0 };
			u32 _allow_tearing{ 0 };
			u32 _present_flags{ 0 };
			D3D12_VIEWPORT _viewport{};
			D3D12_RECT _scissor_rect{};
	};
}