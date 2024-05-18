#pragma once

#include "CommonHeaders.h"
#include "..\Platform\Window.h"

namespace lightning::graphics {
	
	class Surface {};

	struct RenderSurface {
		platform::Window window{};
		Surface surface{};
	};

	enum class GraphicsPlatform : u32 {
		DIRECT3D12 = 0,
		VULKAN = 1,
		OPEN_GL,
	};

	bool initialize(GraphicsPlatform platform);
	void shutdown();
}