using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using plane.Diagnostics;
using plane.Graphics.Providers;
using plane.Graphics.Shaders;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Windowing;

namespace plane.Graphics;

public unsafe class Renderer : IDisposable
{
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

    private ComPtr<ID3D11SamplerState> PixelShaderSampler = default;

    public Camera Camera;

    public readonly List<RenderObject> RenderObjects = new List<RenderObject>();

    public Renderer(IWindow window)
    {
        CreateDeviceAndSwapChain(window);

        CreateBackBuffer();
        
        CreateDepthBuffer(window);

        CreateViewport(window);

        RasterizerDesc rasterizerDesc = new RasterizerDesc()
        {
            MultisampleEnable = 1,
            AntialiasedLineEnable = 1,
            FillMode = FillMode.Solid,
            CullMode = CullMode.Back,
        };

        Rasterizer = new Rasterizer(this, rasterizerDesc);

        string planeRootFolder = Path.GetDirectoryName(typeof(Plane).Assembly.Location)!;

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

        SamplerDesc sampDesc = new SamplerDesc()
        {
            Filter = Filter.Anisotropic,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            ComparisonFunc = ComparisonFunc.Never,
            MinLOD = 0,
            MaxLOD = float.MaxValue,
        };

        SilkMarshal.ThrowHResult(Device.CreateSamplerState(sampDesc, ref PixelShaderSampler));

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

        Context.RSSetState(Rasterizer!.RasterizerState);
        Context.RSSetViewports(1, Viewport);

        Context.PSSetSamplers(0, 1, ref PixelShaderSampler);

        Context.VSSetShader(VertexShader!.NativeShader, null, 0);
        Context.PSSetShader(PixelShader!.NativeShader, null, 0);
        Context.GSSetShader(ref Unsafe.NullRef<ID3D11GeometryShader>(), null, 0);

        for (int i = 0; i < RenderObjects.Count; i++)
        {
            RenderObjects[i].Render(Camera);
        }

        Context.ResolveSubresource(BackBuffer!.NativeTexture, 0, MultiSampleBackBuffer!.NativeTexture, 0, BackBuffer!.Format);

        SwapChain.Present(1, 0);
    }

    private void CreateDeviceAndSwapChain(IWindow window)
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
        Device.SetInfoQueueCallback((Direct3D11Message message) => Logger.Log.WriteLine(message.Description, message.LogSeverity, DateTime.Now));
#endif

        SwapChainDesc1 swapChainDesc = new SwapChainDesc1()
        {
            BufferCount = 2,
            Format = Format.FormatR8G8B8A8Unorm,
            BufferUsage = DXGI.UsageRenderTargetOutput,
            SwapEffect = SwapEffect.FlipDiscard,
            SampleDesc = new SampleDesc(1, 0),
        };


        SilkMarshal.ThrowHResult(DXGIProvider.DXGI.Value.CreateDXGIFactory(out ComPtr<IDXGIFactory2> dxgiFactory));

        SilkMarshal.ThrowHResult(dxgiFactory.CreateSwapChainForHwnd(Device, window.Native!.Win32!.Value!.Hwnd, swapChainDesc, null, ref Unsafe.NullRef<IDXGIOutput>(), ref SwapChain));

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

    private void CreateDepthBuffer(IWindow window)
    {
        DepthBuffer = new Texture2D(this, window.Size.X, window.Size.Y, TextureType.DepthBuffer, new SampleDesc(8, 0), BindFlag.DepthStencil | BindFlag.ShaderResource, Format.FormatR32Typeless, usage: Usage.Default);

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

    private void CreateViewport(IWindow window)
    {
        Viewport viewport = new Viewport()
        {
            TopLeftX = 0,
            TopLeftY = 0,
            Width = window.Size.X,
            Height = window.Size.Y,
            MinDepth = 0.0f,
            MaxDepth = 1.0f,
        };

        Viewport = viewport;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Device.Dispose();

        Context.Dispose();

        SwapChain.Dispose();
    }
}