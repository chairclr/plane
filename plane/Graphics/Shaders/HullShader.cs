using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Graphics.Shaders;

public class HullShader : Shader, IDisposable
{
    internal ComPtr<ID3D11HullShader> NativeShader = default;

    internal unsafe override void Create(Renderer renderer)
    {
        SilkMarshal.ThrowHResult(renderer.Device.Get().CreateHullShader(ShaderData.Get().GetBufferPointer(), ShaderData.Get().GetBufferSize(), null, NativeShader.GetAddressOf()));
    }

    public new void Dispose()
    {
        GC.SuppressFinalize(this);

        NativeShader.Dispose();
    }
}