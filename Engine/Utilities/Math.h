#pragma once

#include "CommonHeaders.h"

namespace lightning::math {
	template<typename T> constexpr T clamp(T value, T min, T max) {
		return (value < min) ? min : (value > max) ? max : value;
	}
}