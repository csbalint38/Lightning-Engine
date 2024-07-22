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

#define ELEMENTS_TYPE_STATIC_NORMAL 0x01
#define ELEMENTS_TYPE_STATIC_NORMAL_TEXTURE 0x03
#define ELEMENTS_TYPE_STATIC_COLOR 0x04
#define ELEMENTS_TYPE_SKELETAL 0x08
#define ELEMENTS_TYPE_SKELETAL_COLOR ELEMENTS_TYPE_SKELETAL | ELEMENTS_TYPE_STATIC_COLOR
#define ELEMENTS_TYPE_SKELETAL_NORMAL ELEMENTS_TYPE_SKELETAL | ELEMENTS_TYPE_STATIC_NORMAL
#define ELEMENTS_TYPE_SKELETAL_NORMAL_COLOR ELEMENTS_TYPE_SKELETAL_NORMAL | ELEMENTS_TYPE_STATIC_COLOR
#define ELEMENTS_TYPE_SKELETAL_NORMAL_TEXTURE ELEMENTS_TYPE_SKELETAL | ELEMENTS_TYPE_STATIC_NORMAL_TEXTURE
#define ELEMENTS_TYPE_SKELETAL_NORMAL_TEXTURE_COLOR ELEMENTS_TYPE_SKELETAL_NORMAL_TEXTURE | ELEMENTS_TYPE_STATIC_COLOR

struct VertexElement
{
    #if ELEMENTS_TYPE == ELEMENTS_TYPE_STATIC_NORMAL
    uint color_t_sign;
    uint16_t2 normal;
    #elif ELEMENTS_TYPE == ELEMENTS_TYPE_STATIC_NORMAL_TEXTURE
    uint color_t_sign;
    uint16_t2 normal;
    uint16_t2 tangent;
    float2 uv;
    #elif ELEMENTS_TYPE == ELEMENTS_TYPE_STATIC_COLOR
    #elif ELEMENTS_TYPE == ELEMENTS_TYPE_SKELETAL
    #elif ELEMENTS_TYPE == ELEMENTS_TYPE_SKELETAL_NORMAL
    #elif ELEMENTS_TYPE == ELEMENTS_TYPE_SKELETAL_NORMAL_COLOR
    #elif ELEMENTS_TYPE == ELEMENTS_TYPE_SKELETAL_NORMAL_TEXTURE
    #elif ELEMENTS_TYPE == ELEMENTS_TYPE_SKELETAL_NORMAL_TEXTURE_COLOR
    #endif
};

const static float inv_intervals = 2.f / ((1 << 16) - 1);

ConstantBuffer<GlobalShaderData> global_data : register(b0, space0);
ConstantBuffer<PerObjectData> per_object_buffer : register(b1, space0);
StructuredBuffer<float3> vertex_positions : register(t0, space0);
StructuredBuffer<VertexElement> elements : register(t1, space0);
StructuredBuffer<DirectionalLightParameters> directional_lights : register(t3, space0);
StructuredBuffer<LightParameters> cullable_lights : register(t4, space0);
StructuredBuffer<uint2> light_grid : register(t5, space0);
StructuredBuffer<uint> light_index_list : register(t6, space0);

VertexOut test_shader_vs(in uint vertex_idx : SV_VertexID) {
    VertexOut vs_out;
    
    float4 position = float4(vertex_positions[vertex_idx], 1.f);
    float4 world_position = mul(per_object_buffer.world, position);
    
    #if ELEMENTS_TYPE == ELEMENTS_TYPE_STATIC_NORMAL
    VertexElement element = elements[vertex_idx];
    float2 n_xy = element.normal * inv_intervals - 1.f;
    uint signs = (element.color_t_sign >> 24) & 0xff;
    float n_sign = float(signs & 0x02) - 1;
    float3 normal = float3(n_xy.x, n_xy.y, sqrt(saturate(1.f - dot(n_xy, n_xy))) * n_sign);
    
    vs_out.homogeneous_position = mul(per_object_buffer.world_view_projection, position);
    vs_out.world_position = world_position.xyz;
    vs_out.world_normal = mul(float4(normal, 0.f), per_object_buffer.inv_world).xyz;
    vs_out.world_tangent = 0.f;
    vs_out.uv = 0.f;

    #elif ELEMENTS_TYPE == ELEMENTS_TYPE_STATIC_NORMAL_TEXTURE
    VertexElement element = elements[vertex_idx];
    float2 n_xy = element.normal * inv_intervals - 1.f;
    uint signs = (element.color_t_sign >> 24) & 0xff;
    float n_sign = float(signs & 0x02) - 1;
    float3 normal = float3(n_xy.x, n_xy.y, sqrt(saturate(1.f - dot(n_xy, n_xy))) * n_sign);
    
    vs_out.homogeneous_position = mul(per_object_buffer.world_view_projection, position);
    vs_out.world_position = world_position.xyz;
    vs_out.world_normal = mul(float4(normal, 0.f), per_object_buffer.inv_world).xyz;
    vs_out.world_tangent = 0.f;
    vs_out.uv = 0.f;
    #endif

    return vs_out;
}

