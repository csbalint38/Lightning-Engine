#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12::content {

	bool initialize();
	void shutdown();

	namespace submesh {
		id::id_type add(const u8*& data);
		void remove(id::id_type id);
	}

	namespace texture {
		id::id_type add(const u8* const);
		void remove(id::id_type);
		void get_descriptor_indicies(const id::id_type* const texture_ids, u32 id_count, u32* const indicies);
	}

	namespace material {
		id::id_type add(MaterialInitInfo info);
		void remove(id::id_type id);
	}
}