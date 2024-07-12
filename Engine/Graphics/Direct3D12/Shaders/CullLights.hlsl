#include "Common.hlsli"

static const uint max_lights_per_goup = 1024;
groupshared uint _min_depth_vs;
groupshared uint _max_depth_vs;
groupshared uint _light_count;
groupshared uint _light_index_start_offset;
groupshared uint _light_index_list[max_lights_per_goup];

ConstantBuffer<GlobalShaderData> global_data : register(b0, space0);
ConstantBuffer<LightCullingDispatchParameters> shader_params : register(b1, space0);
StructuredBuffer<Frustum> frustums : register(t0, space0);
StructuredBuffer<LightCullingLightInfo> lights : register(t1, space0);

RWStructuredBuffer<uint> light_index_counter : register(u0, space0);
RWStructuredBuffer<uint2> light_grid_opaque : register(u1, space0);
RWStructuredBuffer<uint2> light_index_list : register(u3, space0);

[numthreads(TILE_SIZE, TILE_SIZE, 1)]
void cull_lights_cs(ComputeShaderInput cs_in)
{
    // INITIALIZATION
    if (cs_in.group_index == 0)
    {
        _min_depth_vs = 0x7f7fffff; // FLT_MAX as uint
        _max_depth_vs = 0;
        _light_count = 0;
    }
    
    uint i = 0;
    uint index = 0;
    
    // DEPTH MIN/MAX
    GroupMemoryBarrierWithGroupSync();
    const float depth = Texture2D(ResourceDescriptrHeap[shader_params.depth_buffer_srv_index])[cs_in.dispatch_thread_id.xy].r;
    const float depth_vs = clip_to_view(float4(0.f, 0.f, depth, 1.f), global_data.inverse_projection).z;
    
    const uint z = asuint(-depth_vs);
    
    if (depth != 0)
    {
        InterlockedMin(_min_depth_vs, z);
        InterlockedMax(_max_depth_vs, z);

    }
    
    // LIGHT CULLING
    GroupMemoryBarrierWithGroupSync();
    
    // UPDATE LIGHT GRID
    GroupMemoryBarrierWithGroupSync();
    
    // UPDATE LIGHT INDEX
    GroupMemoryBarrierWithGroupSync();

}