struct PS_INPUT
{
    float4 position : SV_Position;
    float4 worldPosition : W_Position;
    float2 uv : TEXCOORD;
    float3 normal : NORMAL;
};
struct PS_OUTPUT
{
    float4 color : SV_Target;
};

struct VS_INPUT
{
    float3 position : POSITION;
    float2 uv : TEXCOORD;
    float3 normal : NORMAL;
};