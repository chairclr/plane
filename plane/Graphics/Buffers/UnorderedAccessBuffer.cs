using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace plane.Graphics.Buffers;

public unsafe class UnorderedAccessBuffer<T> : Buffer<T>, IDisposable
    where T : unmanaged
{
    public uint Length { get; private set; }

    public uint Stride => (uint)Unsafe.SizeOf<T>();

    public uint Size => Length * Stride;

    public Format Format { get; private set; }

    private readonly bool Writable;

    private ComPtr<ID3D11UnorderedAccessView> _unorderedAccessView;

    public ComPtr<ID3D11UnorderedAccessView> UnorderedAccessView => _unorderedAccessView;

    public UnorderedAccessBuffer(Renderer renderer, Span<T> data, Format format, bool writable = false)
        : base(renderer)
    {
        Length = (uint)data.Length;

        Format = format;

        Writable = writable;

        BufferDesc bufferDesc = new BufferDesc()
        {
            BindFlags = (uint)BindFlag.UnorderedAccess,
            Usage = Writable ? Usage.Dynamic : Usage.Default,
            ByteWidth = Size,
            CPUAccessFlags = (uint)(Writable ? CpuAccessFlag.Write : CpuAccessFlag.None) 
        };

        SubresourceData subresourceData = new SubresourceData()
        {
            PSysMem = Unsafe.AsPointer(ref data[0])
        };

        SilkMarshal.ThrowHResult(Renderer!.Device.CreateBuffer(bufferDesc, subresourceData, ref NativeBuffer));

        UnorderedAccessViewDesc unorderedAccessViewDesc = new UnorderedAccessViewDesc()
        {
            Format = format,
            ViewDimension = UavDimension.Buffer
        };

        unorderedAccessViewDesc.Buffer.FirstElement = 0;
        unorderedAccessViewDesc.Buffer.NumElements = Length;

        SilkMarshal.ThrowHResult(Renderer!.Device.CreateUnorderedAccessView(NativeBuffer, unorderedAccessViewDesc, ref _unorderedAccessView));
    }

    public void WriteData(Span<T> data)
    {
        if (!Writable) throw new Exception("Buffer not writable.");

        MappedSubresource mappedSubresource = new MappedSubresource();

        SilkMarshal.ThrowHResult(Renderer.Context.Map(NativeBuffer, 0, Map.WriteDiscard, 0, ref mappedSubresource));

        Span<T> mappedData = new Span<T>(mappedSubresource.PData, (int)Length);
        mappedData.CopyTo(data);

        Renderer.Context.Unmap(NativeBuffer, 0);
    }

    public void Bind(int slot)
    {
        Renderer.Context.CSSetUnorderedAccessViews((uint)slot, 1, ref _unorderedAccessView, (uint*)null);
    }
}
