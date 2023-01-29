using System.Diagnostics;
using plane.Diagnostics;
using plane.Graphics.Direct3D11;
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

    private Texture2D? DepthBuffer;

    private ComPtr<ID3D11DepthStencilView> DepthStencilView = default;

    private ComPtr<ID3D11DepthStencilState> DepthStencilState = default;

    private Viewport Viewport = default;

    private Rasterizer? Rasterizer;

    // TODO: Implement blend state and alpha blending

    private VertexShader? VertexShader;

    private PixelShader? PixelShader;

    public Renderer(IWindow window)
    {
        CreateDeviceAndSwapChain(window);

        BackBuffer = new Texture2D(this, TextureType.BackBuffer);

        SilkMarshal.ThrowHResult(SwapChain.Get().GetBuffer(0, ref Texture2D.Guid, (void**)BackBuffer.NativeTexture.GetAddressOf()));

        SilkMarshal.ThrowHResult(Device.Get().CreateRenderTargetView(BackBuffer.AsResource(), null, RenderTargetView.GetAddressOf()));

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

        PixelShader = ShaderCompiler.CompileFromFile<PixelShader>(Path.Combine(planeRootFolder, "Shaders/PixelShader.hlsl"), "PSMain", ShaderModel.PixelShader5_0);
        PixelShader.Create(this);
    }

    public void Render()
    {
        // Do Rendering Things
    }

    private void CreateDeviceAndSwapChain(IWindow window)
    {
        bool debug = Debugger.IsAttached;

        SilkMarshal.ThrowHResult
        (
            D3D11Provider.D3D11.Value.CreateDevice
            (
                pAdapter: default(ComPtr<IDXGIAdapter>),
                DriverType: D3DDriverType.Hardware,
                Software: 0,
                Flags: debug ? (uint)CreateDeviceFlag.Debug : (uint)CreateDeviceFlag.None,
                pFeatureLevels: null,
                FeatureLevels: 0,
                SDKVersion: D3D11.SdkVersion,
                ppDevice: Device.GetAddressOf(),
                pFeatureLevel: null,
                ppImmediateContext: Context.GetAddressOf()
            )
        );

        if (debug)
        {
            Device.SetInfoQueueCallback((Direct3D11Message message) => Logger.Log.WriteLine(message.Description ?? "", message.LogSeverity, DateTime.Now));
        }

        SwapChainDesc1 swapChainDesc = new SwapChainDesc1()
        {
            BufferCount = 2,
            Format = Format.FormatB8G8R8A8Unorm,
            BufferUsage = DXGI.UsageRenderTargetOutput,
            SwapEffect = SwapEffect.FlipDiscard,
            SampleDesc = new SampleDesc(1, 0),
        };

        using ComPtr<IDXGIFactory2> dxgiFactory = default;

        SilkMarshal.ThrowHResult(DXGIProvider.DXGI.Value.CreateDXGIFactory(ref SilkMarshal.GuidOf<IDXGIFactory2>(), (void**)dxgiFactory.GetAddressOf()));

        SilkMarshal.ThrowHResult(dxgiFactory.Get().CreateSwapChainForHwnd((IUnknown*)Device.Handle, window.Native!.Win32!.Value!.Hwnd, ref swapChainDesc, null, null, SwapChain.GetAddressOf()));
    }

    private void CreateDepthBuffer(IWindow window)
    {
        DepthBuffer = new Texture2D(this, window.Size.X, window.Size.Y, TextureType.DepthBuffer, new SampleDesc(1, 0), BindFlag.DepthStencil | BindFlag.ShaderResource, Format.FormatR32Typeless, usage: Usage.Default);

        DepthStencilViewDesc depthStencilViewDesc = new DepthStencilViewDesc()
        {
            Format = Format.FormatD32Float,
            ViewDimension = DsvDimension.Texture2D,
        };

        SilkMarshal.ThrowHResult(Device.Get().CreateDepthStencilView(DepthBuffer.AsResource(), ref depthStencilViewDesc, DepthStencilView.GetAddressOf()));

        Context.Get().OMSetRenderTargets(1, RenderTargetView.GetAddressOf(), DepthStencilView);

        DepthStencilDesc depthStencilDesc = new DepthStencilDesc()
        {
            DepthEnable = 1,
            DepthWriteMask = DepthWriteMask.All,
            DepthFunc = ComparisonFunc.LessEqual,
        };

        SilkMarshal.ThrowHResult(Device.Get().CreateDepthStencilState(ref depthStencilDesc, DepthStencilState.GetAddressOf()));
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

        Context.Get().RSSetViewports(1, ref Viewport);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Device.Dispose();
        Context.Dispose();
        SwapChain.Dispose();
    }
}