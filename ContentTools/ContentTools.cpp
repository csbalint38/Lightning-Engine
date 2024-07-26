#include "ToolsCommon.h"

namespace lightning::tools {
	extern void shutdown_texture_tools();
}

EDITOR_INTERFACE void shutdown_content_tools() {
	using namespace lightning::tools;

	shutdownt_texture_tools();
}