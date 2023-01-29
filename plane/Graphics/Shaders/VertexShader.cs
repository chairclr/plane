using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace plane.Graphics.Shaders;

public class VertexShader : Shader
{
    internal ComPtr<ID3D11VertexShader> NativeShader = default;
    internal ComPtr<ID3D11InputLayout> NativeInputLayout = default;

    internal unsafe override void Create(Renderer renderer)
    {
        SilkMarshal.ThrowHResult(renderer.Device.Get().CreateVertexShader(ShaderData.Get().GetBufferPointer(), ShaderData.Get().GetBufferSize(), null, NativeShader.GetAddressOf()));
    }

    internal unsafe void SetInputLayout(Renderer renderer, Span<InputElementDesc> inputLayout)
    {
        fixed (InputElementDesc* layoutPtr = &inputLayout[0])
        {
            SilkMarshal.ThrowHResult(renderer.Device.Get().CreateInputLayout(layoutPtr, (uint)inputLayout.Length, ShaderData.Get().GetBufferPointer(), ShaderData.Get().GetBufferSize(), NativeInputLayout.GetAddressOf()));
        }
    }
}