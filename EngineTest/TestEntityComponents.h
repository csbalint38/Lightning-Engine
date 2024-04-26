#pragma once

#include "Test.h"

class EngineTest : public Test {
public:
	bool initialize() override { return true; }
	void run() override { }
	void shutdown() override { }
};