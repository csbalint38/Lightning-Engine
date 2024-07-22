#include "ToolsCommon.h"
#include "Content/ContentToEngine.h"
#include <directXTex.h>

using namespace DirectX;

namespace lightning::tools {
	namespace {

		struct ImportError {
			enum ErrorCode {
				SUCCEEDED = 0,
				UNKNOWN,
				COMPRESS,
				DECOMPRESS,
				LOAD,
				MIPMAP_GENERATION,
				MAX_SIZE_EXCEEDED,
				SIZE_MISMATCH,
				FILE_NOT_FOUND
			};
		};

		struct TextureDimension {
			enum Dimension : u32 {
				TEXTURE_1D,
				TEXTURE_2D,
				TEXTURE_3D,
				TEXTURE_CUBE
			};
		};

		struct TextureImportSettings {
			char* sources;
			u32 source_count;
			u32 dimension;
			u32 mip_levels;
			u32 array_size;
			f32 alpha_threshold;
			u32 prefer_bc7;
			u32 output_format;
			u32 compress;
		};

		struct TextureInfo {
			u32 width;
			u32 height;
			u32 array_size;
			u32 mip_levels;
			u32 format;
			u32 import_error;
			u32 flags;
		};

		struct TextureData {
			constexpr static u32 max_mips{ 14 }; // 8k textures;
			u8* subresource_data;
			u32 subresource_size;
			u8* icon;
			u32 icon_size;
			TextureInfo info;
			TextureImportSettings import_settings;
		};

		[[nodiscard]] ScratchImage load_from_file(TextureData* const data, const char* file_name) {
			using namespace lightning::content;

			assert(file_exists(file_name));
			ScratchImage scratch;

			if (!file_exists(file_name)) {
				data->import_error = ImportError::FILE_NOT_FOUND;
				return scratch;
			}

			data->info.import_error = ImportError::LOAD;

			WIC_FLAGS wic_flags{ WIC_FLAGS_NONE };
			TGA_FLAGS tga_flags{ TGA_FLAGS_NONE };

			if (data->import_settings.output_format == DXGI_FORMAT_BC4_UNORM || data->import_settings.output_format == DXGI_FORMAT_BC5_UNORM) {
				wic_flags |= WIC_FLAGS_IGNORE_SRGB;
				tga_flags |= TGA_FLAGS_IGNORE_SRGB;
			}

			const std::wstring wfile{ to_wstring(file_name) };
			const wchar_t* const file{ w_file.c_str() };

			wic_flags |= WIC_FLAGS_FORCE_RGB;
			HRESULT hr{ LoadFromWICFile(file, wic_flags, nullptr, scratch) };

			if (FAILED(hr)) {
				hr = LoadFromTGAFile(file, tga_flags, nullptr, scratch);
			}

			if (FAILED(hr)) {
				hr = LoadFromHDRFile(file, nullptr, scratch);
				if (SUCCEEDED(hr)) data->info.flags |= TextureFlags::IS_HDR;
			}

			if (FAILED(hr)) {
				hr = LoadFromDDSFile(file, DDS_FLAGS_FORCE_RGB, nullptr, scratch);

				if (SUCCEEDED(hr)) {
					data->info.import_error = ImportError::DECOMPRESS;
					ScratchImage mip_scratch;
					hr = Decompress(scratch.GetImages(), scratch.GetImageCount(), scratch.GetMetadata(), DXGI_FORMAT_UNKNOWN, mip_scratch);

					if (SUCCEEDED(hr)) {
						scratch = std::move(mip_scratch);
					}
				}
			}

			if (SUCCEEDED(hr)) {
				data->info.import_error = ImportError::SUCCEEDED;
			}

			return scratch;
		}
	}

	EDITOR_NTERFACE void decompress_mipmaps(TextureData* const data) {

	}

	EDITOR_INTERFACE void import(TextureData* const data) {
		const TextureImportSettings& settings{ data->import_settings };
		assert(settings.sources && settings.source_count);

		util::vector<ScratchImage> scratch_images;
		util::vectir<Image> images;

		u32 width{ 0 };
		u32 height{ 0 };
		DXGI_FORMAT format{};
		util::vector<std::string> files = split(settings.sources, ';');
		assert(files.size() == settings.source_count);

		for (u32 i{ 0 }; i < settings.source_count; ++i) {
			scratch_images.emplace_back(load_from_file(data, files[i].c_str()));
			if (data->info.import_error) return;
		}
	}
}