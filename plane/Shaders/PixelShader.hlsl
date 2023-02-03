#include "Common.hlsl"

cbuffer PixelShaderBuffer : register(b0)
{
    float TimeElapsed;
    float3 Padding;
};

SamplerState LinearSamplerView : SAMPLER : register(s0);
Texture2D MainTextureView : TEXTURE : register(t0);

PS_OUTPUT PSMain(PS_INPUT input) : SV_TARGET
{
    PS_OUTPUT output;
    
    float4 finalColor = float4(0.0, 0.0, 0.0, 1.0);
    
    float4 textureColor = MainTextureView.Sample(LinearSamplerView, input.uv);
    
    finalColor = float4(textureColor.xyz, 1.0);
    
    output.color = finalColor;
    
    return output;
}