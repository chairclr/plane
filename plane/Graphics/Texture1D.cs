using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using SixLabors.ImageSharp.PixelFormats;

namespace plane.Graphics;

public unsafe class Texture1D : IDisposable, IMappable
{
    private readonly Renderer Renderer;

    public ComPtr<ID3D11Texture1D> NativeTexture = default;

    public readonly TextureType TextureType;

    public Format Format = Format.FormatUnknown;

    public int Width = 0;

    private ComPtr<ID3D11ShaderResourceView> _shaderResourceView = default;

    private ComPtr<ID3D11UnorderedAccessView> _unorderedAccessView = default;

    public ref ComPtr<ID3D11ShaderResourceView> ShaderResourceView
    {
        get
        {
            if (Unsafe.IsNullRef(ref _shaderResourceView.Get()))
            {
                _shaderResourceView = CreateShaderResourceView();
            }

            return ref _shaderResourceView;
        }
    }

    public ref ComPtr<ID3D11UnorderedAccessView> UnorderedAccessView
    {
        get
        {
            if (Unsafe.IsNullRef(ref _unorderedAccessView.Get()))
            {
                _unorderedAccessView = CreateUnorderedAccessView();
            }

            return ref _unorderedAccessView;
        }
    }

    internal Texture1D(Renderer renderer, Texture1DDesc desc, TextureType textureType)
    {
        Renderer = renderer;

        TextureType = textureType;

        Format = desc.Format;

        Width = (int)desc.Width;

        SilkMarshal.ThrowHResult(Renderer.Device.CreateTexture1D(desc, null, ref NativeTexture));
    }

    public Texture1D(Renderer renderer, int width, TextureType textureType, BindFlag bindFlags = BindFlag.ShaderResource, Format format = Format.FormatR8G8B8A8Unorm, Usage usage = Usage.Default, CpuAccessFlag cpuAccessFlags = CpuAccessFlag.None, uint arraySize = 1, uint mipLevels = 1, uint miscFlag = 0)
    {
        Renderer = renderer;

        TextureType = textureType;

        Format = format;

        Width = width;

        Texture1DDesc desc = new Texture1DDesc()
        {
            Width = (uint)width,
            Format = format,
            BindFlags = (uint)bindFlags,
            Usage = usage,
            CPUAccessFlags = (uint)cpuAccessFlags,
            MiscFlags = miscFlag,
            ArraySize = arraySize,
            MipLevels = mipLevels,
        };

        SilkMarshal.ThrowHResult(Renderer.Device.CreateTexture1D(desc, null, ref NativeTexture));
    }

    public Texture1D(Renderer renderer, int width, TextureType textureType, SubresourceData subresourceData, BindFlag bindFlags = BindFlag.ShaderResource, Format format = Format.FormatR8G8B8A8Unorm, Usage usage = Usage.Default, CpuAccessFlag cpuAccessFlags = CpuAccessFlag.None, uint arraySize = 1, uint mipLevels = 1, uint miscFlag = 0)
    {
        Renderer = renderer;

        TextureType = textureType;

        Format = format;

        Width = width;

        Texture1DDesc desc = new Texture1DDesc()
        {
            Width = (uint)width,
            Format = format,
            BindFlags = (uint)bindFlags,
            Usage = usage,
            CPUAccessFlags = (uint)cpuAccessFlags,
            MiscFlags = miscFlag,
            ArraySize = arraySize,
            MipLevels = mipLevels
        };

        SilkMarshal.ThrowHResult(Renderer.Device.CreateTexture1D(desc, subresourceData, ref NativeTexture));
    }

    public Texture1D(Renderer renderer, Span<Rgba32> colors, TextureType textureType, BindFlag bindFlags = BindFlag.ShaderResource, Usage usage = Usage.Default, CpuAccessFlag cpuAccessFlags = CpuAccessFlag.None, uint arraySize = 1, uint mipLevels = 1, uint miscFlag = 0)
    {
        Renderer = renderer;

        TextureType = textureType;

        Format = Format.FormatR8G8B8A8Unorm;

        Width = colors.Length;

        Texture1DDesc desc = new Texture1DDesc()
        {
            Width = (uint)colors.Length,
            Format = Format.FormatR8G8B8A8Unorm,
            BindFlags = (uint)bindFlags,
            Usage = usage,
            CPUAccessFlags = (uint)cpuAccessFlags,
            MiscFlags = miscFlag,
            ArraySize = arraySize,
            MipLevels = mipLevels
        };

        SubresourceData subresourceData = new SubresourceData()
        {
            PSysMem = Unsafe.AsPointer(ref MemoryMarshal.AsBytes(colors)[0]),
            SysMemPitch = (uint)(colors.Length * Unsafe.SizeOf<Rgba32>()),
        };

        SilkMarshal.ThrowHResult(Renderer.Device.CreateTexture1D(desc, subresourceData, ref NativeTexture));
    }

