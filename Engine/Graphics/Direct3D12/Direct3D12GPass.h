#pragma once

#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12 {
	struct D3D12FrameInfo;
}

namespace lightning::graphics::direct3d12::gpass {
	bool initialize();
	void shutdown();

	void set_size(math::u32v2);
	void depth_prepass(id3d12_graphics_command_lsit* cmd_list, const D3D12FrameInfo& info);
	void render(id3d12_graphics_command_lsit* cmd_list, const D3D12FrameInfo& info);
}