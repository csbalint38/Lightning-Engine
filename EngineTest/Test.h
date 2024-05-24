#pragma once
#include <thread>
#include <chrono>
#include <string>

#define TEST_ENTITY_COMPONENTS 0
#define TEST_WINDOW 0
#define TEST_RENDERER 1

class Test {
	public:
		virtual bool initialize() = 0;
		virtual void run() = 0;
		virtual void shutdown() = 0;
};

#if _WIN64
#include <Windows.h>

class TimeIt {
	public:
		using clock = std::chrono::high_resolution_clock;
		using timestamp = std::chrono::steady_clock::time_point;

		void begin() {
			_start = clock::now();
		}

		void end() {
			auto dt = clock::now() - _start;
			_ms_avg += ((float)std::chrono::duration_cast<std::chrono::microseconds>(dt).count() - _ms_avg) / (float)_counter;
			++_counter;

			if (std::chrono::duration_cast<std::chrono::seconds>(clock::now() - _seconds).count() >= 1) {
				OutputDebugString("Avg. frame (ms): ");
				OutputDebugString(std::to_string(_ms_avg).c_str());
				OutputDebugString((" " + std::to_string(_counter)).c_str());
				OutputDebugString(" fps");
				OutputDebugString("\n");
				_ms_avg = 0.f;
				_counter = 1;
				_seconds = clock::now();
			}
		}


	private:
		float _ms_avg{ 0.f };
		int _counter{ 1 };
		timestamp _start;
		timestamp _seconds{ clock::now() };
};
#endif