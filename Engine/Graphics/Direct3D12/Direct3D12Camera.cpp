#include "Direct3D12Camera.h"

namespace lightning::graphics::direct3d12::camera {
	namespace {

	}

	D3D12Camera::D3D12Camera(CameraInitInfo info) : _up{ DirectX::XMLoadFloat3(&info.up) }, _near_z{ info.near_z }, _far_z{ info.far_z }, _field_of_view{ info.field_of_view }, _aspect_ratio{ info.aspect_ratio }, _projection_type{ info.type }, _entity_id{ info.entity_id }, _is_dirty{ true } {
		assert(id::is_valid(_entity_id));
		update();
	}

	void D3D12Camera::update() {

	}

	void D3D12Camera::up(math::v3 up) {
		_up = DirectX::XMLoadFloat3(&up);
	}

	void D3D12Camera::field_of_view(f32 fov) {
		assert(_projection_type == graphics::Camera::PERSPECTIVE);
		_field_of_view = fov;
		_is_dirty = true;
	}

	void D3D12Camera::aspect_ratio(f32 aspect_ratio) {
		assert(_projection_type == graphics::Camera::PERSPECTIVE);
		_aspect_ratio = aspect_ratio;
		_is_dirty = true;
	}

	void D3D12Camera::view_width(f32 width) {
		assert(width);
		assert(_projection_type == graphics::Camera::ORTOGRAPHIC);
		_view_width = width;
		_is_dirty = true;
	}

	void D3D12Camera::view_height(f32 height) {
		assert(height);
		assert(_projection_type == graphics::Camera::ORTOGRAPHIC);
		_view_height = height;
		_is_dirty = true;
	}

	void D3D12Camera::near_z(f32 near_z) {
		_near_z = near_z;
		_is_dirty = true;
	}

	void D3D12Camera::far_z(f32 far_z) {
		_far_z = far_z;
		_is_dirty = true;
	}

}