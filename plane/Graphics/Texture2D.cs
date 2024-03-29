﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace plane.Graphics;

public unsafe class Texture2D : IDisposable, IMappable
{
    private readonly Renderer Renderer;

    public ComPtr<ID3D11Texture2D> NativeTexture = default;

    public readonly TextureType TextureType;

    public Format Format = Format.FormatUnknown;

    public int Width = 0;

    public int Height = 0;

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

    internal Texture2D(Renderer renderer, Texture2DDesc desc, TextureType textureType)
    {
        Renderer = renderer;

        TextureType = textureType;

        Format = desc.Format;

        Width = (int)desc.Width;

        Height = (int)desc.Height;

        SilkMarshal.ThrowHResult(Renderer.Device.CreateTexture2D(desc, null, ref NativeTexture));
    }

    public Texture2D(Renderer renderer, int width, int height, TextureType textureType, SampleDesc? sampleDesc = null, BindFlag bindFlags = BindFlag.ShaderResource, Format format = Format.FormatR8G8B8A8Unorm, Usage usage = Usage.Default, CpuAccessFlag cpuAccessFlags = CpuAccessFlag.None, uint arraySize = 1, uint mipLevels = 1, uint miscFlag = 0)
    {
        Renderer = renderer;

        TextureType = textureType;

        Format = format;

        Width = width;

        Height = height;

        sampleDesc ??= new SampleDesc(1, 0);

        Texture2DDesc desc = new Texture2DDesc()
        {
            Width = (uint)width,
            Height = (uint)height,
            Format = format,
            SampleDesc = sampleDesc.Value,
            BindFlags = (uint)bindFlags,
            Usage = usage,
            CPUAccessFlags = (uint)cpuAccessFlags,
            MiscFlags = miscFlag,
            ArraySize = arraySize,
            MipLevels = mipLevels
        };

        SilkMarshal.ThrowHResult(Renderer.Device.CreateTexture2D(desc, null, ref NativeTexture));
    }

    public Texture2D(Renderer renderer, int width, int height, TextureType textureType, SubresourceData subresourceData, SampleDesc? sampleDesc = null, BindFlag bindFlags = BindFlag.ShaderResource, Format format = Format.FormatR8G8B8A8Unorm, Usage usage = Usage.Default, CpuAccessFlag cpuAccessFlags = CpuAccessFlag.None, uint arraySize = 1, uint mipLevels = 1, uint miscFlag = 0)
    {
        Renderer = renderer;

        TextureType = textureType;

        Format = format;

        Width = width;

        Height = height;

        sampleDesc ??= new SampleDesc(1, 0);

        Texture2DDesc desc = new Texture2DDesc()
        {
            Width = (uint)width,
            Height = (uint)height,
            Format = format,
            SampleDesc = sampleDesc.Value,
            BindFlags = (uint)bindFlags,
            Usage = usage,
            CPUAccessFlags = (uint)cpuAccessFlags,
            MiscFlags = miscFlag,
            ArraySize = arraySize,
            MipLevels = mipLevels
        };

        SilkMarshal.ThrowHResult(Renderer.Device.CreateTexture2D(desc, subresourceData, ref NativeTexture));
    }

    public Texture2D(Renderer renderer, Image<Rgba32> image, TextureType textureType, SampleDesc? sampleDesc = null, BindFlag bindFlags = BindFlag.ShaderResource, Usage usage = Usage.Default, CpuAccessFlag cpuAccessFlags = CpuAccessFlag.None, uint arraySize = 1, uint mipLevels = 1, uint miscFlag = 0)
    {
        Renderer = renderer;

        TextureType = textureType;

        Format = Format.FormatR8G8B8A8Unorm;

        Width = image.Width;

        Height = image.Height;

        sampleDesc ??= new SampleDesc(1, 0);

        Texture2DDesc desc = new Texture2DDesc()
        {
            Width = (uint)image.Width,
            Height = (uint)image.Height,
            Format = Format.FormatR8G8B8A8Unorm,
            SampleDesc = sampleDesc.Value,
            BindFlags = (uint)bindFlags,
            Usage = usage,
            CPUAccessFlags = (uint)cpuAccessFlags,
            MiscFlags = miscFlag,
            ArraySize = arraySize,
            MipLevels = mipLevels
        };

        Span<Rgba32> imageData = new Span<Rgba32>(new Rgba32[image.Width * image.Height]);

        image.CopyPixelDataTo(imageData);

        SubresourceData subresourceData = new SubresourceData()
        {
            PSysMem = Unsafe.AsPointer(ref MemoryMarshal.AsBytes(imageData)[0]),
            SysMemPitch = (uint)(image.Width * Unsafe.SizeOf<Rgba32>()),
        };

        SilkMarshal.ThrowHResult(Renderer.Device.CreateTexture2D(desc, subresourceData, ref NativeTexture));
    }

