using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Graphics.Shaders;

public class DomainShader : Shader, IDisposable
{
    internal ComPtr<ID3D11DomainShader> NativeShader = default;

    internal unsafe override void Create(Renderer renderer)
    {
        SilkMarshal.ThrowHResult(renderer.Device.CreateDomainShader(ShaderData.GetBufferPointer(), ShaderData.GetBufferSize(), ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref NativeShader));
    }

    public new void Dispose()
    {
        GC.SuppressFinalize(this);

        NativeShader.Dispose();
    }
}