    public Texture1D(Renderer renderer, int width, Usage usage = Usage.Default)
        : this(renderer, width, TextureType.Diffuse, usage: usage)
    {

    }

    public Texture1D(Renderer renderer, int width, SubresourceData subresourceData)
        : this(renderer, width, TextureType.Diffuse, subresourceData, usage: Usage.Dynamic, cpuAccessFlags: CpuAccessFlag.Write)
    {

    }

    public Texture1D(Renderer renderer, Span<Rgba32> colors)
        : this(renderer, colors, TextureType.Diffuse, usage: Usage.Dynamic, cpuAccessFlags: CpuAccessFlag.Write)
    {

    }

    public Texture1D(Renderer renderer, TextureType textureType)
    {
        Renderer = renderer;

        TextureType = textureType;
    }

    public void CacheDescription()
    {
        Texture1DDesc desc = GetTextureDescription();

        Format = desc.Format;

        Width = (int)desc.Width;
    }

    public MappedSubresource MapRead(int subresource = 0)
    {
        MappedSubresource mappedSubresource = new MappedSubresource();

        SilkMarshal.ThrowHResult(Renderer.Context.Map(NativeTexture, (uint)subresource, Map.Read, 0, ref mappedSubresource));

        return mappedSubresource;
    }

    public MappedSubresource MapWrite(int subresource = 0)
    {
        MappedSubresource mappedSubresource = new MappedSubresource();

        SilkMarshal.ThrowHResult(Renderer.Context.Map(NativeTexture, (uint)subresource, Map.Write, 0, ref mappedSubresource));

        return mappedSubresource;
    }

    public ReadOnlySpan<T> MapReadSpan<T>(int subresource = 0)
    {
        if (Format == Format.FormatUnknown || Width == 0)
        {
            CacheDescription();
        }

        return new ReadOnlySpan<T>(MapRead(subresource).PData, Width);
    }

    public Span<T> MapWriteSpan<T>(int subresource = 0)
    {
        if (Format == Format.FormatUnknown || Width == 0)
        {
            CacheDescription();
        }

        return new Span<T>(MapWrite(subresource).PData, Width);
    }

    public void Unmap()
    {
        Renderer.Context.Unmap(NativeTexture, 0);
    }

    public Texture1DDesc GetTextureDescription()
    {
        Texture1DDesc textureDesc = new Texture1DDesc();

        NativeTexture.GetDesc(ref textureDesc);

        return textureDesc;
    }

    internal ComPtr<ID3D11ShaderResourceView> CreateShaderResourceView()
    {
        ComPtr<ID3D11ShaderResourceView> resourceView = default;

        Texture1DDesc textureDesc = GetTextureDescription();

        ShaderResourceViewDesc shaderResourceViewDesc = new ShaderResourceViewDesc()
        {
            Format = textureDesc.Format,
            ViewDimension = D3DSrvDimension.D3D101SrvDimensionTexture1D,
        };

        shaderResourceViewDesc.Texture2D.MipLevels = textureDesc.MipLevels;

        SilkMarshal.ThrowHResult(Renderer.Device.CreateShaderResourceView(NativeTexture, shaderResourceViewDesc, ref resourceView));

        return resourceView;
    }

    internal ComPtr<ID3D11UnorderedAccessView> CreateUnorderedAccessView()
    {
        ComPtr<ID3D11UnorderedAccessView> accessView = default;

        Texture1DDesc textureDesc = GetTextureDescription();

        UnorderedAccessViewDesc unorderedAccessViewDesc = new UnorderedAccessViewDesc()
        {
            Format = textureDesc.Format,
            ViewDimension = UavDimension.Texture1D,
        };

        SilkMarshal.ThrowHResult(Renderer.Device.CreateUnorderedAccessView(NativeTexture, unorderedAccessViewDesc, ref accessView));

        return accessView;
    }

    public void Dispose()
    {
        NativeTexture.Dispose();

        _shaderResourceView.Dispose();

        GC.SuppressFinalize(this);
    }

    public static Texture1D GetSolidColorTexture(Renderer renderer, int width, Rgba32 color)
    {
        Rgba32[] colors = new Rgba32[width];

        for (int i = 0; i < width; i++)
        {
            colors[i] = color;
        }

        return new Texture1D(renderer, colors);
    }

    public static Texture1D GetSinglePixelTexture(Renderer renderer, Rgba32 color) => GetSolidColorTexture(renderer, 1, color);
}