#include "Common.hlsli"

ConstantBuffer<GlobalShaderData> global_data : register(b0, space0);
ConstantBuffer<LightCullingDispatchParameters> shader_params : register(b1, space0);
RWStructuredBuffer<Frustum> frustums : register(u0, space0);

[numthreads(TILE_SIZE, TILE_SIZE, 1)]
void compute_grid_frustum_cs(uint3 dispatch_thread_id : SV_DispatchThreadID)
{
    if (dispatch_thread_id.x >= shader_params.num_threads.x || dispatch_thread_id.y >= shader_params.num_threads.y)
    {
        return;
    }

    const float2 inv_view_dimension = TILE_SIZE / float2(global_data.view_width, global_data.view_height);
    const float2 top_left = dispatch_thread_id.xy * inv_view_dimension;
    const float2 center = top_left + (inv_view_dimension * .5f);
    
    float3 top_left_vs = unproject_uv(top_left, 0, global_data.inverse_projection).xyz;
    float3 center_vs = unproject_uv(center, 0, global_data.inverse_projection).xyz;
    
    const float far_clip_rcp = -global_data.inverse_projection._m33;
    Frustum frustum = { normalize(center_vs), distance(center_vs, top_left_vs) * far_clip_rcp };

    frustums[dispatch_thread_id.x + (dispatch_thread_id.y * shader_params.num_threads.x)] = frustum;
}
