#include "Fractals.hlsli"

struct ShaderConstants
{
    float width;
    float height;
    uint frame;
};

ConstantBuffer<ShaderConstants> shader_params : register(b1);

float4 fill_color_ps(in noperspective float4 position : SV_Position, in noperspective float2 in_uv : TEXCOORD) : SV_Target0
{
    const float2 inv_dim = float2(1.f / shader_params.width, 1.f / shader_params.height);
    const float2 uv = (position.xy) * inv_dim;
    float3 color = draw_mandelbrot(uv);
    
    return float4(color, 1.f);

}