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

	struct PrimitiveTopology {
		enum Type : u32 {
			POINT_LIST = 1,
			LINE_LIST,
			LINE_STRIP,
			TRIANGLE_LIST,
			TRIANGLE_STRIP,

			count
		};
	};

	id::id_type create_resource(const void* const data, AssetType::Type type);
	void destroy_resource(id::id_type id, AssetType::Type type);
}
