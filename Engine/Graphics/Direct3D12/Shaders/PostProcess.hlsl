struct ShaderConstants
{
    uint gpass_main_buffer_index;
};

ConstantBuffer<ShaderConstants> shader_params : register(b1);
Texture2D textures[] : register(t0, space0);

float4 post_process_ps(in noperspective float4 position : SV_Position, in noperspective float2 uv : TEXCOORD) : SV_Target0
{
    Texture2D gpass_main = textures[shader_params.gpass_main_buffer_index];
    float4 color = float4(gpass_main[position.xy].xyz, 1.f);

    return color;
}