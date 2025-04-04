#pragma once
#include <thread>
#include <chrono>
#include <string>

#define TEST_ENTITY_COMPONENTS 0
#define TEST_WINDOW 1
#define TEST_RENDERER 0

class Test {
	public:
		#ifdef _WIN_64
		virtual bool initialize() = 0;
		virtual void run() = 0;
		#elif __linux__
		virtual bool Initialize(void* disp) = 0;
		virtual void Run(void* disp) = 0;
		#endif
		virtual void shutdown() = 0;
};

#ifdef _WIN64
#include <Windows.h>
#else
#include <iostream>
#endif

class TimeIt {
	public:
		using clock = std::chrono::high_resolution_clock;
		using timestamp = std::chrono::steady_clock::time_point;

		constexpr float dt_avg() const { return _dt_avg * 1e-6f; }

		void begin() {
			_start = clock::now();
		}

		void end() {
			auto dt = clock::now() - _start;
			_us_avg += ((float)std::chrono::duration_cast<std::chrono::microseconds>(dt).count() - _us_avg) / (float)_counter;
			++_counter;
			_dt_avg = _us_avg;

			if (std::chrono::duration_cast<std::chrono::seconds>(clock::now() - _seconds).count() >= 1) {
				#ifdef _WIN64
				OutputDebugStringA("Avg. frame (ms): ");
				OutputDebugStringA(std::to_string(_us_avg * 0.001f).c_str());
				OutputDebugStringA((" " + std::to_string(_counter)).c_str());
				OutputDebugStringA(" fps");
				OutputDebugStringA("\n");
				#else
				std::cout << "Avg. frame (ms): ";
				std::cout << std::to_string(_us_avg).c_str();
				std::cout << (" " + std::to_string(_counter)).c_str();
				std::cout << " fps" << std::endl;
				#endif

				_us_avg = 0.f;
				_counter = 1;
				_seconds = clock::now();
			}
		}


	private:
		float _dt_avg{ 16.7f };
		float _us_avg{ 0.f };
		int _counter{ 1 };
		timestamp _start;
		timestamp _seconds{ clock::now() };
};
