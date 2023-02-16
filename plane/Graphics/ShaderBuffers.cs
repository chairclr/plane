using System.Numerics;

namespace plane.Graphics;

[StructAlign16]
public partial struct VertexShaderBuffer
{
    public Matrix4x4 ViewProjection; // 64 bytes
    public Matrix4x4 World; // 128 bytes
}

[StructAlign16]
public partial struct PixelShaderBuffer
{
    public float TimeElapsed;
}

[StructAlign16]
public partial struct ComputeShaderBuffer
{
    public int BlurSize;

    public ComputeShaderBuffer()
    {
        BlurSize = 4;
    }
}