#pragma once

#include "CommonHeaders.h"

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


struct ShaderFileInfo {
	const char* file_name;
	const char* function;
	ShaderType::Type type;
};

bool compile_shaders();