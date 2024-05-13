#include <fstream>
#include "ContentLoader.h"

namespace lightning::content {
	bool load_game() {
		std::ifstream game("game.bin", std::ios::in | std::ios::binary);
		util::vector<u8> buffer(std::istreambuf_iterator<char>(game), {});
		assert(buffer.size());
		const u8* at{ buffer.data() };
		constexpr u32 su32{ sizeof(u32) };
		const u32 num_entities{ *at };
		at += su32;

	}

	void unload_game() {

	}
}