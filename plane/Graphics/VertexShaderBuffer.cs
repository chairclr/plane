using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

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