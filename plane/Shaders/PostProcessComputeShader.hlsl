#include "Common.hlsl"

RWTexture2D<unorm float4> RenderTargetTexture : register(u1);
RWTexture2D<unorm float4> OutputTargetTexture : register(u2);

cbuffer ComputeShaderBuffer : register(b0)
{
    int BlurSize;
};

[numthreads(32, 32, 1)]
void CSMainX(uint3 id : SV_DispatchThreadID)
{
    float3 sumX = float3(0.0, 0.0, 0.0);
    
    for (int x = 0; x < BlurSize; x++)
    {
        uint2 pos = id.xy;
            
        pos.x += x - (BlurSize / 2);
            
        sumX += RenderTargetTexture[pos].rgb;
    }
    
    OutputTargetTexture[id.xy] = float4(sumX / BlurSize, 1.0);
}

[numthreads(32, 32, 1)]
void CSMainY(uint3 id : SV_DispatchThreadID)
{
    float3 sumY = float3(0.0, 0.0, 0.0);
    
    for (int y = 0; y < BlurSize; y++)
    {
        uint2 pos = id.xy;
            
        pos.y += y - (BlurSize / 2);
            
        sumY += RenderTargetTexture[pos].rgb;
    }
    
    OutputTargetTexture[id.xy] = float4(sumY / BlurSize, 1.0);
}