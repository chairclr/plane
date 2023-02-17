using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Graphics.Buffers;

public unsafe class ConstantBuffer<T> : Buffer<T>, IDisposable
    where T : unmanaged
{
    public T Data;

    public ConstantBuffer(Renderer renderer)
        : base(renderer)
    {
        Data = new T();

        BufferDesc bufferDesc = new BufferDesc()
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)Unsafe.SizeOf<T>(),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = (uint)ResourceMiscFlag.None
        };

        SubresourceData subresourceData = new SubresourceData()
        {
            PSysMem = Unsafe.AsPointer(ref Data)
        };

        SilkMarshal.ThrowHResult(Renderer.Device.CreateBuffer(bufferDesc, subresourceData, ref NativeBuffer));
    }

    public void WriteData()
    {
        MappedSubresource mappedSubresource = new MappedSubresource();

        SilkMarshal.ThrowHResult(Renderer.Context.Map(NativeBuffer, 0, Map.WriteDiscard, 0, ref mappedSubresource));

        Unsafe.AsRef<T>(mappedSubresource.PData) = Data;

        Renderer.Context.Unmap(NativeBuffer, 0);
    }

    public void Bind(int slot, BindTo to)
    {
        switch (to)
        {
            case BindTo.VertexShader:
                Renderer.Context.VSSetConstantBuffers((uint)slot, 1, ref NativeBuffer);
                break;
            case BindTo.PixelShader:
                Renderer.Context.PSSetConstantBuffers((uint)slot, 1, ref NativeBuffer);
                break;
            case BindTo.GeometryShader:
                Renderer.Context.GSSetConstantBuffers((uint)slot, 1, ref NativeBuffer);
                break;
            case BindTo.ComputeShader:
                Renderer.Context.CSSetConstantBuffers((uint)slot, 1, ref NativeBuffer);
                break;
            case BindTo.HullShader:
                Renderer.Context.HSSetConstantBuffers((uint)slot, 1, ref NativeBuffer);
                break;
            case BindTo.DomainShader:
                Renderer.Context.DSSetConstantBuffers((uint)slot, 1, ref NativeBuffer);
                break;
            default:
                throw new ArgumentException($"Invalid binding target {to}.", nameof(to));
        }
    }
}
