#include "Common.hlsl"

cbuffer VertexShaderBuffer : register(b0)
{
    float4x4 ViewProjection; // 64 bytes
    float4x4 World; // 128 bytes
};

PS_INPUT VSMain(VS_INPUT input)
{
    PS_INPUT output;
    
    output.uv = input.uv;
    
    output.worldPosition = mul(float4(input.position, 1.0), World);
    
    output.position = mul(output.worldPosition, ViewProjection);
    
    output.normal = normalize(mul(float4(input.normal, 0.0), World));
    
    return output;
}