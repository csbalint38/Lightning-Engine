#pragma once
#include "CommonHeaders.h"

namespace lightning::content {

	struct AssetType {
		enum Type : u32 {
			UNKNOWN = 0,
			ANIMATION,
			AUDIO,
			MATERIAL,
			MESH,
			SKELETON,
			TEXTURE,

			count
		};
	};

	typedef struct CompiledShader {
		static constexpr u32 hash_length{ 16 };
		constexpr u64 byte_code_size() const { return _byte_code_size; }
		constexpr const u8* const hash() const { return &_hash[0]; }
		constexpr const u8* const byte_code() const { return &_byte_code; }

		private:
			u64 _byte_code_size;
			u8 _hash[hash_length];
			u8 _byte_code;
	} const* compiled_shader_ptr;

	id::id_type create_resource(const void* const data, AssetType::Type type);
	void destroy_resource(id::id_type id, AssetType::Type type);

	id::id_type add_shader(const u8* data);
	void remove_shader(id::id_type id);
	compiled_shader_ptr get_shader(id::id_type id);
}
