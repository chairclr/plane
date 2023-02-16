#include "Common.hlsl"

#define BLUR_SIZE 8

RWTexture2D<unorm float4> RenderTargetTexture : register(u1);

[numthreads(16, 16, 1)]
void CSMainX(uint3 id : SV_DispatchThreadID)
{
    float3 sumX = float3(0.0, 0.0, 0.0);
    
    for (int x = 0; x < BLUR_SIZE; x++)
    {
        uint2 pos = id.xy;
            
        pos.x += x - (BLUR_SIZE / 2);
            
        sumX += RenderTargetTexture[pos].rgb;
    }
    
    RenderTargetTexture[id.xy] = float4(sumX / BLUR_SIZE, 1.0);
}

[numthreads(16, 16, 1)]
void CSMainY(uint3 id : SV_DispatchThreadID)
{
    float3 sumY = float3(0.0, 0.0, 0.0);
    
    for (int y = 0; y < BLUR_SIZE; y++)
    {
        uint2 pos = id.xy;
            
        pos.y += y - (BLUR_SIZE / 2);
            
        sumY += RenderTargetTexture[pos].rgb;
    }
    
    RenderTargetTexture[id.xy] = float4(sumY / BLUR_SIZE, 1.0);
}