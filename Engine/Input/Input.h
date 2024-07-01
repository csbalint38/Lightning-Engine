#pragma once

#include "CommonHeaders.h"
#include "EngineAPI/Input.h"

namespace lightning::input {
	void set(InputSource::Type type, InputCode::Code code, math::v3 value);
}