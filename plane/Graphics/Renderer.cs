using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using plane.Diagnostics;
using plane.Graphics.Providers;
using plane.Graphics.Shaders;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace plane.Graphics;

public unsafe class Renderer : IDisposable
{
    public IWindow Window;

    internal ComPtr<ID3D11Device> Device = default;

    internal ComPtr<ID3D11DeviceContext> Context = default;

    private ComPtr<IDXGISwapChain1> SwapChain = default;

    private Texture2D? BackBuffer;

    private ComPtr<ID3D11RenderTargetView> RenderTargetView = default;

    private Texture2D? MultiSampleBackBuffer;

    private ComPtr<ID3D11RenderTargetView> MultiSampleRenderTargetView = default;

    private Texture2D? DepthBuffer;

    private ComPtr<ID3D11DepthStencilView> DepthStencilView = default;

    private ComPtr<ID3D11DepthStencilState> DepthStencilState = default;

    private Viewport Viewport = default;

    private readonly Rasterizer? Rasterizer;

    // TODO: Implement blend state and alpha blending

    private readonly VertexShader? VertexShader;

    private readonly PixelShader? PixelShader;

    private Sampler PixelShaderSampler;

    public Camera Camera;

    public readonly List<RenderObject> RenderObjects = new List<RenderObject>();

    private PixelShaderBuffer PixelShaderBufferData;

    private readonly Buffer<PixelShaderBuffer> PixelShaderBuffer;

    public Renderer(IWindow window)
    {
        Window = window;

        CreateDeviceAndSwapChain();

        CreateBackBuffer();
        
        CreateDepthBuffer();

        CreateViewport();

        RasterizerDesc rasterizerDesc = new RasterizerDesc()
        {
            MultisampleEnable = 1,
            AntialiasedLineEnable = 1,
            FillMode = FillMode.Solid,
            CullMode = CullMode.Back,
        };

        Rasterizer = new Rasterizer(this, rasterizerDesc);

        string planeRootFolder = Path.GetDirectoryName(typeof(plane.Plane).Assembly.Location)!;

        VertexShader = ShaderCompiler.CompileFromFile<VertexShader>(Path.Combine(planeRootFolder, "Shaders/VertexShader.hlsl"), "VSMain", ShaderModel.VertexShader5_0);
        VertexShader.Create(this);

        InputElementDesc[] vertexLayout =
        {
            new InputElementDesc((byte*)SilkMarshal.StringToPtr("POSITION"), 0, Format.FormatR32G32B32Float, 0, 0,                          InputClassification.PerVertexData),
            new InputElementDesc((byte*)SilkMarshal.StringToPtr("TEXCOORD"), 0, Format.FormatR32G32Float,    0, D3D11.AppendAlignedElement, InputClassification.PerVertexData),
            new InputElementDesc((byte*)SilkMarshal.StringToPtr("NORMAL"),   0, Format.FormatR32G32B32Float, 0, D3D11.AppendAlignedElement, InputClassification.PerVertexData),
        };

        VertexShader.SetInputLayout(this, vertexLayout);

        PixelShader = ShaderCompiler.CompileFromFile<PixelShader>(Path.Combine(planeRootFolder, "Shaders/PixelShader.hlsl"), "PSMain", ShaderModel.PixelShader5_0);
        PixelShader.Create(this);

        PixelShaderSampler = new Sampler(this, new SamplerDesc()
        {
            Filter = Filter.Anisotropic,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            ComparisonFunc = ComparisonFunc.Never,
            MinLOD = 0,
            MaxLOD = float.MaxValue
        });

        PixelShaderBuffer = new Buffer<PixelShaderBuffer>(this, ref PixelShaderBufferData, BindFlag.ConstantBuffer, Usage.Dynamic, CpuAccessFlag.Write);

        Camera = new Camera(window, 70f, 0.2f, 1000f);
    }

