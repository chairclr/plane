using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace plane.Graphics;

public unsafe class Buffer<T> : IDisposable 
    where T : unmanaged 
{
    public ComPtr<ID3D11Device> Device = default;

    public ComPtr<ID3D11Buffer> DataBuffer = default;

    /// <summary>
    /// Number of elements in buffer
    /// </summary>
    public uint Length;

    /// <summary>
    /// Size of buffer data in bytes
    /// </summary>
    public uint Size;

    /// <summary>
    /// The size of a single element in the buffer
    /// </summary>
    public uint Stride;

    public Buffer(Renderer renderer, ReadOnlySpan<T> data, BindFlag bindFlag, Usage usage = Usage.Default, CpuAccessFlag cpuAccessFlags = CpuAccessFlag.None, ResourceMiscFlag resourceMiscFlags = ResourceMiscFlag.None)
    {
        Length = (uint)data.Length;
        Stride = (uint)Unsafe.SizeOf<T>();
        Size = Length * Stride;
        Device = renderer.Device;

        BufferDesc bufferDesc = new BufferDesc()
        {
            Usage = usage,
            ByteWidth = Size,
            StructureByteStride = Stride,
            BindFlags = (uint)bindFlag,
            CPUAccessFlags = (uint)cpuAccessFlags,
            MiscFlags = (uint)resourceMiscFlags
        };

        fixed (void* bufferData = &data[0])
        {
            SubresourceData bufferSubresource = new SubresourceData()
            {
                PSysMem = bufferData
            };

            SilkMarshal.ThrowHResult(Device.Get().CreateBuffer(bufferDesc, bufferSubresource, DataBuffer.GetAddressOf()));
        }
    }
    public Buffer(Renderer renderer, ref T data, BindFlag bindFlag, Usage usage = Usage.Default, CpuAccessFlag cpuAccessFlags = CpuAccessFlag.None, ResourceMiscFlag resourceMiscFlags = ResourceMiscFlag.None)
        : this(renderer, new ReadOnlySpan<T>(Unsafe.AsPointer(ref data), 1), bindFlag, usage, cpuAccessFlags, resourceMiscFlags)
    {

    }

    public void WriteData(Renderer renderer, ReadOnlySpan<T> data)
    {
        MappedSubresource mappedSubresource = new MappedSubresource();
        SilkMarshal.ThrowHResult(renderer.Context.Get().Map((ID3D11Resource*)DataBuffer.Handle, 0, Map.WriteDiscard, 0, ref mappedSubresource));

        Span<T> subresourceSpan = new Span<T>(mappedSubresource.PData, data.Length);

        data.CopyTo(subresourceSpan);

        renderer.Context.Get().Unmap((ID3D11Resource*)DataBuffer.Handle, 0);
    }

    public void WriteData(Renderer renderer, ReadOnlySpan<T> data, uint subresource, Map mapType, MapFlag mapFlags)
    {
        MappedSubresource mappedSubresource = new MappedSubresource();
        renderer.Context.Get().Map((ID3D11Resource*)DataBuffer.Handle, subresource, mapType, (uint)mapFlags, ref mappedSubresource);

        Span<T> subresourceSpan = new Span<T>(mappedSubresource.PData, data.Length);

        data.CopyTo(subresourceSpan);

        renderer.Context.Get().Unmap((ID3D11Resource*)DataBuffer.Handle, 0);
    }

    public void WriteData(Renderer renderer, ref T data) => WriteData(renderer, new ReadOnlySpan<T>(Unsafe.AsPointer(ref data), 1));

    public void WriteData(Renderer renderer, ref T data, uint subresource, Map mapType, MapFlag mapFlags) => WriteData(renderer, new ReadOnlySpan<T>(Unsafe.AsPointer(ref data), 1), subresource, mapType, mapFlags);

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Device.Dispose();

        DataBuffer.Dispose();
    }
}
