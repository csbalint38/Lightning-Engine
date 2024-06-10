struct GlobalShaderData {
    float4x4 view;
    float4x4 projection;
    float4x4 inverse_projection;
    float4x4 view_projection;
    float4x4 inv_view_projection;
    
    float3 camera_position;
    float view_width;
    
    float3 camera_direction;
    float view_height;
    
    float delta_time;
};

struct PerObjectData {
    float4x4 world;
    float4x4 inv_world;
    float4x4 world_view_projection;
};

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