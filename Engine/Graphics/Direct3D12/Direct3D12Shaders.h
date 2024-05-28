#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12::shaders {
	struct ShaderType {
		enum Type : u32 {
			VERTEX = 0,
			HULL,
			DOMAIN,
			GEOMETRY,
			PIXEL,
			COMPUTE,
			AMPLIFICATION,
			MESH,

			count
		};
	};

	struct EngineShader {
		enum Id : u32 {
			FULLSCREEN_TRIANGLE_VS = 0,

			count
		};
	};

	bool initialize();
	void shutdown();

	D3D12_SHADER_BYTECODE get_engine_shader(EngineShader::Id id);
}