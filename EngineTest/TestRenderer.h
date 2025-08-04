#pragma once
#include "Test.h"

// #define OPENGL

class EngineTest : public Test {
	public:
		bool initialize() override;
		void run() override;
		void shutdown() override;
};