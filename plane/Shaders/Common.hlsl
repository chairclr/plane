struct STANDARD_PS_INPUT
{
    float4 position : SV_Position;
    float4 worldPosition : W_Position;
    float2 uv : TEXCOORD;
    float3 normal : NORMAL;
};
struct STANDARD_PS_OUTPUT
{
    float4 color : SV_Target;
};

struct STANDARD_VS_INPUT
{
    float3 position : POSITION;
    float2 uv : TEXCOORD;
    float3 normal : NORMAL;
};