    public void Render()
    {
        Context.OMSetRenderTargets(1, ref MultiSampleRenderTargetView, DepthStencilView);
        Context.OMSetDepthStencilState(DepthStencilState, 0);

        float[] clearColor = new float[] { 0.55f, 0.7f, 0.75f, 1f };
        Context.ClearRenderTargetView(MultiSampleRenderTargetView, ref clearColor[0]);
        Context.ClearDepthStencilView(DepthStencilView, (uint)(ClearFlag.Depth | ClearFlag.Stencil), 1.0f, 0);

        Context.IASetInputLayout(VertexShader!.NativeInputLayout);
        Context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D10PrimitiveTopologyTrianglelist);

        Rasterizer!.Bind();
        Context.RSSetViewports(1, Viewport);

        PixelShaderSampler.Bind(0, BindTo.PixelShader);

        VertexShader!.Bind(this);
        PixelShader!.Bind(this);

        PixelShaderBufferData.TimeElapsed += 1f / 144f;

        PixelShaderBuffer.WriteData(ref PixelShaderBufferData);
        PixelShaderBuffer.Bind(0, BindTo.PixelShader);

        for (int i = 0; i < RenderObjects.Count; i++)
        {
            RenderObjects[i].Render(Camera);
        }

        Context.ResolveSubresource(BackBuffer!.NativeTexture, 0, MultiSampleBackBuffer!.NativeTexture, 0, BackBuffer!.Format);

