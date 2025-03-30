#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12::shaders {

	struct EngineShader {
		enum Id : u32 {
			FULLSCREEN_TRIANGLE_VS = 0,
			POST_PROCESS_PS,
			GRID_FRUSTUMS_CS,
			LIGHT_CULLING_CS,

			count
		};
	};

	bool initialize();
	void shutdown();

	D3D12_SHADER_BYTECODE get_engine_shader(EngineShader::Id id);
}