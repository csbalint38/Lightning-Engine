#pragma once

#include <mutex>
#include <fstream>
#include <chrono>
#include <iomanip>

struct Logger {
	std::mutex mutex;
	std::ofstream file;

	static Logger& Get() {
		static Logger instance;

		return instance;
	}

	void Open(const std::wstring& path) {
		std::lock_guard<std::mutex> lock(mutex);

		file.open(path, std::ios::out | std::ios::trunc);
		file.setf(std::ios::unitbuf);
	}

	template<class... Args> void Log(const char* level, const char* fmt, Args&&... args) {
		char msg[2048];

		std::snprintf(msg, sizeof(msg), fmt, std::forward<Args>(args)...);

		auto now{ std::chrono::system_clock::now() };
		std::time_t t{ std::chrono::system_clock::to_time_t(now) };
		std::tm lt{};

		#ifdef _WIN32
		localtime_s(&lt, &t);
		#endif

		// TODO: Add non-Windows local time

		std::lock_guard<std::mutex> lock(mutex);

		file << std::put_time(&lt, "%H:%M:%S") << " [" << level << "] " << msg << "\n";
	}
};

#define LOG_INFO(...) Logger::Get().Log("INFO", __VA_ARGS__)
#define LOG_WARNING(...) Logger::Get().Log("WARNING", __VA_ARGS__)
#define LOG_ERROR(...) Logger::Get().Log("ERROR", __VA_ARGS__)
#define CHECK_HR(hr, what) do {									\
	HRESULT _hr = (hr);											\
	if (FAILED(_hr)) {											\
		LOG_ERROR("%s failed: 0x%08X", what, (unsigned)_hr);	\
	}															\
	else {														\
		LOG_INFO("%s OK", what);								\
	}															\
} while (0)
