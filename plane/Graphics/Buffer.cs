using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace plane.Graphics;

public unsafe class Buffer<T> : IDisposable 
    where T : unmanaged 
{
    private readonly Renderer Renderer;

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
        Renderer = renderer;

        Length = (uint)data.Length;

        Stride = (uint)Unsafe.SizeOf<T>();

        Size = Length * Stride;

        BufferDesc bufferDesc = new BufferDesc()
        {
            Usage = usage,
            ByteWidth = Size,
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

            SilkMarshal.ThrowHResult(Renderer.Device.CreateBuffer(bufferDesc, bufferSubresource, ref DataBuffer));
        }
    }

    public Buffer(Renderer renderer, ref T data, BindFlag bindFlag, Usage usage = Usage.Default, CpuAccessFlag cpuAccessFlags = CpuAccessFlag.None, ResourceMiscFlag resourceMiscFlags = ResourceMiscFlag.None)
        : this(renderer, new ReadOnlySpan<T>(Unsafe.AsPointer(ref data), 1), bindFlag, usage, cpuAccessFlags, resourceMiscFlags)
    {

    }

    public void WriteData(ReadOnlySpan<T> data)
    {
        MappedSubresource mappedSubresource = new MappedSubresource();

        SilkMarshal.ThrowHResult(Renderer.Context.Map(DataBuffer, 0, Map.WriteDiscard, 0, ref mappedSubresource));

        Span<T> subresourceSpan = new Span<T>(mappedSubresource.PData, data.Length);

        data.CopyTo(subresourceSpan);

        Renderer.Context.Unmap(DataBuffer, 0);
    }

    public void WriteData(ReadOnlySpan<T> data, uint subresource, Map mapType, MapFlag mapFlags)
    {
        MappedSubresource mappedSubresource = new MappedSubresource();

        Renderer.Context.Map(DataBuffer, subresource, mapType, (uint)mapFlags, ref mappedSubresource);

        Span<T> subresourceSpan = new Span<T>(mappedSubresource.PData, data.Length);

        data.CopyTo(subresourceSpan);

        Renderer.Context.Unmap(DataBuffer, 0);
    }

    public void WriteData(ref T data) => WriteData(new ReadOnlySpan<T>(Unsafe.AsPointer(ref data), 1));

    public void WriteData(ref T data, uint subresource, Map mapType, MapFlag mapFlags) => WriteData(new ReadOnlySpan<T>(Unsafe.AsPointer(ref data), 1), subresource, mapType, mapFlags);

    public void Bind(int slot, BindTo to)
    {
        switch (to)
        {
            case BindTo.VertexShader:
                Renderer.Context.VSSetConstantBuffers((uint)slot, 1, ref DataBuffer);
                break;
            case BindTo.PixelShader:
                Renderer.Context.PSSetConstantBuffers((uint)slot, 1, ref DataBuffer);
                break;
            case BindTo.GeometryShader:
                Renderer.Context.GSSetConstantBuffers((uint)slot, 1, ref DataBuffer);
                break;
            case BindTo.ComputeShader:
                Renderer.Context.CSSetConstantBuffers((uint)slot, 1, ref DataBuffer);
                break;
            case BindTo.HullShader:
                Renderer.Context.HSSetConstantBuffers((uint)slot, 1, ref DataBuffer);
                break;
            case BindTo.DomainShader:
                Renderer.Context.DSSetConstantBuffers((uint)slot, 1, ref DataBuffer);
                break;
            default:
                throw new ArgumentException($"Invalid binding target {to}", nameof(to));
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        DataBuffer.Dispose();
    }
}
