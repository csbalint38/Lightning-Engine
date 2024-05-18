#pragma once
#include "CommonHeaders.h"
#include "Renderer.h"

namespace lightning::graphics {
	struct PlatformInterface {
		bool(*initialize)(void);
		void(*shutdown)(void);
	};
}