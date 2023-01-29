using Silk.NET.Core.Native;

namespace plane.Graphics.Shaders;

public abstract class Shader
{
    internal ComPtr<ID3D10Blob> ShaderData = default;

    internal abstract void Create(Renderer renderer);
}