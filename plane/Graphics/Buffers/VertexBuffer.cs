using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Graphics.Buffers;

public unsafe class VertexBuffer<T> : Buffer<T>, IDisposable
    where T : unmanaged
{
    public uint Length { get; private set; }

    public uint Stride => (uint)Unsafe.SizeOf<T>();

    public uint Size => Length * Stride;

    public VertexBuffer(Renderer renderer, Span<T> vertexData)
        : base(renderer)
    {
        Length = (uint)vertexData.Length;

        BufferDesc bufferDesc = new BufferDesc()
        {
            Usage = Usage.Default,
            ByteWidth = Size,
            BindFlags = (uint)BindFlag.VertexBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.None,
            MiscFlags = (uint)ResourceMiscFlag.None
        };

        SubresourceData subresourceData = new SubresourceData()
        {
            PSysMem = Unsafe.AsPointer(ref vertexData[0])
        };

        SilkMarshal.ThrowHResult(Renderer.Device.CreateBuffer(bufferDesc, subresourceData, ref NativeBuffer));
    }
}