    public Texture2D(Renderer renderer, int width, int height, Usage usage = Usage.Default)
        : this(renderer, width, height, TextureType.Diffuse, usage: usage)
    {

    }

    public Texture2D(Renderer renderer, int width, int height, SubresourceData subresourceData)
        : this(renderer, width, height, TextureType.Diffuse, subresourceData, usage: Usage.Dynamic, cpuAccessFlags: CpuAccessFlag.Write)
    {

    }

    public Texture2D(Renderer renderer, Image<Rgba32> image)
        : this(renderer, image, TextureType.Diffuse, usage: Usage.Dynamic, cpuAccessFlags: CpuAccessFlag.Write)
    {

    }

    public Texture2D(Renderer renderer, TextureType textureType)
    {
        Renderer = renderer;

        TextureType = textureType;
    }

    public void CacheDescription()
    {
        Texture2DDesc desc = GetTextureDescription();

        Format = desc.Format;

        Width = (int)desc.Width;

        Height = (int)desc.Height;
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
        if (Format == Format.FormatUnknown || Width == 0 || Height == 0)
        {
            CacheDescription();
        }

        return new ReadOnlySpan<T>(MapRead(subresource).PData, Width * Height);
    }

    public Span<T> MapWriteSpan<T>(int subresource = 0)
    {
        if (Format == Format.FormatUnknown || Width == 0 || Height == 0)
        {
            CacheDescription();
        }

        return new Span<T>(MapWrite(subresource).PData, Width * Height);
    }

    public void Unmap()
    {
        Renderer.Context.Unmap(NativeTexture, 0);
    }

    internal Texture2DDesc GetTextureDescription()
    {
        Texture2DDesc textureDesc = new Texture2DDesc();

        NativeTexture.GetDesc(ref textureDesc);

        return textureDesc;
    }

    internal ComPtr<ID3D11ShaderResourceView> CreateShaderResourceView()
    {
        ComPtr<ID3D11ShaderResourceView> resourceView = default;

        Texture2DDesc textureDesc = GetTextureDescription();

        ShaderResourceViewDesc shaderResourceViewDesc = new ShaderResourceViewDesc()
        {
            Format = textureDesc.Format,
            ViewDimension = D3DSrvDimension.D3D101SrvDimensionTexture2D,
        };

        shaderResourceViewDesc.Texture2D.MipLevels = textureDesc.MipLevels;

        SilkMarshal.ThrowHResult(Renderer.Device.CreateShaderResourceView(NativeTexture, shaderResourceViewDesc, ref resourceView));

        return resourceView;
    }

    internal ComPtr<ID3D11UnorderedAccessView> CreateUnorderedAccessView()
    {
        ComPtr<ID3D11UnorderedAccessView> accessView = default;

        Texture2DDesc textureDesc = GetTextureDescription();

        UnorderedAccessViewDesc unorderedAccessViewDesc = new UnorderedAccessViewDesc()
        {
            Format = textureDesc.Format,
            ViewDimension = UavDimension.Texture2D,
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

    public static Texture2D LoadFromFile(Renderer renderer, string path)
    {
        using Image<Rgba32> image = Image.Load<Rgba32>(path);

        return new Texture2D(renderer, image);
    }

    public static Texture2D GetSolidColorTexture(Renderer renderer, int width, int height, Rgba32 color)
    {
        using Image<Rgba32> image = new Image<Rgba32>(width, height, color);

        return new Texture2D(renderer, image);
    }

    public static Texture2D GetSinglePixelTexture(Renderer renderer, Rgba32 color) => GetSolidColorTexture(renderer, 1, 1, color);
}