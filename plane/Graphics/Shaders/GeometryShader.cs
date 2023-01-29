using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Graphics.Shaders;

public class GeometryShader : Shader
{
    internal ComPtr<ID3D11GeometryShader> NativeShader = default;
}