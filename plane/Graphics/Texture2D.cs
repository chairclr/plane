using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace plane.Graphics;

public unsafe class Texture2D : IDisposable
{
    private readonly Renderer Renderer;

    public readonly TextureType TextureType;

    internal ComPtr<ID3D11Texture2D> NativeTexture = default;

    internal Format Format = Format.FormatUnknown;

    private ComPtr<ID3D11ShaderResourceView> _shaderResourceView = default;

    internal ref ComPtr<ID3D11ShaderResourceView> ShaderResourceView
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

    

    internal Texture2D(Renderer renderer, Texture2DDesc desc, TextureType textureType)
    {
        Renderer = renderer;

        TextureType = textureType;

        SilkMarshal.ThrowHResult(Renderer.Device.CreateTexture2D(desc, null, ref NativeTexture));
    }

    public Texture2D(Renderer renderer, int width, int height, TextureType textureType, SampleDesc? sampleDesc = null, BindFlag bindFlags = BindFlag.ShaderResource, Format format = Format.FormatR8G8B8A8Unorm, Usage usage = Usage.Default, CpuAccessFlag cpuAccessFlags = CpuAccessFlag.None, uint arraySize = 1, uint mipLevels = 1, uint miscFlag = 0)
    {
        Renderer = renderer;

        TextureType = textureType;

        Format = format;

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

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        NativeTexture.Dispose();

        _shaderResourceView.Dispose();
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