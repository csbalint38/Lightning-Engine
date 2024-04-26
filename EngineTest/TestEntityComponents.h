#pragma once

#include "Test.h"
#include "..\Engine\Components\Entity.h"
#include "..\Engine\Components\Transform.h"

using namespace lightning;

class EngineTest : public Test {
public:
	bool initialize() override { return true; }
	void run() override { }
	void shutdown() override { }
};