#if !defined(LIGHTNING_COMMON_HLSLI) && !defined(__cplusplus)
#error Do not include this header in shader files directly. Include Common.hlsli instead.
#endif

Plane compute_plane(float3 p0, float3 p1, float3 p2)
{
    Plane plane;
    
    const float3 v0 = p1 - p0;
    const float3 v2 = p2 - p0;
    
    plane.normal = normalize(cross(v0, v2));
    plane.distance = dot(plane.normal, p0);

    return plane;
}

float4 clip_to_view(float4 clip, float4x4 inverse_projection)
{
    float4 view = mul(inverse_projection, clip);
    view /= view.w;
    
    return view;
}

float4 screen_to_view(float4 screen, float2 inv_view_dimensions, float4x4 inverse_projection)
{
    float2 tex_coord = screen.xy * inv_view_dimensions;
    
    float4 clip = float4(float2(tex_coord.x, 1.f - tex_coord.y) * 2.f - 1.f, screen.z, screen.w);
    
    return clip_to_view(clip, inverse_projection);

}