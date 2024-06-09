#pragma once

#include "CommonHeaders.h"
#include "Platform/Window.h"
#include "EngineAPI/Camera.h"

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

	struct CameraParameter {
		enum Parameter : u32 {
			UP_VECTOR,
			FIELD_OF_VIEW,
			ASPECT_RATIO,
			VIEW_WIDTH,
			VIEW_HEIGHT,
			NEAR_Z,
			FAR_Z,
			VIEW,
			PROJECTION,
			INVERSE_PROJECTION,
			VIEW_PROJECTION,
			INVERSE_VIEW_PROJECTION,
			TYPE,
			ENTITY_ID,

			count
		};
	};

	struct CameraInitInfo {
		id::id_type entity_id{ id::invalid_id };
		Camera::Type type{};
		math::v3 up;
		union {
			f32 field_of_view;
			f32 view_width;
		};
		union {
			f32 aspect_ratio;
			f32 view_height;
		};
		f32 near_z;
		f32 far_z;
	};

	struct PerspectiveCameraInitInfo : public CameraInitInfo {
		explicit PerspectiveCameraInitInfo(id::id_type id) {
			assert(id::is_valid(id));
			entity_id = id;
			type = Camera::PERSPECTIVE;
			up = { 0.f, 1.f, 0.f };
			field_of_view = .25f;
			aspect_ratio = 16.f / 10.f;
			near_z = 0.001;
			far_z = 10000.f;
		}
	};

	struct OrtographicCameraInitInfo : public CameraInitInfo {
		explicit OrtographicCameraInitInfo(id::id_type id) {
			assert(id::is_valid(id));
			entity_id = id;
			type = Camera::ORTOGRAPHIC;
			up = { 0.f, 1.f, 0.f };
			view_width = 1920.f;
			view_height = 1080;
			near_z = 0.001;
			far_z = 10000.f;
		}
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

	Camera create_camera(CameraInitInfo info);
	void remove_camera(camera_id id);

	id::id_type add_submesh(const u8*& data);
	void remove_submesh(id::id_type id);
}