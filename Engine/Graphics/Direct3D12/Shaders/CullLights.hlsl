#include "Common.hlsli"

#if 1

static const uint max_lights_per_group = 1024;

groupshared uint _min_depth_vs;
groupshared uint _max_depth_vs;
groupshared uint _light_count;
groupshared uint _light_index_start_offset;
groupshared uint _light_index_list[max_lights_per_group];

ConstantBuffer<GlobalShaderData> global_data : register(b0, space0);
ConstantBuffer<LightCullingDispatchParameters> shader_params : register(b1, space0);
StructuredBuffer<Frustum> frustums : register(t0, space0);
StructuredBuffer<LightCullingLightInfo> lights : register(t1, space0);

RWStructuredBuffer<uint> light_index_counter : register(u0, space0);
RWStructuredBuffer<uint2> light_grid_opaque : register(u1, space0);
RWStructuredBuffer<uint> light_index_list_opaque : register(u3, space0);

[numthreads(TILE_SIZE, TILE_SIZE, 1)]
void cull_lights_cs(ComputeShaderInput cs_in)
{
    const float depth = Texture2D( ResourceDescriptorHeap[shader_params.depth_buffer_srv_index])[cs_in.dispatch_thread_id.xy].r;
    const float c = global_data.projection._m22;
    const float d = global_data.projection._m23;
    const uint grid_index = cs_in.group_id.x + (cs_in.group_id.y * shader_params.num_thread_groups.x);
    const Frustum frustum = frustums[grid_index];
    
    if (cs_in.group_index == 0)
    {
        _min_depth_vs = 0x7f7fffff;
        _max_depth_vs = 0;
        _light_count = 0;
    }

    uint i = 0, index = 0;

    GroupMemoryBarrierWithGroupSync();

    if (depth != 0)
    {
        const uint z = asuint(d / (depth + c));
        InterlockedMin(_min_depth_vs, z);
        InterlockedMax(_max_depth_vs, z);
    }

    GroupMemoryBarrierWithGroupSync();

    const float min_depth_vs = -asfloat(_min_depth_vs);
    const float max_depth_vs = -asfloat(_max_depth_vs);

    for (i = cs_in.group_index; i < shader_params.num_lights; i += TILE_SIZE * TILE_SIZE)
    {
        const LightCullingLightInfo light = lights[i];
        const float3 light_position_vs = mul(global_data.view, float4(light.position, 1.f)).xyz;

        if (light.type == LIGHT_TYPE_POINT_LIGHT)
        {
            const Sphere sphere = { light_position_vs, light.range };
            if (sphere_inside_frustum(sphere, frustum, min_depth_vs, max_depth_vs))
            {
                InterlockedAdd(_light_count, 1, index);
                if (index < max_lights_per_group)
                    _light_index_list[index] = i;
            }
        }
        else if (light.type == LIGHT_TYPE_SPOTLIGHT)
        {
            const float3 light_direction_vs = mul(global_data.view, float4(light.direction, 0.f)).xyz;
            const Cone cone = { light_position_vs, light.range, light_direction_vs, light.cone_radius };
            if (cone_inside_frustum(cone, frustum, min_depth_vs, max_depth_vs))
            {
                InterlockedAdd(_light_count, 1, index);
                if (index < max_lights_per_group)
                    _light_index_list[index] = i;
            }
        }
    }

    GroupMemoryBarrierWithGroupSync();

    const uint light_count = min(_light_count, max_lights_per_group);

    if (cs_in.group_index == 0)
    {
        InterlockedAdd(light_index_counter[0], light_count, _light_index_start_offset);
        light_grid_opaque[grid_index] = uint2(_light_index_start_offset, light_count);
    }

    GroupMemoryBarrierWithGroupSync();

    for (i = cs_in.group_index; i < light_count; i += TILE_SIZE * TILE_SIZE)
    {
        light_index_list_opaque[_light_index_start_offset + i] = _light_index_list[i];
    }
}

