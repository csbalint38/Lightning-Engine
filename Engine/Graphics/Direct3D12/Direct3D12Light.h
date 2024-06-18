#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12::light {
	graphics::Light create(LightInitInfo info);
	void remove(light_id id, u64 light_set_key);
	void set_parameter(light_id id, u64 light_set_key, LightParameter::Parameter parameter, const void* const data, u32 data_size);
	void get_parameter(light_id id, u64 light_set_key, LightParameter::Parameter parameter, void* const data, u32 data_size);
}