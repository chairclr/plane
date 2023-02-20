using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Assimp;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace plane.Graphics;

public unsafe class Texture3D : IDisposable, IMappable
{
    private readonly Renderer Renderer;

    public ComPtr<ID3D11Texture3D> NativeTexture = default;

    public readonly TextureType TextureType;

    public Format Format = Format.FormatUnknown;

    public int Width = 0;

    public int Height = 0;

    public int Depth = 0;

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

    internal Texture3D(Renderer renderer, Texture3DDesc desc, TextureType textureType)
    {
        Renderer = renderer;

        TextureType = textureType;

        Format = desc.Format;

        Width = (int)desc.Width;

        Height = (int)desc.Height;

        Depth = (int)desc.Depth;

        SilkMarshal.ThrowHResult(Renderer.Device.CreateTexture3D(desc, null, ref NativeTexture));
    }

    public Texture3D(Renderer renderer, int width, int height, int depth, TextureType textureType, BindFlag bindFlags = BindFlag.ShaderResource, Format format = Format.FormatR8G8B8A8Unorm, Usage usage = Usage.Default, CpuAccessFlag cpuAccessFlags = CpuAccessFlag.None, uint mipLevels = 1, uint miscFlag = 0)
    {
        Renderer = renderer;

        TextureType = textureType;

        Format = format;

        Width = width;

        Height = height;

        Depth = depth;

        Texture3DDesc desc = new Texture3DDesc()
        {
            Width = (uint)width,
            Height = (uint)height,
            Depth = (uint)depth,
            Format = format,
            BindFlags = (uint)bindFlags,
            Usage = usage,
            CPUAccessFlags = (uint)cpuAccessFlags,
            MiscFlags = miscFlag,
            MipLevels = mipLevels
        };

        SilkMarshal.ThrowHResult(Renderer.Device.CreateTexture3D(desc, null, ref NativeTexture));
    }

    public Texture3D(Renderer renderer, int width, int height, int depth, TextureType textureType, SubresourceData subresourceData, BindFlag bindFlags = BindFlag.ShaderResource, Format format = Format.FormatR8G8B8A8Unorm, Usage usage = Usage.Default, CpuAccessFlag cpuAccessFlags = CpuAccessFlag.None, uint mipLevels = 1, uint miscFlag = 0)
    {
        Renderer = renderer;

        TextureType = textureType;

        Format = format;

        Width = width;

        Height = height;

        Depth = depth;

        Texture3DDesc desc = new Texture3DDesc()
        {
            Width = (uint)width,
            Height = (uint)height,
            Depth = (uint)depth,
            Format = format,
            BindFlags = (uint)bindFlags,
            Usage = usage,
            CPUAccessFlags = (uint)cpuAccessFlags,
            MiscFlags = miscFlag,
            MipLevels = mipLevels,
        };

        SilkMarshal.ThrowHResult(Renderer.Device.CreateTexture3D(desc, subresourceData, ref NativeTexture));
    }

    public Texture3D(Renderer renderer, int width, int height, int depth,  Usage usage = Usage.Default)
        : this(renderer, width, height, depth, TextureType.Diffuse, usage: usage)
    {

    }

    public Texture3D(Renderer renderer, int width, int height, int depth, SubresourceData subresourceData)
        : this(renderer, width, height, depth, TextureType.Diffuse, subresourceData, usage: Usage.Dynamic, cpuAccessFlags: CpuAccessFlag.Write)
    {

    }

    public Texture3D(Renderer renderer, TextureType textureType)
    {
        Renderer = renderer;

        TextureType = textureType;
    }

    public void CacheDescription()
    {
        Texture3DDesc desc = GetTextureDescription();

        Format = desc.Format;

        Width = (int)desc.Width;

        Height = (int)desc.Height;

        Depth = (int)desc.Depth;
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
        if (Format == Format.FormatUnknown || Width == 0 || Height == 0 || Depth == 0)
        {
            CacheDescription();
        }

        return new ReadOnlySpan<T>(MapRead(subresource).PData, Width * Height * Depth);
    }

    public Span<T> MapWriteSpan<T>(int subresource = 0)
    {
        if (Format == Format.FormatUnknown || Width == 0 || Height == 0 || Depth == 0)
        {
            CacheDescription();
        }

        return new Span<T>(MapWrite(subresource).PData, Width * Height * Depth);
    }

    public void Unmap()
    {
        Renderer.Context.Unmap(NativeTexture, 0);
    }

    internal Texture3DDesc GetTextureDescription()
    {
        Texture3DDesc textureDesc = new Texture3DDesc();

        NativeTexture.GetDesc(ref textureDesc);

        return textureDesc;
    }

    internal ComPtr<ID3D11ShaderResourceView> CreateShaderResourceView()
    {
        ComPtr<ID3D11ShaderResourceView> resourceView = default;

        Texture3DDesc textureDesc = GetTextureDescription();

        ShaderResourceViewDesc shaderResourceViewDesc = new ShaderResourceViewDesc()
        {
            Format = textureDesc.Format,
            ViewDimension = D3DSrvDimension.D3D101SrvDimensionTexture3D,
        };

        shaderResourceViewDesc.Texture3D.MipLevels = textureDesc.MipLevels;

        SilkMarshal.ThrowHResult(Renderer.Device.CreateShaderResourceView(NativeTexture, shaderResourceViewDesc, ref resourceView));

        return resourceView;
    }

    internal ComPtr<ID3D11UnorderedAccessView> CreateUnorderedAccessView()
    {
        ComPtr<ID3D11UnorderedAccessView> accessView = default;

        Texture3DDesc textureDesc = GetTextureDescription();

        UnorderedAccessViewDesc unorderedAccessViewDesc = new UnorderedAccessViewDesc()
        {
            Format = textureDesc.Format,
            ViewDimension = UavDimension.Texture3D,
        };

        unorderedAccessViewDesc.Texture3D.WSize = uint.MaxValue;

        SilkMarshal.ThrowHResult(Renderer.Device.CreateUnorderedAccessView(NativeTexture, unorderedAccessViewDesc, ref accessView));

        return accessView;
    }

    public void Dispose()
    {
        NativeTexture.Dispose();

        _shaderResourceView.Dispose();
        
        GC.SuppressFinalize(this);
    }
}