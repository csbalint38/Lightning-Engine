const static float PI = 3.1415926535897932f;
const static float SAMPLE_OFFSET = .5f;

cbuffer Constants : register(b0)
{
    uint g_cube_map_in_size;
    uint g_cube_map_out_size;
    uint g_sample_count_or_mirror;
    float g_roughness;
};

Texture2D<float4> env_map : register(t0);
TextureCube<float4> cube_map_in : register(t0);

RWTexture2DArray<float4> output : register(u0);

SamplerState linear_sampler : register(s0);

float radical_inverse_vdc(uint bits)
{
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}

float2 Hammersley(uint i, uint n)
{
    return float2(float(i) / float(n), radical_inverse_vdc(i));
}

float pow4(float x)
{
    float xx = x * x;

    return xx * xx;
}

float pow5(float x)
{
    float xx = x * x;
    
    return xx * xx * x;
}

float3 get_sample_direction_equirectangular(uint face, float x, float y)
{
    float3 direction[6] =
    {
        { -x, 1.f, -y }, // x+ left
        { x, -1.f, -y }, // x- right
        { y, x, 1.f }, // y+ bottom
        { -y, x, -1.f }, // y- top
        { 1.f, x, -y }, // z+ front
        { -1.f, -x, y }, // z- back
    };
    
    return normalize(direction[face]);
}

float3 get_sample_direction_cubemap(uint face, float x, float y)
{
    float3 direction[6] =
    {
        { 1.f, -y, -x }, // x+ left
        { -1.f, -y, x }, // x- right
        { x, 1.f, y }, // y+ bottom
        { x, -1.f, -y }, // y- top
        { x, -y, 1.f }, // z+ front
        { -x, -y, -1.f }, // z- back
    };
    
    return normalize(direction[face]);
}

float2 direction_to_equirectangular_uv(float3 dir)
{
    float phi = atan2(dir.y, dir.x);
    float theta = acos(dir.z);
    float u = phi * (.5f / PI) + .5f;
    float v = theta * (1.f / PI);
    
    return float2(u, v);
}

float v_Smith_ggx_correlated(float n_o_v, float n_o_l, float a)
{
    float ggxl = n_o_v * sqrt((-n_o_l * a + n_o_l) * n_o_l + a);
    float ggxv = n_o_l * sqrt((-n_o_v * a + n_o_v) * n_o_v + a);
    
    return .5f * rcp(ggxv + ggxl);
}

float d_ggx(float n_o_h, float a)
{
    float d = (n_o_h * a - n_o_h) * n_o_h + 1.f;

    return a / (PI * d * d);
}

float3x3 get_tangent_frame(float3 normal)
{
    float3 up = abs(normal.z) < .999f ? float3(0, 0, 1) : float3(1, 0, 0);
    float3 tangent_x = normalize(cross(up, normal));
    float3 tangent_y = cross(normal, tangent_x);
    
    return float3x3(tangent_x, tangent_y, normal);
}


float3 importance_sample_ggx(float2 e, float a)
{
    float phi = 2.f * PI * e.x;
    float cos_theta = sqrt((1.f - e.y) / (1.f + (a - 1.f) * e.y));
    float sin_theta = sqrt(1.f - cos_theta * cos_theta);

    float3 h;
    h.x = sin_theta * cos(phi);
    h.y = sin_theta * sin(phi);
    h.z = cos_theta;

    return h;
}

float2 integrate_brdf(float n_o_v, float roughness)
{
    float a4 = pow4(roughness);
    float3 v;
    
    v.x = sqrt(1.f - n_o_v * n_o_v);
    v.y = 0.f;
    v.z = n_o_v;
    
    float a = 0.f;
    float b = 0.f;
    uint num_samples = g_sample_count_or_mirror;
    
    for (uint i = 0; i < num_samples; i++)
    {
        float2 x_i = Hammersley(i, num_samples);
        float3 h = importance_sample_ggx(x_i, a4);
        float3 l = reflect(-v, h);
        float n_o_l = saturate(l.z);
        float n_o_h = saturate(h.z);
        float v_o_h = saturate(dot(v, h));
        
        if (n_o_l > 0.f)
        {
            float g = v_Smith_ggx_correlated(n_o_v, n_o_l, a4);
            float g_v_is = 4 * n_o_l * g * v_o_h / n_o_h;
            float f_c = pow5(1.f - v_o_h);
            
            a += (1.f - f_c) * g_v_is;
            b += f_c * g_v_is;
        }
    }
    return float2(a, b) / num_samples;
}

