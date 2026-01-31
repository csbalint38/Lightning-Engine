#pragma once

#include "..\Components\ComponentsCommonHeaders.h"

namespace lightning::transform {
	DEFINE_TYPED_ID(transform_id);

	#undef ABSOLUTE

	struct Space {
		enum Frame : u32 {
			ABSOLUTE,
			LOCAL,
			WORLD
		};
	};

	class Component final {
		private:
			transform_id _id;
		public:
			constexpr explicit Component(transform_id id) : _id{ id } {}
			constexpr Component() : _id{ id::invalid_id } {}
			constexpr transform_id get_id() const { return _id; }
			constexpr bool is_valid() const { return id::is_valid(_id); }

			math::v4 rotation() const;
			math::v3 position() const;
			math::v3 scale() const;
			math::v3 right() const;
			math::v3 up() const;
			math::v3 front() const;
			math::m3x3 local_frame() const;
			DirectX::XMVECTOR calculate_local_position(math::v3 delta) const;
			DirectX::XMVECTOR calculate_absolute_rotation(math::v3 rotation) const;
			DirectX::XMVECTOR calculate_local_rotation(math::v3 delta) const;
			DirectX::XMVECTOR calculate_world_rotation(math::v3 delta) const;
	};
}