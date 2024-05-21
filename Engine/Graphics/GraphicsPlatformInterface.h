#pragma once
#include "CommonHeaders.h"
#include "Renderer.h"
#include "Platform/Window.h"

namespace lightning::graphics {
	struct PlatformInterface {
		bool(*initialize)(void);
		void(*shutdown)(void);

		struct {
			Surface(*create)(platform::Window);
			void (*remove)(surface_id);
			void (*resize)(surface_id, u32, u32);
			u32 (*width)(surface_id);
			u32 (*height)(surface_id);
			void (*render)(surface_id);
		} surface;
	};
}