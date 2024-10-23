#include "ToolsCommon.h"
#include <DirectXTex.h>
#include <dxgi1_6.h>

using namespace DirectX;
using namespace Microsoft::WRL;

namespace lightning::tools {
	namespace {
		namespace shaders {
			//#include "EnvMapProcessing_EquirectangularToCubeMapCS.inc"
		};

		constexpr u32 prefiltered_diffuse_cubemap_size{ 64 };
		constexpr u32 prefiltered_specular_cubemap_size{ 256 };
		constexpr u32 roughness_mip_levels{ 6 };
		constexpr u32 brdf_integration_lut_size{ 256 };

		struct ShaderConstants {
			u32 cube_map_in_size;
			u32 cube_map_out_size;
			u32 sample_count;
			f32 roughness;
			
			private:
		};

		math::v3 get_sample_direction_equirectangular(u32 face, f32 u, f32 v) {
			math::v3 directions[6]{
				{ -u, 1.f, -v },	// x+ left
				{ u, -1.f, -v },	// x- right
				{ v, u, 1.f },		// y+ bottom
				{ -v, u, -1.f },	// y- top
				{ 1.f, u, -v },		// z+ front
				{ -1.f, -u, -v },	// z- back
			};

			XMVECTOR dir{ XMLoadFloat3(&directions[face]) };
			dir = XMVector3Normalize(dir);
			math::v3 normalized_dir;
			XMStoreFloat3(&normalized_dir, dir);

			return normalized_dir;
		}

		math::v2 direction_to_equirectangular(const math::v3& dir) {
			const f32 phi{ atan2f(dir.y, dir.x) };
			const f32 theta{ XMScalarACos(dir.z) };
			const f32 s{ phi * math::INV_TWO_PI + .5f };
			const f32 t{ theta * math::INV_PI };

			return { s, t };
		}

		void sample_cube_face(const Image& env_map, const Image& cube_face, u32 face_index, bool mirror) {
			assert(cube_face.width == cube_face.height);

			const f32 inv_width{ 1.f / (f32)cube_face.height };
			const f32 inv_height{ 1.f / (f32)cube_face.width };
			const u32 row_pitch{ (u32)cube_face.rowPitch };
			const u32 env_width{ (u32)env_map.width -1 };
			const u32 env_height{ (u32)env_map.height - 1 };
			const u32 env_row_pitch{ (u32)env_map.rowPitch };

			constexpr u32 bytes_per_pixel{ sizeof(f32) * 4 };

			for (u32 y{ 0 }; y < cube_face.height; ++y) {
				const f32 v{ (y * inv_height) * 2.f - 1.f };

				for (u32 x{ 0 }; x < cube_face.width; ++x) {
					const f32 u{ (x * inv_width) * 2.f - 1.f };
					const math::v3 sample_direction{ get_sample_direction_equirectangular(face_index, u, v) };
					math::v2 uv{ direction_to_equirectangular(sample_direction) };

					assert(uv.x >= 0.f && uv.x <= 1.f && uv.y >= 0.f && uv.y <= 1.f);

					if (mirror) uv.x = 1.f - uv.x;
					const f32 pos_x{ uv.x * env_width };
					const f32 pos_y{ uv.y * env_height };
					u8* dst_pixel{ &cube_face.pixels[row_pitch * y + x * bytes_per_pixel] };
					u8* const src_pixel{ &cube_face.pixels[row_pitch * y + x * bytes_per_pixel] };
					memcpy(dst_pixel, src_pixel, bytes_per_pixel);
				}
			}
		}
	}

	HRESULT equirectangular_to_cubemap(const Image* env_maps, u32 env_map_count, u32 cubemap_size, bool use_prefilter_size, bool mirror_cubemap, ScratchImage& cube_maps) {
		if (use_prefilter_size) {
			cubemap_size = prefiltered_specular_cubemap_size;
		}

		assert(env_maps && env_map_count);

		HRESULT hr{ S_OK };

		ScratchImage working_scratch{};
		hr = working_scratch.InitializeCube(DXGI_FORMAT_R32G32B32A32_FLOAT, cubemap_size, cubemap_size, env_map_count, 1);

		if (FAILED(hr)) {
			return hr;
		}

		for (u32 i{ 0 }; i < env_map_count; ++i) {
			const Image& env_map{ env_maps[i] };

			assert(math::is_equal((f32)env_map.width / (f32)env_map.height, 2.f));

			ScratchImage f32_env_map{};

			if (env_maps[0].format != DXGI_FORMAT_R32G32B32A32_FLOAT) {
				hr = Convert(env_map, DXGI_FORMAT_R32G32B32A32_FLOAT, TEX_FILTER_DEFAULT, TEX_THRESHOLD_DEFAULT, f32_env_map);

				if (FAILED(hr)) {
					return hr;
				}
			}
			else {
				f32_env_map.InitializeFromImage(env_map);
			}

			assert(f32_env_map.GetImageCount() == 1);

			const Image* dst_images{ &working_scratch.GetImages()[i * 6] };
			const Image& env_map_image{ f32_env_map.GetImages()[i] };
			const bool mirror{ mirror_cubemap };

			std::thread threads[]{
				std::thread { [&] { sample_cube_face(env_map_image, dst_images[0], 0, mirror); } },
				std::thread { [&] { sample_cube_face(env_map_image, dst_images[1], 1, mirror); } },
				std::thread { [&] { sample_cube_face(env_map_image, dst_images[2], 2, mirror); } },
				std::thread { [&] { sample_cube_face(env_map_image, dst_images[3], 3, mirror); } },
				std::thread { [&] { sample_cube_face(env_map_image, dst_images[4], 4, mirror); } },
			};

			sample_cube_face(f32_env_map.GetImages()[i], dst_images[5], 5, mirror);

			for (u32 face{ 0 }; face < 5; ++face) threads[face].join();
		}

		if (env_maps[0].format != DXGI_FORMAT_R32G32B32A32_FLOAT) {
			hr = Convert(working_scratch.GetImages(), working_scratch.GetImageCount(), working_scratch.GetMetadata(), env_maps[0].format, TEX_FILTER_DEFAULT, TEX_THRESHOLD_DEFAULT, cube_maps);
		}
		else {
			cube_maps = std::move(working_scratch);
		}

		return hr;
	}
}