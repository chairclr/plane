using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Graphics.Shaders;

public class PixelShader : Shader, IDisposable
{
    internal ComPtr<ID3D11PixelShader> NativeShader = default;

    internal unsafe override void Create(Renderer renderer)
    {
        SilkMarshal.ThrowHResult(renderer.Device.CreatePixelShader(ShaderData.GetBufferPointer(), ShaderData.GetBufferSize(), ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref NativeShader));
    }

    public unsafe override void Bind(Renderer renderer)
    {
        renderer.Context.PSSetShader(NativeShader, null, 0);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            NativeShader.Dispose();
        }
    }
}