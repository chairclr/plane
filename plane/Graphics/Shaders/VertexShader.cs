using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace plane.Graphics.Shaders;

public class VertexShader : Shader, IDisposable
{
    internal ComPtr<ID3D11VertexShader> NativeShader = default;
    internal ComPtr<ID3D11InputLayout> NativeInputLayout = default;

    internal unsafe override void Create(Renderer renderer)
    {
        SilkMarshal.ThrowHResult(renderer.Device.CreateVertexShader(ShaderData.BufferPointer, ShaderData.Size, ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref NativeShader));
    }

    internal unsafe void SetInputLayout(Renderer renderer, ReadOnlySpan<InputElementDesc> inputLayout)
    {
        SilkMarshal.ThrowHResult(renderer.Device.CreateInputLayout(inputLayout[0], (uint)inputLayout.Length, ShaderData.BufferPointer, ShaderData.Size, ref NativeInputLayout));
    }

    public unsafe override void Bind(Renderer renderer)
    {
        renderer.Context.VSSetShader(NativeShader, null, 0);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            NativeShader.Dispose();
            NativeInputLayout.Dispose();
        }
    }
}