#if !defined(SHIPPING)

#include "..\Content\ContentLoader.h"
#include "..\Components\Script.h"
#include <thread>

bool engine_initialize() {
	bool result{ lightning::content::load_game() };
	return result;
}

void engine_update() {
	lightning::script::update(10.f);
	std::this_thread::sleep_for(std::chrono::milliseconds(10));
}

void engine_shutdown() {
	lightning::content::unload_game();
}
#endif