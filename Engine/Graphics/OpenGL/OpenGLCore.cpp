#include "OpenGLCore.h"

namespace lightning::graphics::opengl::core {
    namespace {
        GLFWwindow* window;
    }

    bool initialize() {
        // Determine wihich adapter (i.e. graphics card) to use.
        // determine what is the maximum feature level that is supported
        // Create virtual adapter.
        return true;
    }

    void shutdown() {
        // Clean up OpenGL resources and context
        // This is a placeholder for actual OpenGL shutdown code
    }
}