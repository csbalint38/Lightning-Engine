#if !defined(LIGHTNING_COMMON_HLSLI) && !defined(__cplusplus)
#error Do not include this header in shader files directly. Include Common.hlsli instead.
#endif

struct GlobalShaderData
{
    float4x4 view;
    float4x4 projection;
    float4x4 inverse_projection;
    float4x4 view_projection;
    float4x4 inv_view_projection;
    
    float3 camera_position;
    float view_width;
    
    float3 camera_direction;
    float view_height;
    
    uint num_directional_lights;
    
    float delta_time;
};

struct PerObjectData
{
    float4x4 world;
    float4x4 inv_world;
    float4x4 world_view_projection;
};

struct Plane
{
    float3 normal;
    float distance;
};

struct Frustum
{
    Plane planes[4];
};

struct LightCullingDispatchParameters
{
    uint2 num_thred_groups;
    uint3 num_threds;
    
    uint num_lights;
    uint depth_buffer_srv_index;
};

struct LightCullingLightInfo
{
    float3 position;
    float range;
    float3 direction;
    float cone_radius;
    uint type;
    float3 _pad;
};

struct LightParameters {
    float3 position;
    float intensity;
    float3 direction;
    uint type;
    float3 color;
    float range;
    float3 attenuation;
    float cos_umbra;
    float cos_penumbra;
    float3 _pad;
};

struct DirectionalLightParameters
{
    float3 direction;
    float intensity;
    float3 color;
    float _pad;
};

#ifdef __cplusplus
static_assert((sizeof(PerObjectData) % 16) == 0, "Make sure PerObjectData is formatted in 16-byte chunks without any implicit padding.");
static_assert((sizeof(PerObjectData) % 16) == 0, "Make sure LightParameters is formatted in 16-byte chunks without any implicit padding.");
static_assert((sizeof(LightCullingInfo) % 16) == 0, "Make sure LightCullingInfo is formatted in 16-byte chunks without any implicit padding.");
static_assert((sizeof(DirectionalLightParameters) % 16) == 0, "Make sure DirectionalLightParameters is formatted in 16-byte chunks without any implicit padding.");
#endif