float3 prefilter_env_map(float roughness, float3 n)
{
    float a4 = pow4(roughness);
    float3 v = n;

    float3 prefiltered_color = 0;
    float total_weight = 0;
    uint num_samples = g_sample_count_or_mirror;
    float resolution = g_cube_map_in_size;
    float inv_omega_p = (6.f * resolution * resolution) * PI * .25f;
    float mip_level = 0;
    float3x3 tangent_frame = get_tangent_frame(n);

    for (uint i = 0; i < num_samples; i++)
    {
        float2 x_i = Hammersley(i, num_samples);
        float3 h = mul(importance_sample_ggx(x_i, a4), tangent_frame);
        float3 l = 2 * dot(v, h) * h - v;
        float n_o_l = saturate(dot(n, l));

        if(n_o_l > 0)
        {
            float n_o_h = saturate(dot(n, h));
            float h_o_v = saturate(dot(h, v));
            float pdf = d_ggx(n_o_h, a4) * .25f;
            float omega_s = 1.f / (float(num_samples) * pdf + 0.0001f);

            mip_level = roughness == 0.f ? 0.f : .5f * log2(omega_s * inv_omega_p);

            prefiltered_color += cube_map_in.SampleLevel(linear_sampler, l, mip_level).rgb * n_o_l;
            total_weight += n_o_l;
        }
    }

    return prefiltered_color / total_weight;
}

float3 sample_hemisphere_discrete(float3 normal)
{
    float3 n = normal;
    float3 irradiance = 0;
    float3x3 tangent_frame = get_tangent_frame(n);
    
    float delta = .02f;
    uint sample_count = 0;
    
    for (float phi = 0; phi < 2 * PI; phi += delta)
    {
        float sin_phi = sin(phi);
        float cos_phi = cos(phi);
        
        for (float theta = 0; theta < .5 * PI; theta += delta)
        {
            float sin_theta = sin(theta);
            float cos_theta = cos(theta);
            float3 transform = float3(sin_theta * cos_phi, sin_theta * sin_phi, cos_theta);
            float3 sample_dir = mul(transform, tangent_frame);
        
            irradiance += cube_map_in.SampleLevel(linear_sampler, sample_dir, 0).rgb * cos_theta * sin_theta;
            
            ++sample_count;
        }
    }

    irradiance *= PI / float(sample_count);
    
    return irradiance;
}

float3 sample_hemisphere_random(float3 normal)
{
    float3 irradiance = 0.f;
    float3x3 tangent_frame = get_tangent_frame(normal);
    uint sample_count = g_sample_count_or_mirror;
    
    for (uint i = 0; i < sample_count; ++i)
    {
        float2 x_i = Hammersley(i, sample_count);
        float phi = 2.f * PI * x_i.x;
        float sin_theta = sqrt(x_i.y);
        float cos_theta = sqrt(1.f - x_i.y);
        float sin_phi = sin(phi);
        float cos_phi = cos(phi);
        
        float3 transform = float3(sin_theta * cos_phi, sin_theta * sin_phi, cos_theta);
        float3 sample_dir = mul(transform, tangent_frame);
        
        irradiance += cube_map_in.SampleLevel(linear_sampler, sample_dir, 0).rgb * cos_theta;
    }

    irradiance *= 1.f / sample_count;
    
    return irradiance;
}