        SwapChain.Present(1, 0);
    }

    internal void Resize()
    {
        Texture2DDesc backBufferDesc = BackBuffer!.GetTextureDescription();

        Texture2DDesc multiSampleBackBufferDesc = MultiSampleBackBuffer!.GetTextureDescription();

        Texture2DDesc depthBufferDesc = DepthBuffer!.GetTextureDescription();

        DepthStencilViewDesc depthStencilViewDesc = default;

        DepthStencilView.GetDesc(ref depthStencilViewDesc);

        DepthStencilDesc depthStencilDesc = default;

        DepthStencilState.GetDesc(ref depthStencilDesc);

        BackBuffer.Dispose();
        RenderTargetView.Dispose();
        MultiSampleBackBuffer.Dispose();    
        MultiSampleRenderTargetView.Dispose();
        DepthBuffer.NativeTexture.Dispose();
        DepthStencilView.Dispose();
        DepthStencilState.Dispose();

        Context.Get().OMSetRenderTargets(0, null, null);

        Camera.UpdateProjectionMatrix(70f, 0.2f, 1000f);

        CreateViewport();

        SilkMarshal.ThrowHResult(SwapChain.ResizeBuffers(0, (uint)Window.Size.X, (uint)Window.Size.Y, backBufferDesc.Format, 0));

        CreateBackBuffer();

        multiSampleBackBufferDesc.Width = (uint)Window.Size.X;
        multiSampleBackBufferDesc.Height = (uint)Window.Size.Y;

        depthBufferDesc.Width = (uint)Window.Size.X;
        depthBufferDesc.Height = (uint)Window.Size.Y;

        DepthBuffer = new Texture2D(this, depthBufferDesc, TextureType.DepthBuffer);

        SilkMarshal.ThrowHResult(Device.CreateDepthStencilView(DepthBuffer.NativeTexture, depthStencilViewDesc, ref DepthStencilView));

        SilkMarshal.ThrowHResult(Device.CreateDepthStencilState(depthStencilDesc, ref DepthStencilState));
    }

    private void CreateDeviceAndSwapChain()
    {
        SilkMarshal.ThrowHResult
        (
            D3D11Provider.D3D11.Value.CreateDevice
            (
                pAdapter: default(ComPtr<IDXGIAdapter>),
                DriverType: D3DDriverType.Hardware,
                Software: 0,
#if DEBUG
                Flags: (uint)CreateDeviceFlag.Debug,
#else
                Flags: (uint)CreateDeviceFlag.None,
#endif
                pFeatureLevels: null,
                FeatureLevels: 0,
                SDKVersion: D3D11.SdkVersion,
                ppDevice: ref Device,
                pFeatureLevel: null,
                ppImmediateContext: ref Context
            )
        );

#if DEBUG
        Device.SetInfoQueueCallback((D3DDebugMessage message) => Logger.WriteLine(message.Description, message.LogSeverity));
#endif

        SwapChainDesc1 swapChainDesc = new SwapChainDesc1()
        {
            BufferCount = 2,
            Format = Format.FormatR8G8B8A8Unorm,
            BufferUsage = DXGI.UsageRenderTargetOutput,
            SwapEffect = SwapEffect.FlipDiscard,
            SampleDesc = new SampleDesc(1, 0)
        };

        SilkMarshal.ThrowHResult(DXGIProvider.DXGI.Value.CreateDXGIFactory(out ComPtr<IDXGIFactory2> dxgiFactory));

        SilkMarshal.ThrowHResult(dxgiFactory.CreateSwapChainForHwnd(Device, Window.Native!.Win32!.Value!.Hwnd, swapChainDesc, null, ref Unsafe.NullRef<IDXGIOutput>(), ref SwapChain));

        dxgiFactory.Dispose();
    }

    private void CreateBackBuffer()
    {
        BackBuffer = new Texture2D(this, TextureType.BackBuffer);

        SilkMarshal.ThrowHResult(SwapChain.GetBuffer(0, out BackBuffer.NativeTexture));

        RenderTargetViewDesc backBufferRenderTargetViewDesc = new RenderTargetViewDesc()
        {
            ViewDimension = RtvDimension.Texture2D,
        };

        SilkMarshal.ThrowHResult(Device.CreateRenderTargetView(BackBuffer.NativeTexture, backBufferRenderTargetViewDesc, ref RenderTargetView));


        Texture2DDesc backBufferDesc = BackBuffer.GetTextureDescription();

        BackBuffer.Format = backBufferDesc.Format;

        MultiSampleBackBuffer = new Texture2D(this, (int)backBufferDesc.Width, (int)backBufferDesc.Height, TextureType.None, new SampleDesc(8, 0), BindFlag.RenderTarget, BackBuffer.Format);

        RenderTargetViewDesc multiSampleRenderTargetViewDesc = new RenderTargetViewDesc()
        {
            ViewDimension = RtvDimension.Texture2Dms
        };

        SilkMarshal.ThrowHResult(Device.CreateRenderTargetView(MultiSampleBackBuffer.NativeTexture, multiSampleRenderTargetViewDesc, ref MultiSampleRenderTargetView));
    }

    private void CreateDepthBuffer()
    {
        DepthBuffer = new Texture2D(this, Window.Size.X, Window.Size.Y, TextureType.DepthBuffer, new SampleDesc(8, 0), BindFlag.DepthStencil | BindFlag.ShaderResource, Format.FormatR32Typeless, usage: Usage.Default);

        DepthStencilViewDesc depthStencilViewDesc = new DepthStencilViewDesc()
        {
            Format = Format.FormatD32Float,
            ViewDimension = DsvDimension.Texture2Dms,
        };

        SilkMarshal.ThrowHResult(Device.CreateDepthStencilView(DepthBuffer.NativeTexture, depthStencilViewDesc, ref DepthStencilView));

        DepthStencilDesc depthStencilDesc = new DepthStencilDesc()
        {
            DepthEnable = 1,
            DepthWriteMask = DepthWriteMask.All,
            DepthFunc = ComparisonFunc.LessEqual,
        };

        SilkMarshal.ThrowHResult(Device.CreateDepthStencilState(depthStencilDesc, ref DepthStencilState));
    }

    private void CreateViewport()
    {
        Viewport = new Viewport()
        {
            TopLeftX = 0,
            TopLeftY = 0,
            Width = Window.Size.X,
            Height = Window.Size.Y,
            MinDepth = 0.0f,
            MaxDepth = 1.0f,
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Device.Dispose();

        Context.Dispose();

        SwapChain.Dispose();
    }
}