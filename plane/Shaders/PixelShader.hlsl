#include "Common.hlsl"

cbuffer PixelShaderBuffer : register(b0)
{
    float TimeElapsed;
};

SamplerState LinearSamplerView : SAMPLER : register(s0);
Texture2D MainTextureView : TEXTURE : register(t0);

STANDARD_PS_OUTPUT PSMain(STANDARD_PS_INPUT input) : SV_TARGET
{
    STANDARD_PS_OUTPUT output;
    
    float4 finalColor = float4(0.0, 0.0, 0.0, 1.0);
    
    float4 textureColor = MainTextureView.Sample(LinearSamplerView, input.uv);
    
    finalColor = float4(textureColor.xyz, 1.0);
    
    output.color = finalColor;
    
    return output;
}