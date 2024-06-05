#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12::upload {
	class D3D12UploadContext {
		public:
			D3D12UploadContext(u32 aligned_size);
			DISABLE_COPY_AND_MOVE(D3D12UploadContext);
			~D3D12UploadContext() {}

			void end_upload();

			id3d12_graphics_command_list* const command_list() const { return _cmd_list; }
			ID3D12Resource* const upload_buffer() const { return _upload_buffer; }
			void* const cpu_address() const { return _cpu_address; }

		private:
			id3d12_graphics_command_list* _cmd_list{ nullptr };
			ID3D12Resource* _upload_buffer{ nullptr };
			void* _cpu_address{ nullptr };
			u32 _frame_index{ u32_invalid_id };
	};

	bool initialize();
	void shutbown();
}