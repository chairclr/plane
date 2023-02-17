using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using plane.Diagnostics;
using plane.Graphics.Buffers;
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

    private Texture2D? MultiSampleBackBuffer;

    private ComPtr<ID3D11RenderTargetView> MultiSampleRenderTargetView = default;

    private Texture2D? PostProcessBackBuffer1;

    private Texture2D? PostProcessBackBuffer2;

    private Texture2D? MultiSampleDepthBuffer;

    private ComPtr<ID3D11DepthStencilView> MultiSampleDepthStencilView = default;

    private ComPtr<ID3D11DepthStencilState> MultiSampleDepthStencilState = default;

    private Texture2D? ImGuiBackBuffer;

    private ComPtr<ID3D11RenderTargetView> ImGuiRenderTargetView = default;

    private Viewport Viewport = default;

    private readonly Rasterizer? Rasterizer;

    private readonly ComPtr<ID3D11BlendState> BlendState = default;

    private readonly VertexShader? VertexShader;

    private readonly PixelShader? PixelShader;

    private PixelShaderBuffer PixelShaderBufferData;

    private readonly Buffer<PixelShaderBuffer> PixelShaderBuffer;

    private readonly ComputeShader? PostProcessComputeShaderX;

    private readonly ComputeShader? PostProcessComputeShaderY;

    private ComputeShaderBuffer ComputeShaderBufferData = new ComputeShaderBuffer();

    private readonly Buffer<ComputeShaderBuffer> ComputeShaderBuffer;

    private readonly Sampler PixelShaderSampler;

    private readonly ImGuiRenderer ImGuiRenderer;

    public Camera Camera;

    public readonly List<RenderObject> RenderObjects = new List<RenderObject>();

    public Renderer(IWindow window)
    {
        Window = window;

        CreateDeviceAndSwapChain();

        CreateBackBuffer();

        CreateDepthBuffer();

        CreateViewport();

        Rasterizer = new Rasterizer(this, new RasterizerDesc()
        {
            MultisampleEnable = true,
            AntialiasedLineEnable = true,
            FillMode = FillMode.Solid,
            CullMode = CullMode.Back,
        });

        BlendDesc blendDesc = new BlendDesc();

        blendDesc.RenderTarget[0] = new RenderTargetBlendDesc()
        {
            BlendEnable = true,
            SrcBlend = Blend.SrcAlpha,
            DestBlend = Blend.InvSrcAlpha,
            BlendOp = BlendOp.Add,
            SrcBlendAlpha = Blend.One,
            DestBlendAlpha = Blend.Zero,
            BlendOpAlpha = BlendOp.Add,
            RenderTargetWriteMask = (byte)ColorWriteEnable.All
        };

        SilkMarshal.ThrowHResult(Device.CreateBlendState(blendDesc, ref BlendState));

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

        PostProcessComputeShaderX = ShaderCompiler.CompileFromFile<ComputeShader>(Path.Combine(planeRootFolder, "Shaders/PostProcessComputeShader.hlsl"), "CSMainX", ShaderModel.ComputeShader5_0);
        PostProcessComputeShaderX.Create(this);
        PostProcessComputeShaderY = ShaderCompiler.CompileFromFile<ComputeShader>(Path.Combine(planeRootFolder, "Shaders/PostProcessComputeShader.hlsl"), "CSMainY", ShaderModel.ComputeShader5_0);
        PostProcessComputeShaderY.Create(this);

        ComputeShaderBuffer = new Buffer<ComputeShaderBuffer>(this, ref ComputeShaderBufferData, BindFlag.ConstantBuffer, Usage.Dynamic, CpuAccessFlag.Write);

        ImGuiRenderer = new ImGuiRenderer(this);

        Camera = new Camera(window, 70f, 0.2f, 1000f);
    }

    public void Render()
    {
        RenderScene();

        PostProcessScene();

        RenderImGui();

        Present();
    }

    private void RenderScene()
    {
        Context.OMSetRenderTargets(1, ref MultiSampleRenderTargetView, MultiSampleDepthStencilView);
        Context.OMSetDepthStencilState(MultiSampleDepthStencilState, 0);
        Context.OMSetBlendState(BlendState, null, 0xFFFFFFFF);

        float[] clearColor = new float[] { 0.55f, 0.7f, 0.75f, 1f };
        Context.ClearRenderTargetView(MultiSampleRenderTargetView, ref clearColor[0]);
        Context.ClearDepthStencilView(MultiSampleDepthStencilView, (uint)(ClearFlag.Depth | ClearFlag.Stencil), 1.0f, 0);

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
    }

    

    private void PostProcessScene()
    {
        Context.ResolveSubresource(PostProcessBackBuffer1!.NativeTexture, 0, MultiSampleBackBuffer!.NativeTexture, 0, PostProcessBackBuffer1!.Format);

        ComputeShaderBuffer.WriteData(ref ComputeShaderBufferData);
        ComputeShaderBuffer.Bind(0, BindTo.ComputeShader);

        Context.CSSetUnorderedAccessViews(1, 1, ref PostProcessBackBuffer1!.UnorderedAccessView, null);
        Context.CSSetUnorderedAccessViews(2, 1, ref PostProcessBackBuffer2!.UnorderedAccessView, null);

        uint numThreadsX = (uint)MathF.Ceiling((float)Window.Size.X / 32f);
        uint numThreadsY = (uint)MathF.Ceiling((float)Window.Size.Y / 32f);

        PostProcessComputeShaderX!.Bind(this);
        Context.Dispatch(numThreadsX, numThreadsY, 1);

        Context.CopyResource(PostProcessBackBuffer1!.NativeTexture, PostProcessBackBuffer2!.NativeTexture);

        PostProcessComputeShaderY!.Bind(this);
        Context.Dispatch(numThreadsX, numThreadsY, 1);
    }

    private void RenderImGui()
    {
        Context.CopyResource(ImGuiBackBuffer!.NativeTexture, PostProcessBackBuffer2!.NativeTexture);

        Context.OMSetRenderTargets(1, ref ImGuiRenderTargetView, ref Unsafe.NullRef<ID3D11DepthStencilView>());

        ImGuiRenderer.Begin();

        

        ImGuiRenderer.End();
    }

    private void Present()
    {
        Context.CopyResource(BackBuffer!.NativeTexture, ImGuiBackBuffer!.NativeTexture);

        SwapChain.Present(1, 0);
    }

    internal void Resize()
    {
        DepthStencilViewDesc depthStencilViewDesc = default;

        MultiSampleDepthStencilView.GetDesc(ref depthStencilViewDesc);

        DepthStencilDesc depthStencilDesc = default;

        MultiSampleDepthStencilState.GetDesc(ref depthStencilDesc);

        BackBuffer!.Dispose();
        MultiSampleBackBuffer!.Dispose();
        MultiSampleRenderTargetView!.Dispose();
        PostProcessBackBuffer1!.Dispose();
        PostProcessBackBuffer2!.Dispose();
        ImGuiBackBuffer!.Dispose();
        ImGuiRenderTargetView!.Dispose();
        MultiSampleDepthBuffer!.NativeTexture.Dispose();
        MultiSampleDepthStencilView!.Dispose();
        MultiSampleDepthStencilState!.Dispose();

        Context.Get().OMSetRenderTargets(0, null, null);

        Camera.UpdateProjectionMatrix(70f, 0.2f, 1000f);

        CreateViewport();

        SilkMarshal.ThrowHResult(SwapChain.ResizeBuffers(0, (uint)Window.Size.X, (uint)Window.Size.Y, Format.FormatUnknown, 0));

        CreateBackBuffer();

        CreateDepthBuffer();
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
        Device.SetObjectName("RendererDevice");
        Context.SetObjectName("RendererContext");

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
        // BackBuffer
        {
            BackBuffer = new Texture2D(this, TextureType.BackBuffer);

            SilkMarshal.ThrowHResult(SwapChain.GetBuffer(0, out BackBuffer.NativeTexture));
        }

        Texture2DDesc backBufferDesc = BackBuffer.GetTextureDescription();

        // MultiSampleBackBuffer
        {

            BackBuffer.Format = backBufferDesc.Format;

            MultiSampleBackBuffer = new Texture2D(this, (int)backBufferDesc.Width, (int)backBufferDesc.Height, TextureType.None, new SampleDesc(8, 0), BindFlag.RenderTarget, BackBuffer.Format);

            RenderTargetViewDesc multiSampleRenderTargetViewDesc = new RenderTargetViewDesc()
            {
                ViewDimension = RtvDimension.Texture2Dms
            };

            SilkMarshal.ThrowHResult(Device.CreateRenderTargetView(MultiSampleBackBuffer.NativeTexture, multiSampleRenderTargetViewDesc, ref MultiSampleRenderTargetView));
        }


        PostProcessBackBuffer1 = new Texture2D(this, (int)backBufferDesc.Width, (int)backBufferDesc.Height, TextureType.None, new SampleDesc(1, 0), BindFlag.UnorderedAccess, BackBuffer.Format);

        PostProcessBackBuffer2 = new Texture2D(this, (int)backBufferDesc.Width, (int)backBufferDesc.Height, TextureType.None, new SampleDesc(1, 0), BindFlag.UnorderedAccess, BackBuffer.Format);

        // ImGuiBackBuffer
        {
            ImGuiBackBuffer = new Texture2D(this, (int)backBufferDesc.Width, (int)backBufferDesc.Height, TextureType.None, new SampleDesc(1, 0), BindFlag.RenderTarget, BackBuffer.Format);

            RenderTargetViewDesc imGuiRenderTargetViewDesc = new RenderTargetViewDesc()
            {
                ViewDimension = RtvDimension.Texture2D
            };

            SilkMarshal.ThrowHResult(Device.CreateRenderTargetView(ImGuiBackBuffer.NativeTexture, imGuiRenderTargetViewDesc, ref ImGuiRenderTargetView));
        }
    }

    private void CreateDepthBuffer()
    {
        MultiSampleDepthBuffer = new Texture2D(this, Window.Size.X, Window.Size.Y, TextureType.DepthBuffer, new SampleDesc(8, 0), BindFlag.DepthStencil, Format.FormatR32Typeless, usage: Usage.Default);

        DepthStencilViewDesc depthStencilViewDesc = new DepthStencilViewDesc()
        {
            Format = Format.FormatD32Float,
            ViewDimension = DsvDimension.Texture2Dms,
        };

        SilkMarshal.ThrowHResult(Device.CreateDepthStencilView(MultiSampleDepthBuffer.NativeTexture, depthStencilViewDesc, ref MultiSampleDepthStencilView));

        DepthStencilDesc depthStencilDesc = new DepthStencilDesc()
        {
            DepthEnable = 1,
            DepthWriteMask = DepthWriteMask.All,
            DepthFunc = ComparisonFunc.LessEqual,
        };

        SilkMarshal.ThrowHResult(Device.CreateDepthStencilState(depthStencilDesc, ref MultiSampleDepthStencilState));
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

        ImGuiRenderer.Dispose();

        BackBuffer?.Dispose();

        MultiSampleBackBuffer?.Dispose();

        MultiSampleRenderTargetView.Dispose();

        MultiSampleDepthBuffer?.Dispose();

        MultiSampleDepthStencilState.Dispose();

        MultiSampleDepthStencilView.Dispose();

        Rasterizer?.Dispose();

        VertexShader?.Dispose();

        PixelShader?.Dispose();

        PixelShaderBuffer.Dispose();

        PixelShaderSampler.Dispose();

        foreach (RenderObject renderObject in RenderObjects)
        {
            renderObject.Dispose();
        }

        SwapChain.Dispose();

        Context.Dispose();

        Device.Dispose();
    }
}