float3 sample_hemisphere_brute(float3 normal)
{
    float3 irradiance = 0.f;
    float sample_count = 0.f;
    float inv_dim = 1.f / g_cube_map_in_size;
    
    for (uint face = 0; face < 6; ++face)
    {
        for (uint y = 0; y < g_cube_map_in_size; ++y)
        {
            for (uint x = 0; x < g_cube_map_in_size; ++x)
            {
                float2 uv = (float2(x, y) + SAMPLE_OFFSET) * inv_dim;
                float2 pos = 2.f * uv - 1.f;
                
                float3 sample_dir = get_sample_direction_cubemap(face, pos.x, pos.y);
                float cos_theta = dot(sample_dir, normal);
                
                if (cos_theta > 0.f)
                {
                    float tmp = 1.f + pos.x * pos.x + pos.y * pos.y;
                    float weight = 4.f * cos_theta / (sqrt(tmp) * tmp);
                    irradiance += cube_map_in.SampleLevel(linear_sampler, sample_dir, 0).rgb * weight * cos_theta;
                    sample_count += weight;
                }
            }
        }
    }
    irradiance *= 1.f / sample_count;
    
    return irradiance;
}

[numthreads(16, 16, 1)]
void equirectangular_to_cube_map_cs(uint3 dispatch_thread_id : SV_DispatchThreadID, uint3 group_id : SV_GroupID)
{
    uint face = group_id.z;
    uint size = g_cube_map_out_size;
    
    if (dispatch_thread_id.x >= size || dispatch_thread_id.y >= size || face >= 6) return;

    float2 uv = (float2(dispatch_thread_id.xy) + SAMPLE_OFFSET) / size;
    float2 pos = 2.f * uv - 1.f;
    float3 sampleDirection = get_sample_direction_equirectangular(face, pos.x, pos.y);
    float2 dir = direction_to_equirectangular_uv(sampleDirection);
    
    if (g_sample_count_or_mirror == 1) dir.x = 1.f - dir.x;
    float4 env_sample = env_map.SampleLevel(linear_sampler, dir, 0);

    output[uint3(dispatch_thread_id.x, dispatch_thread_id.y, face)] = env_sample;
}

[numthreads(16, 16, 1)]
void prefilter_diffuse_env_map_cs(uint3 dispatch_thread_id : SV_DispatchThreadID, uint3 group_id : SV_GroupID)
{
    uint face = group_id.z;
    uint size = g_cube_map_out_size;
    
    if (dispatch_thread_id.x >= size || dispatch_thread_id.y >= size || face >= 6) return;

    float2 uv = (float2(dispatch_thread_id.xy) + SAMPLE_OFFSET) / size;
    float2 pos = 2.f * uv - 1.f;
    float3 sample_direction = get_sample_direction_cubemap(face, pos.x, pos.y);
    float3 irradiance = sample_hemisphere_brute(sample_direction);
    // float3 irradiance = sample_hemisphere_random(sample_direction);
    // float3 irradiance = sample_hemisphere_discrete(sample_direction);
    
    output[uint3(dispatch_thread_id.x, dispatch_thread_id.y, face)] = float4(irradiance, 1.f);
}

[numthreads(16, 16, 1)]
void compute_brdf_integration_lut_cs(uint3 dispatch_thread_id : SV_DispatchThreadID)
{
    uint size = g_cube_map_out_size;
    
    if (dispatch_thread_id.x >= size || dispatch_thread_id.y >= size) return;
    
    float2 uv = float2(dispatch_thread_id.xy) / (size - 1);
    float2 result = integrate_brdf(uv.x, uv.y);
    
    output[uint3(dispatch_thread_id.x, dispatch_thread_id.y, 0)] = float4(result, 1.f, 1.f);
}

[numthreads(16, 16, 1)]
void prefilter_specular_env_map_cs(uint3 dispatch_thread_id : SV_DispatchThreadID, uint3 group_id : SV_GroupID)
{
    uint face = group_id.z;
    uint size = g_cube_map_out_size;

    if(dispatch_thread_id.x >= size || dispatch_thread_id.y >= size || face >= 6) return;

    float2 uv = (float2(dispatch_thread_id.xy) + SAMPLE_OFFSET) / size;
    float2 pos = 2.f * uv - 1.f;
    float3 sample_direction = get_sample_direction_cubemap(face, pos.x, pos.y);
    float3 irradiance = prefilter_env_map(g_roughness, sample_direction);

    output[uint3(dispatch_thread_id.x, dispatch_thread_id.y, face)] = float4(irradiance, 1.f);
}
