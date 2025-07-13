#pragma once

namespace lightning::graphics {
    struct PlatformInterface;

    namespace opengl {
        void get_platform_interface(PlatformInterface& pi);
    }
}