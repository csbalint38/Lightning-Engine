#include "Common.hlsli"

struct VertexOut {
    float4 homogeneous_position : SV_POSITION;
    float3 world_position : POSITIONT;
    float3 world_normal : NORMAL;
    float3 world_tangent : TANGENT;
    float2 uv : TEXTURE;
};

struct ElementStaticNormalTexture
{
    uint Coloc_t_sign;
    uint16_t2 normal;
    uint16_t2 tangent;
    float2 uv;
};

struct PixelOut {
    float4 color : SV_TARGET0;
};

const static float inv_intervals = 2.f / ((1 << 16) - 1);

ConstantBuffer<GlobalShaderData> global_data : register(b0, space0);
ConstantBuffer<PerObjectData> per_object_buffer : register(b1, space0);
StructuredBuffer<float3> vertex_positions : register(t0, space0);
StructuredBuffer<ElementStaticNormalTexture> Elements : register(t1, space0);

VertexOut test_shader_vs(in uint vertex_idx : SV_VertexID) {
    VertexOut vs_out;
    
    float4 position = float4(vertex_positions[vertex_idx], 1.f);
    float4 world_position = mul(per_object_buffer.world, position);
    
    uint signs = 0;
    uint16_t2 packed_normal = 0;
    ElementStaticNormalTexture element = Elements[vertex_idx];
    signs = (element.Coloc_t_sign >> 24) & 0xff;
    packed_normal = element.normal;
    
    float n_sign = float(signs & 0x02) - 1;
    float3 normal;
    normal.x = packed_normal.x * inv_intervals - 1.f;
    normal.y = packed_normal.y * inv_intervals - 1.f;
    normal.z = sqrt(saturate(1.f - dot(normal.xy, normal.xy))) * n_sign;
    
    vs_out.homogeneous_position = mul(per_object_buffer.world_view_projection, position);
    vs_out.world_position = world_position.xyz;
    vs_out.world_normal = mul(float4(normal, 0.f), per_object_buffer.inv_world).xyz;
    vs_out.world_tangent = 0.f;
    vs_out.uv = 0.f;
    
    return vs_out;
}

[earlydepthstencil]
PixelOut test_shader_ps(in VertexOut ps_in) {
    PixelOut ps_out;
    ps_out.color = float4(ps_in.world_normal, 1.f);
    
    return ps_out;
}