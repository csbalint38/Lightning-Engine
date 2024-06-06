#pragma once

#include "CommonHeaders.h"
#include "..\Platform\Window.h"

namespace lightning::graphics {

	DEFINE_TYPED_ID(surface_id);
	
	class Surface {
		private:
			surface_id _id{ id::invalid_id };

		public:
			constexpr explicit Surface(surface_id id) : _id{ id } {}
			constexpr Surface() = default;
			constexpr surface_id get_id() const { return _id; }
			constexpr bool is_valid() const { return id::is_valid(_id); }

			void resize(u32 width, u32 height) const;
			u32 width() const;
			u32 height() const;
			void render() const;
	};

	struct RenderSurface {
		platform::Window window{};
		Surface surface{};
	};

	enum class GraphicsPlatform : u32 {
		DIRECT3D12 = 0,
		VULKAN = 1,
		OPEN_GL = 2,
	};

	bool initialize(GraphicsPlatform platform);
	void shutdown();

	const char* get_engine_shaders_path();
	const char* get_engine_shaders_path(graphics::GraphicsPlatform platform);

	Surface create_surface(platform::Window window);
	void remove_surface(surface_id id);

	id::id_type add_submesh(const u8*& data);
	void remove_submesh(id::id_type id);
}