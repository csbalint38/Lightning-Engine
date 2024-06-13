#include "Common.hlsli"

struct VertexOut {
    float4 homogeneous_position : SV_POSITION;
    float3 world_position : POSITIONT;
    float3 world_normal : NORMAL;
    float3 world_tangent : TANGENT;
    float2 uv : TEXTURE;
};

struct PixelOut {
    float4 color : SV_TARGET0;
};

ConstantBuffer<GlobalShaderData> per_frame_buffer : register(b0, space0);
ConstantBuffer<PerObjectData> per_object_buffer : register(b1, space0);
StructuredBuffer<float3> vertex_position : register(t0, space0);

VertexOut test_shader_vs(in uint vertex_idx : SV_VertexID) {
    VertexOut vs_out;
    
    vs_out.homogeneous_position = 0.f;
    vs_out.world_position = 0.f;
    vs_out.world_normal = 0.f;
    vs_out.world_tangent = 0.f;
    vs_out.uv = 0.f;
    
    return vs_out;
}

[earlydepthstencil]
PixelOut test_shader_ps(in VertexOut ps_in) {
    PixelOut ps_out;
    ps_out.color = 0.f;
    
    return ps_out;
}