#else
static const uint max_lights_per_group = 1024;

groupshared uint _min_depth_vs;
groupshared uint _max_depth_vs;
groupshared uint _light_count;
groupshared uint _light_index_start_offset;
groupshared uint _light_index_list[max_lights_per_group];

ConstantBuffer<GlobalShaderData> global_data : register(b0, space0);
ConstantBuffer<LightCullingDispatchParameters> shader_params : register(b1, space0);
StructuredBuffer<Frustum> frustums : register(t0, space0);
StructuredBuffer<LightCullingLightInfo> lights : register(t1, space0);

RWStructuredBuffer<uint> light_index_counter : register(u0, space0);
RWStructuredBuffer<uint2> light_grid_opaque : register(u1, space0);
RWStructuredBuffer<uint> light_index_list_opaque : register(u3, space0);

[numthreads(TILE_SIZE, TILE_SIZE, 1)]
void cull_lights_cs(ComputeShaderInput cs_in)
{
    if (cs_in.group_index == 0)
    {
        _min_depth_vs = 0x7f7fffff;
        _max_depth_vs = 0;
        _light_count = 0;
    }

    uint i = 0, index = 0;

    GroupMemoryBarrierWithGroupSync();

    const float depth = Texture2D(ResourceDescriptorHeap[shader_params.depth_buffer_srv_index])[cs_in.dispatch_thread_id.xy].r;
    const float depth_vs = clip_to_view(float4(0.f, 0.f, depth, 1.f), global_data.inverse_projection).z;
    const uint z = asuint(-depth_vs);

    if (depth != 0)
    {
        InterlockedMin(_min_depth_vs, z);
        InterlockedMax(_max_depth_vs, z);
    }

    GroupMemoryBarrierWithGroupSync();

    const uint grid_index = cs_in.group_id.x + (cs_in.group_id.y * shader_params.num_thread_groups.x);
    const Frustum frustum = frustums[grid_index];
    const float min_depth_vs = -asfloat(_min_depth_vs);
    const float max_depth_vs = -asfloat(_max_depth_vs);

    for (i = cs_in.group_index; i < shader_params.num_lights; i += TILE_SIZE * TILE_SIZE)
    {
        const LightCullingLightInfo light = lights[i];
        const float3 light_position_vs = mul(global_data.view, float4(light.position, 1.f)).xyz;

        if (light.type == LIGHT_TYPE_POINT_LIGHT)
        {
            const Sphere sphere = { light_position_vs, light.range };
            if (sphere_inside_frustum(sphere, frustum, min_depth_vs, max_depth_vs))
            {
                InterlockedAdd(_light_count, 1, index);
                if (index < max_lights_per_group)
                    _light_index_list[index] = i;
            }
        }
        else if (light.type == LIGHT_TYPE_SPOTLIGHT)
        {
            const float3 light_direction_vs = mul(global_data.view, float4(light.direction, 0.f)).xyz;
            const Cone cone = { light_position_vs, light.range, light_direction_vs, light.cone_radius };
            if (cone_inside_frustum(cone, frustum, min_depth_vs, max_depth_vs))
            {
                InterlockedAdd(_light_count, 1, index);
                if (index < max_lights_per_group)
                    _light_index_list[index] = i;
            }
        }
    }

    GroupMemoryBarrierWithGroupSync();

    const uint light_count = min(_light_count, max_lights_per_group);

    if (cs_in.group_index == 0)
    {
        InterlockedAdd(light_index_counter[0], light_count, _light_index_start_offset);
        light_grid_opaque[grid_index] = uint2(_light_index_start_offset, light_count);
    }

    GroupMemoryBarrierWithGroupSync();

    for (i = cs_in.group_index; i < light_count; i += TILE_SIZE * TILE_SIZE)
    {
        light_index_list_opaque[_light_index_start_offset + i] = _light_index_list[i];
    }
}
#endif