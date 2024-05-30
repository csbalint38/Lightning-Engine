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

	void add_transitions_for_depth_prepass(d3dx::D3D12ResourceBarrier& barriers);
	void add_transitions_for_gpass(d3dx::D3D12ResourceBarrier& barriers);
	void add_transitions_for_post_process(d3dx::D3D12ResourceBarrier& barriers);

	void set_render_targets_for_depth_prepass(id3d12_graphics_command_lsit* cmd_list);
	void set_render_targets_for_gpass(id3d12_graphics_command_lsit* cmd_list);
}