#include "Direct3D12LightCulling.h"
#include "Direct3D12Core.h"
#include "Shaders/ShaderTypes.h"
#include "Direct3D12Shaders.h"
#include "Direct3D12Light.h"
#include "Direct3D12Camera.h"
#include "Direct3D12GPass.h"

namespace lightning::graphics::direct3d12::delight {
	namespace {
		struct LightCullingRootParameter {
			enum Parameter : u32 {
				GLOBAL_SHADER_DATA,
				CONSTANTS,
				FRUSTUMS_OUT_OR_INDEX_COUNTER,

				count
			};
		};

		struct CullingParameters {
			D3D12Buffer frustums;
			hlsl::LightCullingDispatchParameters grid_frustums_dispatch_params{};
			u32 frustum_count{ 0 };
			u32 view_width{ 0 };
			u32 view_height{ 0 };
			f32 camera_fov{ 0.f };
		};

		struct LightCuller {
			CullingParameters cullers[FRAME_BUFFER_COUNT]{};
		};

		ID3D12RootSignature* light_culling_root_signature{ nullptr };
		ID3D12PipelineState* grid_frustum_pso{ nullptr };
		util::free_list<LightCuller> light_cullers;


		bool create_root_signatures() {
			assert(!light_culling_root_signature);
			using param = LightCullingRootParameter;
			d3dx::D3D12RootParameter parameters[param::count]{};
			parameters[param::GLOBAL_SHADER_DATA].as_cbv(D3D12_SHADER_VISIBILITY_ALL, 0);
			parameters[param::CONSTANTS].as_cbv(D3D12_SHADER_VISIBILITY_ALL, 1);
			parameters[param::FRUSTUMS_OUT_OR_INDEX_COUNTER].as_uav(D3D12_SHADER_VISIBILITY_ALL, 0);

			light_culling_root_signature = d3dx::D3D12RootSignatureDesc{ &parameters[0], _countof(parameters) }.create();
			NAME_D3D12_OBJECT(light_culling_root_signature, L"Light Culling Root Signature");

			return light_culling_root_signature != nullptr;
		}

		bool create_psos() {
			assert(!grid_frustum_pso);

			struct {
				d3dx::d3d12_pipeline_state_subobject_root_signature root_signature{ light_culling_root_signature };
				d3dx::d3d12_pipeline_state_subobject_cs cs{ shaders::get_engine_shader(shaders::EngineShader::GRID_FRUSTUMS_CS) };
			} stream;

			grid_frustum_pso = d3dx::create_pipeline_state(&stream, sizeof(stream));
			NAME_D3D12_OBJECT(grid_frustum_pso, L"Grid Frustums PSO");

			return grid_frustum_pso != nullptr;
		}

		void resize_buffers(CullingParameters& culler) {
			const u32 frustum_count{ culler.frustum_count };
			const u32 frustum_buffer_size{ sizeof(hlsl::Frustum) * frustum_count };

			D3D12BufferInitInfo info{};
			info.alignment = sizeof(math::v4);
			info.flags = D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;

			if (frustum_buffer_size > culler.frustums.size()) {
				info.size = frustum_buffer_size;
				culler.frustums = D3D12Buffer{ info, false };
				NAME_D3D12_OBJECT_INDEXED(culler.frustums.buffer(), frustum_count, L"Light Grid Frustums Buffer - count");
			}
		}

		void resize(CullingParameters& culler) {
			constexpr u32 tile_size{ light_culling_tile_size };
			assert(culler.view_width >= tile_size && culler.view_height >= tile_size);
			const math::u32v2 tile_count{
				(u32)math::align_size_up<tile_size>(culler.view_width) / tile_size,
				(u32)math::align_size_up<tile_size>(culler.view_height) / tile_size,
			};

			culler.frustum_count = tile_count.x * tile_count.y;

			{
				hlsl::LightCullingDispatchParameters& params{ culler.grid_frustums_dispatch_params };
				params.num_threds = tile_count;
				params.num_thred_groups.x = (u32)math::align_size_up<tile_size>(tile_count.x) / tile_size;
				params.num_thred_groups.y = (u32)math::align_size_up<tile_size>(tile_count.y) / tile_size;
			}

			resize_buffers(culler);
		}

