using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace plane.Graphics.Buffers;

public unsafe class ShaderResourceBuffer<T> : Buffer, IDisposable
    where T : unmanaged
{
    public uint Length { get; private set; }

    public uint Stride => (uint)Unsafe.SizeOf<T>();

    public uint Size => Length * Stride;

    public Format Format { get; private set; }

    private readonly bool Writable;

    private ComPtr<ID3D11ShaderResourceView> _shaderResourceView;

    public ComPtr<ID3D11ShaderResourceView> ShaderResourceView => _shaderResourceView;

    public ShaderResourceBuffer(Renderer renderer, Span<T> data, Format format, bool writable = false)
        : base(renderer)
    {
        Length = (uint)data.Length;

        Format = format;

        Writable = writable;

        BufferDesc bufferDesc = new BufferDesc()
        {
            BindFlags = (uint)BindFlag.ShaderResource,
            Usage = Writable ? Usage.Dynamic : Usage.Default,
            ByteWidth = Size,
            CPUAccessFlags = (uint)(Writable ? CpuAccessFlag.Write : CpuAccessFlag.None)
        };

        SubresourceData subresourceData = new SubresourceData()
        {
            PSysMem = Unsafe.AsPointer(ref data[0])
        };

        SilkMarshal.ThrowHResult(Renderer!.Device.CreateBuffer(bufferDesc, subresourceData, ref NativeBuffer));

        ShaderResourceViewDesc shaderResourceViewDesc = new ShaderResourceViewDesc()
        {
            Format = format,
            ViewDimension = D3DSrvDimension.D3DSrvDimensionBuffer
        };

        shaderResourceViewDesc.Buffer.FirstElement = 0;
        shaderResourceViewDesc.Buffer.NumElements = Length;

        SilkMarshal.ThrowHResult(Renderer!.Device.CreateShaderResourceView(NativeBuffer, shaderResourceViewDesc, ref _shaderResourceView));
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

    public void Bind(int slot, BindTo to)
    {
        switch (to)
        {
            case BindTo.VertexShader:
                Renderer.Context.VSSetShaderResources((uint)slot, 1, ref _shaderResourceView);
                break;
            case BindTo.PixelShader:
                Renderer.Context.PSSetShaderResources((uint)slot, 1, ref _shaderResourceView);
                break;
            case BindTo.GeometryShader:
                Renderer.Context.GSSetShaderResources((uint)slot, 1, ref _shaderResourceView);
                break;
            case BindTo.ComputeShader:
                Renderer.Context.CSSetShaderResources((uint)slot, 1, ref _shaderResourceView);
                break;
            case BindTo.HullShader:
                Renderer.Context.HSSetShaderResources((uint)slot, 1, ref _shaderResourceView);
                break;
            case BindTo.DomainShader:
                Renderer.Context.DSSetShaderResources((uint)slot, 1, ref _shaderResourceView);
                break;
            default:
                throw new ArgumentException($"Invalid binding target {to}.", nameof(to));
        }
    }

    public override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            ShaderResourceView.Dispose();
        }
    }
}