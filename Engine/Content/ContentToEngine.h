#pragma once
#include "CommonHeaders.h"

namespace lightning::content {
	struct PrimitiveTopology {
		enum Type : u32 {
			POINT_LIST = 1,
			LINE_LIST,
			LINE_STRIP,
			TRIANGLE_LIST,
			TRIANGLE_STRIP,

			count
		};
	};
}