		void calculate_grid_frustums(CullingParameters& culler, id3d12_graphics_command_list* const cmd_list, const D3D12FrameInfo info, d3dx::D3D12ResourceBarrier& barriers) {
			ConstantBuffer& cbuffer{ core::c_buffer() };
			hlsl::LightCullingDispatchParameters* const buffer{ cbuffer.allocate<hlsl::LightCullingDispatchParameters>() };
			const hlsl::LightCullingDispatchParameters& params{ culler.grid_frustums_dispatch_params };
			memcpy(buffer, &params, sizeof(hlsl::LightCullingDispatchParameters));

			// TEMP
			barriers.add(culler.frustums.buffer(), D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE | D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE, D3D12_RESOURCE_STATE_UNORDERED_ACCESS);
			barriers.apply(cmd_list);

			using param = LightCullingRootParameter;
			cmd_list->SetComputeRootSignature(light_culling_root_signature);
			cmd_list->SetPipelineState(grid_frustum_pso);
			cmd_list->SetComputeRootConstantBufferView(param::GLOBAL_SHADER_DATA, info.global_shader_data);
			cmd_list->SetComputeRootConstantBufferView(param::CONSTANTS, cbuffer.gpu_address(buffer));
			cmd_list->SetComputeRootUnorderedAccessView(param::FRUSTUMS_OUT_OR_INDEX_COUNTER, culler.frustums.gpu_address());
			cmd_list->Dispatch(params.num_thred_groups.x, params.num_thred_groups.y, 1);

			// TEMP
			barriers.add(culler.frustums.buffer(), D3D12_RESOURCE_STATE_UNORDERED_ACCESS, D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE | D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE);
		}

		void _declspec(noinline) resize_and_calculate_grid_frustums(CullingParameters& culler, id3d12_graphics_command_list* const cmd_list, const D3D12FrameInfo info, d3dx::D3D12ResourceBarrier& barriers) {
			culler.camera_fov = info.camera->field_of_view();
			culler.view_width = info.surface_width;
			culler.view_height = info.surface_height;

			resize(culler);
			calculate_grid_frustums(culler, cmd_list, info, barriers);
		}
	}

	bool initialize() {
		return create_root_signatures() && create_psos() && light::initialize();
	}

	void shutdown() {
		light::shutdown();
		assert(light_culling_root_signature && grid_frustum_pso);
		core::deferred_release(light_culling_root_signature);
		core::deferred_release(grid_frustum_pso);
	}

	[[nodiscard]] id::id_type add_culler() { return light_cullers.add(); }

	void remove_culler(id::id_type id) {
		assert(id::is_valid(id));
		light_cullers.remove(id);
	}

	void cull_lights(id3d12_graphics_command_list* const cmd_list, const D3D12FrameInfo& info, d3dx::D3D12ResourceBarrier& barriers) {
		const id::id_type id{ info.light_culling_id };
		assert(id::is_valid(id));
		CullingParameters& culler{ light_cullers[id].cullers[info.frame_index] };

		if (info.surface_width != culler.view_width || info.surface_height != culler.view_height || !math::is_equal(info.camera->field_of_view(), culler.camera_fov)) {
			resize_and_calculate_grid_frustums(culler, cmd_list, info, barriers);
		}
		//barriers.apply(cmd_list);
	}

	// TEMP

	D3D12_GPU_VIRTUAL_ADDRESS frustums(id::id_type light_culling_id, u32 frame_index) {
		assert(frame_index < FRAME_BUFFER_COUNT && id::is_valid(light_culling_id));
		return light_cullers[light_culling_id].cullers[frame_index].frustums.gpu_address();
	}
}