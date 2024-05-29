float4 fill_color_ps(in noperspective float4 position : SV_Position, in noperspective float2 uv : TEXCOORD) : SV_Target
{
    return float4 (1.f, 0.f, 1.f, 1.f);
}