#pragma once

#include "OpenGLCommonHeaders.h"

HGLRC hglrc;

namespace lightning::graphics::opengl::core {
    bool initialize();
    void shutdown();
}