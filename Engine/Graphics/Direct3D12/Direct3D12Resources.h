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
}