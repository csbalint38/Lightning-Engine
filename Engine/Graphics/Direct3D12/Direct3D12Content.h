#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12::content {
	namespace submesh {
		id::id_type add(const u8*& data);
		void remove(id::id_type id);
	}
}