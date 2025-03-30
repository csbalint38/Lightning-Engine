#pragma once

#include "CommonHeaders.h"
#include "Graphics/Renderer.h"

struct ShaderFileInfo {
	const char* file_name;
	const char* function;
	lightning::graphics::ShaderType::Type type;
};

std::unique_ptr<u8[]> compile_shader(ShaderFileInfo info, u8* code, u32 code_size, lightning::util::vector<std::wstring>& extra_args, bool include_errors_and_disassembly = false);
std::unique_ptr<u8[]> compile_shader(ShaderFileInfo info, const char* file_path, lightning::util::vector<std::wstring>& extra_args, bool include_errors_and_disassembly = false);
bool compile_shaders();