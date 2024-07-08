#include "Common.hlsli"

ConstantBuffer<GlobalShaderData> global_data : register(b0, sapce0);
ConstantBuffer<LightCullingDispatchParameters> shader_params : register(b1, space0);
RWStructuredBuffer<Frustum> frustums : register(u0, space0);

[numthreads(TILE_SIZE, TILE_SIZE, 1)]
void compute_grid_frustum_cs( uint3 dispatch_thread_id : SV_DispatchThreadID )
{
    const uint x = dispatch_thread_id.x;
    const uint y = dispatch_thread_id.y;
    
    if (x >= shader_params.num_threds.x || y >= shader_params.num_threds.y) return;

    float4 screen_space[4];
    screen_space[0] = float4(float2(x, y) * TILE_SIZE, 0.f, 1.f);
    screen_space[1] = float4(float2(x + 1, y) * TILE_SIZE, 0.f, 1.f);
    screen_space[2] = float4(float2(x, y + 1) * TILE_SIZE, 0.f, 1.f);
    screen_space[3] = float4(float2(x + 1, y + 1) * TILE_SIZE, 0.f, 1.f);
    
    const float2 inv_view_dimensions = 1.f / float2(global_data.view_width, global_data.view_height);
    float3 view_space[4];
    
    view_space[0] = screen_to_view(screen_space[0], inv_view_dimensions, global_data.inverse_projection).xyz;
    view_space[1] = screen_to_view(screen_space[1], inv_view_dimensions, global_data.inverse_projection).xyz;
    view_space[2] = screen_to_view(screen_space[2], inv_view_dimensions, global_data.inverse_projection).xyz;
    view_space[3] = screen_to_view(screen_space[3], inv_view_dimensions, global_data.inverse_projection).xyz;

    const float3 eye_pos = (float3) 0;
    Frustum frustum;
    
    frustum.planes[0] = compute_plane(view_space[0], eye_pos, view_space[2]);
    frustum.planes[1] = compute_plane(view_space[3], eye_pos, view_space[1]);
    frustum.planes[2] = compute_plane(view_space[2], eye_pos, view_space[0]);
    frustum.planes[3] = compute_plane(view_space[1], eye_pos, view_space[3]);

    frustums[x + (y * shader_params.num_threds.x)] = frustum;
}