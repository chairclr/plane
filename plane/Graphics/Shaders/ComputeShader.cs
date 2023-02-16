using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Graphics.Shaders;

public class ComputeShader : Shader, IDisposable
{
    internal ComPtr<ID3D11ComputeShader> NativeShader = default;

    internal unsafe override void Create(Renderer renderer)
    {
        SilkMarshal.ThrowHResult(renderer.Device.CreateComputeShader(ShaderData.BufferPointer, ShaderData.Size, ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref NativeShader));
    }

    public unsafe override void Bind(Renderer renderer)
    {
        renderer.Context.CSSetShader(NativeShader, null, 0);
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