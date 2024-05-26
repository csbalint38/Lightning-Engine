#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12 {

	class DescriptorHeap;

	struct DescriptorHandle {
		D3D12_CPU_DESCRIPTOR_HANDLE cpu{};
		D3D12_GPU_DESCRIPTOR_HANDLE gpu{};

		constexpr bool is_valid() const { return cpu.ptr != 0; }
		constexpr bool is_shader_visible() const { return gpu.ptr != 0; }

		#ifdef _DEBUG
		private:
			friend class DescriptorHeap;
			DescriptorHeap* container{ nullptr };
			u32 index{ u32_invalid_id };
		#endif
	};

	class DescriptorHeap {
		public:
			explicit DescriptorHeap(D3D12_DESCRIPTOR_HEAP_TYPE type) : _type{ type } {};
			DISABLE_COPY_AND_MOVE(DescriptorHeap);
			~DescriptorHeap() { assert(!_heap); }

			bool initialize(u32 capacity, bool is_shader_visible);
			void release();
			void process_deferred_free(u32 frame_ids);

			[[nodiscard]] DescriptorHandle allocate();
			void free(DescriptorHandle& handle);

			constexpr D3D12_DESCRIPTOR_HEAP_TYPE type() const { return _type; };
			constexpr D3D12_CPU_DESCRIPTOR_HANDLE cpu_start() const { return _cpu_start; };
			constexpr D3D12_GPU_DESCRIPTOR_HANDLE gpu_start() const { return _gpu_start; };
			constexpr ID3D12DescriptorHeap* heap() const { return _heap; };
			constexpr u32 capacity() const { return _capacity; }
			constexpr u32 size() const { return _size; }
			constexpr u32 descriptor_size() const { return _descriptor_size; }
			constexpr bool is_shader_visible() const { return _gpu_start.ptr != 0; }

		private:
			ID3D12DescriptorHeap* _heap;
			D3D12_CPU_DESCRIPTOR_HANDLE _cpu_start{};
			D3D12_GPU_DESCRIPTOR_HANDLE _gpu_start{};
			std::unique_ptr<u32[]> _free_handles{};
			util::vector<u32> _deferred_free_indicies[FRAME_BUFFER_COUNT]{};
			std::mutex _mutex{};
			u32 _capacity{ 0 };
			u32 _size{ 0 };
			u32 _descriptor_size{};
			const D3D12_DESCRIPTOR_HEAP_TYPE _type{};
	};

	struct D3D12TextureInitInfo {
		ID3D12Heap1* heap{ nullptr };
		ID3D12Resource2* resource{ nullptr };
		D3D12_SHADER_RESOURCE_VIEW_DESC* srv_desc{ nullptr };
		D3D12_RESOURCE_DESC1* desc{ nullptr };
		D3D12_RESOURCE_ALLOCATION_INFO1 allocation_info{};
		D3D12_BARRIER_LAYOUT initial_state{};
		D3D12_CLEAR_VALUE clear_value{};
		DXGI_FORMAT format[1];
	};

	class D3D12Texture {
		public:
			D3D12Texture() = default;
			explicit D3D12Texture(D3D12TextureInitInfo info);
			DISABLE_COPY(D3D12Texture);
			constexpr D3D12Texture(D3D12Texture&& o) : _resource{ o._resource }, _srv{ o._srv } {
				o.reset();
			}

			constexpr D3D12Texture& operator=(D3D12Texture&& o) {
				assert(this != &o);
				if (this != &o) {
					release();
					move(o);
				}
				return *this;
			}

			void release();
			constexpr ID3D12Resource2* const resource() const { return _resource; }
			constexpr DescriptorHandle srv() const { return _srv; }

		private:
			ID3D12Resource2* _resource{ nullptr };
			DescriptorHandle _srv;

			constexpr void reset() {
				_resource = nullptr;
				_srv = {};
			}

			constexpr void move(D3D12Texture& o) {
				_resource = o._resource;
				_srv = o._srv;
				o.reset();
			}
	};
}