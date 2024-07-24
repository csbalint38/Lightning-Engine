#include "ToolsCommon.h"
#include "Content/ContentToEngine.h"
#include "Utilities/IOStream.h"
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
				FORMAT_MISMATCH,
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

		constexpr void set_or_clear_flags(u32& flags, u32 flag, bool set) {
			if (set) flags |= flag;
			else flags &= ~flag;
		}

		constexpr u32 get_max_mip_count(u32 width, u32 height, u32 depth) {
			u32 mip_levels{ 1 };

			while (width > 1 || height > 1 || depth > 1) {
				width >>= 1;
				height >>= 1;
				depth >>= 1;

				++mip_levels;
			}

			return mip_levels;
		}

		void texture_info_from_metadata(const TexMetadata& metadata, TextureInfo& info) {
			using namespace lightning::content;

			const DXGI_FORMAT format{ metadata.format };
			info.format = format;
			info.width = (u32)metadata.width;
			info.height = (u32)metadata.height;
			info.array_size = metadata.IsVolumemap() ? (u32)metadata.depth : (u32)metadata.arraySize;
			info.mip_levels = (u32)metadata.mipLevels;
			set_or_clear_flags(info.flags, TextureFlags::HAS_ALPHA, HasAlpha(format));
			set_or_clear_flags(info.flags, TextureFlags::IS_HDR, format == DXGI_FORMAT_BC6H_UF16 || format == DXGI_FORMAT_BC6H_SF16);
			set_or_clear_flags(info.flags, TextureFlags::IS_CUBE_MAP, metadata.IsCubemap());
			set_or_clear_flags(info.flags, TextureFlags::IS_VOLUME_MAP, metadata.IsVolumemap());
		}

		void copy_subresources(const ScratchImage& scratch, TextureData* const data) {
			const TexMetadata& metadata{ scratch.GetMetadata() };
			const Image* const images{ scratch.GetImages() };
			const u32 image_count{ (u32)scratch.GetImageCount() };
			assert(images && metadata.mipLevels && metadata.mipLevels <= TextureData::max_mips);

			u64 subresource_size{ 0 };

			for (u32 i{ 0 }; i < image_count; ++i) {
				subresource_size += (u32)(sizeof(u32) * 4 + images[i].slicePitch);
			}

			if (subresource_size > ~(u32)0) {
				// Support up to 4GB per resource
				data->info.import_error = ImportError::MAX_SIZE_EXCEEDED;
				return;
			}

			data->subresource_size = (u32)subresource_size;
			data->subresource_data = (u8* const)CoTaskMemRealloc(data->subresource_data, subresource_size);
			assert(data->subresource_data);

			util::BlobStreamWriter blob{ data->subresource_data, data->subresource_size };

			for (u32 i{ 0 }; i < image_count; ++i) {
				const Image& image{ images[i] };
				blob.write((u32)image.width);
				blob.write((u32)image.height);
				blob.write((u32)image.rowPitch);
				blob.write((u32)image.slicePitch);
				blob.write(image.pixels, image.slicePitch);
			}
		}

		void copy_icon(const ScratchImage& scratch, TextureData* const data) {
			const Image* const images{ scratch.GetImages() };
			const u32 image_count{ (u32)scratch.GetImageCount() };
			assert(images && image_count);

			const Image& image{ images[0] };
			data->icon_size = (u32)(sizeof(u32) * 4 + image.slicePitch);
			data->icon = (u8* const)CoTaskMemRealloc(data->icon, data->icon_size);
			assert(data->icon);
			util::BlobStreamWriter blob{ data->icon, data->icon_size };
			blob.write((u32)image.width);
			blob.write((u32)image.height);
			blob.write((u32)image.rowPitch);
			blob.write((u32)image.slicePitch);
			blob.write(image.pixels, image.slicePitch);
		}

		[[nodiscard]] ScratchImage load_from_file(TextureData* const data, const char* file_name) {
			using namespace lightning::content;

			assert(file_exists(file_name));
			ScratchImage scratch;

			if (!file_exists(file_name)) {
				data->info.import_error = ImportError::FILE_NOT_FOUND;
				return scratch;
			}

			data->info.import_error = ImportError::LOAD;

			WIC_FLAGS wic_flags{ WIC_FLAGS_NONE };
			TGA_FLAGS tga_flags{ TGA_FLAGS_NONE };

			if (data->import_settings.output_format == DXGI_FORMAT_BC4_UNORM || data->import_settings.output_format == DXGI_FORMAT_BC5_UNORM) {
				wic_flags |= WIC_FLAGS_IGNORE_SRGB;
				tga_flags |= TGA_FLAGS_IGNORE_SRGB;
			}

			const std::wstring w_file{ to_wstring(file_name) };
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

		[[nodiscard]] ScratchImage initialize_from_images(TextureData* const data, const util::vector<Image>& images) {
			assert(data);
			const TextureImportSettings& settings{ data->import_settings };

			ScratchImage scratch;
			HRESULT hr{ S_OK };
			const u32 array_size{ (u32)images.size() };

			{
				ScratchImage working_scratch{};

				if (settings.dimension == TextureDimension::TEXTURE_1D || settings.dimension == TextureDimension::TEXTURE_2D) {
					const bool allow_1d{ settings.dimension == TextureDimension::TEXTURE_1D };

					if (array_size > 1) {
						hr = working_scratch.InitializeArrayFromImages(images.data(), images.size(), allow_1d);
					}
					else {
						assert(array_size == 1 && images.size() == 1);
						hr = working_scratch.InitializeFromImage(images[0], allow_1d);
					}
				}
				else if (settings.dimension == TextureDimension::TEXTURE_CUBE) {
					assert(array_size % 6 == 0);
					hr = working_scratch.InitializeCubeFromImages(images.data(), images.size());
				}
				else {
					assert(settings.dimension == TextureDimension::TEXTURE_3D);
					hr = working_scratch.Initialize3DFromImages(images.data(), images.size());
				}

				if (FAILED(hr)) {
					data->info.import_error = ImportError::UNKNOWN;
					return{};
				}

				scratch = std::move(working_scratch);
			}

			if (settings.mip_levels != 1) {
				ScratchImage mip_scratch;
				const TexMetadata& metadata{ scratch.GetMetadata() };
				u32 mip_levels{ math::clamp(settings.mip_levels, (u32)0, get_max_mip_count((u32)metadata.width, (u32)metadata.height, (u32)metadata.depth)) };

				if (settings.dimension != TextureDimension::TEXTURE_3D) {
					hr = GenerateMipMaps(scratch.GetImages(), scratch.GetImageCount(), scratch.GetMetadata(), TEX_FILTER_DEFAULT, mip_levels, mip_scratch);
				}
				else {
					hr = GenerateMipMaps3D(scratch.GetImages(), scratch.GetImageCount(), scratch.GetMetadata(), TEX_FILTER_DEFAULT, mip_levels, mip_scratch);
				}

				if (FAILED(hr)) {
					data->info.import_error = ImportError::MIPMAP_GENERATION;
					return{};
				}

				scratch = std::move(mip_scratch);
			}

			return scratch;
		}
	}

	EDITOR_INTERFACE void decompress_mipmaps(TextureData* const data);

	EDITOR_INTERFACE void import(TextureData* const data) {
		const TextureImportSettings& settings{ data->import_settings };
		assert(settings.sources && settings.source_count);

		util::vector<ScratchImage> scratch_images;
		util::vector<Image> images;

		u32 width{ 0 };
		u32 height{ 0 };
		DXGI_FORMAT format{};
		util::vector<std::string> files = split(settings.sources, ';');
		assert(files.size() == settings.source_count);

		for (u32 i{ 0 }; i < settings.source_count; ++i) {
			scratch_images.emplace_back(load_from_file(data, files[i].c_str()));
			if (data->info.import_error) return;

			const ScratchImage& scratch{ scratch_images.back() };
			const TexMetadata& metadata{ scratch.GetMetadata() };

			if (i == 0) {
				width = (u32)metadata.width;
				height = (u32)metadata.height;
				format = metadata.format;
			}

			if (width != metadata.width || height != metadata.height) {
				data->info.import_error = ImportError::SIZE_MISMATCH;
				return;
			}

			if (format != metadata.format) {
				data->info.import_error = ImportError::FORMAT_MISMATCH;
				return;
			}

			const u32 array_size{ (u32)metadata.arraySize };
			const u32 depth{ (u32)metadata.depth };

			for (u32 array_index{ 0 }; array_index < array_size; ++array_index) {
				for (u32 depth_index{ 0 }; depth_index < depth; ++depth_index) {
					const Image* image{ scratch.GetImage(0, array_index, depth_index) };
					assert(image);

					if (!image) {
						data->info.import_error = ImportError::UNKNOWN;
						return;
					}

					if (width != image->width || height != image->height) {
						data->info.import_error = ImportError::SIZE_MISMATCH;
						return;
					}

					images.emplace_back(*image);
				}
			}
		}

		ScratchImage scratch{ initialize_from_images(data, images) };

		if (data->info.import_error) return;

		if (settings.compress) {
			copy_icon(scratch, data);
			ScratchImage bc_scratch{};

			if (data->info.import_error) return;

			scratch = std::move(bc_scratch);
		}

		copy_subresources(scratch, data);
		texture_info_from_metadata(scratch.GetMetadata(), data->info);
	}
}