#define TILE_SIZE 32 
#define NO_LIGHT_ATTENUATION 0

float3 calculate_lighting(float3 n, float3 l, float3 v, float3 light_color)
{
    const float no_l = dot(n, l);
    float specular = 0;
    
    if (no_l > 0.f)
    {
        const float3 r = reflect(-l, n);
        const float va_r = max(dot(v, r), 0.f);
        specular = saturate(no_l * pow(va_r, 4.f) * .5f);
    }

    return (max(0.f, no_l) + specular) * light_color;
}

float3 point_light(float3 n, float3 world_position, float3 v, LightParameters light)
{
    float3 l = light.position - world_position;
    const float d_sq = dot(l, l);
    float3 color = 0.f;
    
    #if NO_LIGHT_ATTENUATION
    if(d_sq < light.range * light.range) {
        const float d_rcp = rsqrt(d_sq);
        l *= d_rcp;
        color = saturate(dot(n, l) * light.color * light.intensity * .2f);
    }
    return color;
    #else
    if (d_sq < light.range * light.range)
    {
        const float d_rcp = rsqrt(d_sq);
        l *= d_rcp;
        const float attenuation = 1.f - smoothstep(-light.range, light.range, rcp(d_rcp));
        color = calculate_lighting(n, l, v, light.color * light.intensity * attenuation);
    }
    return color;
    #endif
}

float3 spotlight(float3 n, float3 world_position, float3 v, LightParameters light)
{
    float3 l = light.position - world_position;
    const float d_sq = dot(l, l);
    float3 color = 0.f;
    
    #if NO_LIGHT_ATTENUATION
    if(d_sq < light.range * light.range) {
        const float d_rcp = rsqrt(d_sq);
        l *= d_rcp;
        const float cos_angle_to_light = saturate(dot(-l, light.direction)); 
        const float angular_attenuation = float(light.cos_penumbra < cos_angle_to_light); 
        color = saturate(dot(n, l) * light.color * light.intensity * angular_attenuation * .2f);
    }
    return color;
    #else
    
    if (d_sq < light.range * light.range)
    {
        const float d_rcp = rsqrt(d_sq);
        l *= d_rcp;
        const float attenuation = 1.f - smoothstep(-light.range, light.range, rcp(d_rcp));
        const float cos_angle_to_light = saturate(dot(-l, light.direction));
        const float angular_attenuation = smoothstep(light.cos_penumbra, light.cos_umbra, cos_angle_to_light);
        color = calculate_lighting(n, l, v, light.intensity * attenuation * angular_attenuation);

    }
    
    return color;
    #endif
}

uint get_grid_index(float2 pos_xy, float view_width)
{
    const uint2 pos = uint2(pos_xy);
    const uint tile_x = ceil(view_width / TILE_SIZE);
    return (pos.x / TILE_SIZE) + (tile_x * (pos.y / TILE_SIZE));
}

[earlydepthstencil]
PixelOut test_shader_ps(in VertexOut ps_in) {
    PixelOut ps_out;
    
    float3 normal = normalize(ps_in.world_normal);
    float3 view_dir = normalize(global_data.camera_position - ps_in.world_position);
    
    float3 color = 0;
    
    uint i = 0;
    for (i = 0; i < global_data.num_directional_lights; ++i)
    {
        DirectionalLightParameters light = directional_lights[i];
        
        float3 light_direction = light.direction;
        
        if (abs(light_direction.z - 1.f) < .001f)
        {
            light_direction = global_data.camera_direction;
        }
        
        color += .02f * calculate_lighting(normal, -light_direction, view_dir, light.color * light.intensity);
    }
    
    const uint grid_index = get_grid_index(ps_in.homogeneous_position.xy, global_data.view_width);
    uint light_start_index = light_grid[grid_index].x;
    const uint light_count = light_grid[grid_index].y;
    
    #if USE_BOUNDING_SPHERES
    const uint num_point_lights = light_start_index + (light_count << 16);
    const uint num_spotlights = num_point_lights + (light_count & 0xffff);
    
    for(i = light_start_index; i < num_point_lights; ++i) {
        const uint light_index = light_index_list[i];
        LightParameters light = cullable_lights[light_index];
        color += point_light(normal, ps_in.world_position, view_dir, light);
    }
    
    for(i = num_point_lights; i < num_spot_lights; ++i) {
        const uint light_index = light_index_list[i];
        LightParameters light = cullable_lights[light_index];
        color += spotlight(normal, ps_in.world_position, view_dir, light);
    }
    #else
    for (i = 0; i < light_count; ++i)
    {
        const uint light_index = light_index_list[light_start_index + i];
        LightParameters light = cullable_lights[light_index];
        
        if (light.type == LIGHT_TYPE_POINT_LIGHT)
        {
            color += point_light(normal, ps_in.world_position, view_dir, light);
        }
        else if (light.type == LIGHT_TYPE_SPOTLIGHT)
        {
            color += spotlight(normal, ps_in.world_position, view_dir, light);
        }

    }
    #endif
    
    float3 ambient = 0 / 255.f;
    ps_out.color = saturate(float4(color + ambient , 1.f));
    
    return ps_out;
}