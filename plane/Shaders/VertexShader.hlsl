#include "Common.hlsl"

cbuffer VertexShaderBuffer : register(b0)
{
    float4x4 ViewProjection; // 64 bytes
    float4x4 World; // 128 bytes
};

STANDARD_PS_INPUT VSMain(STANDARD_VS_INPUT input)
{
    STANDARD_PS_INPUT output;
    
    output.uv = input.uv;
    
    output.worldPosition = mul(float4(input.position, 1.0), World);
    
    output.position = mul(output.worldPosition, ViewProjection);
    
    output.normal = input.normal;
    
    return output;
}