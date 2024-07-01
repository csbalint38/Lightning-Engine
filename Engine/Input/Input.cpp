#include "Input.h"

namespace lightning::input {
	namespace {

		std::unordered_map<u64, InputValue> input_values;
		util::vector<detail::InputSystemBase*> input_callbacks;

		constexpr u64 get_key(InputSource::Type type, u32 code) {
			return ((u64)type << 32) | (u64)code;
		} 
	}

	void set(InputSource::Type type, InputCode::Code code, math::v3 value) {
		assert(type < InputSource::count);
		const u64 key{ get_key(type, code) };
		InputValue& input{ input_values[key] };
		input.previous = input.current;
		input.current = value;

		for (const auto& c : input_callbacks) {
			c->on_event(type, code, input);
		}
	}

	void get(InputSource::Type type, InputCode::Code code, InputValue& value) {
		assert(type < InputSource::count);
		const u64 key{ get_key(type, code) };
		value = input_values[key];
	}

	detail::InputSystemBase::InputSystemBase() {
		input_callbacks.emplace_back(this);
	}

	detail::InputSystemBase::~InputSystemBase() {
		for (u32 i{ 0 }; i < input_callbacks.size(); ++i) {
			if (input_callbacks[i] == this) {
				util::erease_unordered(input_callbacks, i);
				break;
			}
		}
	}
}