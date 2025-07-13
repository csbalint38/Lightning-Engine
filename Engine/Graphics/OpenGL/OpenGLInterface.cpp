#include "CommonHeaders.h"
#include "OpenGLInterface.h"
#include "OpenGLCore.h"
#include "Graphics/GraphicsPlatformInterface.h"

namespace lightning::graphics::opengl {
    void get_platform_interface(PlatformInterface& pi) {
        pi.initialize = core::initialize;
        pi.shutdown = core::shutdown;
    }
}