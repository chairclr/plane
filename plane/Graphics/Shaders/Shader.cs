using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;

namespace plane.Graphics.Shaders;

public abstract class Shader
{
    internal ComPtr<ID3D10Blob> ShaderData = default;
}
