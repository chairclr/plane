using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Graphics.Shaders;

public class VertexShader : Shader
{
    internal ComPtr<ID3D11VertexShader> NativeShader = default;
    internal ComPtr<ID3D11InputLayout> NativeInputLayout = default;
}