using System.Runtime.CompilerServices;
using plane.Diagnostics;
using plane.Graphics.Providers;
using plane.Graphics.Shaders;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using Silk.NET.Windowing;

namespace plane.Graphics;

public unsafe class Renderer : D3D12Provider, IDisposable
{
    public IWindow Window;

    public ComPtr<ID3D12Device> Device = default;

    public ComPtr<ID3D12GraphicsCommandList> CommandList = default;

    public ComPtr<ID3D12CommandAllocator> CommandAllocator = default;

    public ComPtr<ID3D12PipelineState> PipelineState = default;

    public ComPtr<ID3D12DescriptorHeap> RenderTargetViewHeap = default;

    private CommandQueue CommandQueue;

    private SwapChain SwapChain;

    public Camera Camera;

    private uint FrameIndex = 0;

    private uint RtvDescriptorSize;

    private bool Disposed;

    public Renderer(IWindow window)
    {
        Window = window;

        CreateDevice();

        CommandQueue = new CommandQueue(Device);

        GraphicsPipelineStateDesc graphicsPipelineStateDesc = new GraphicsPipelineStateDesc()
        {
            SampleDesc = new SampleDesc(1, 0),
        };

        SilkMarshal.ThrowHResult(Device.CreateGraphicsPipelineState(graphicsPipelineStateDesc, out PipelineState));

        SilkMarshal.ThrowHResult(Device.CreateCommandList(0, CommandListType.Direct, CommandAllocator, PipelineState, out CommandList));

        SwapChain = new SwapChain(CommandQueue, Window, 2, Format.FormatR8G8B8A8Unorm, SwapEffect.FlipDiscard);

        DescriptorHeapDesc descriptorHeapDesc = new DescriptorHeapDesc()
        {
            Type = DescriptorHeapType.Rtv,
            NumDescriptors = 2
        };

        SilkMarshal.ThrowHResult(Device.CreateDescriptorHeap(descriptorHeapDesc, out RenderTargetViewHeap));

        RtvDescriptorSize = Device.GetDescriptorHandleIncrementSize(DescriptorHeapType.Rtv);

        Camera = new Camera(Window, 90f, 0.1f, 1000f);
    }

    private void CreateDevice()
    {
#if DEBUG
        ComPtr<ID3D12Debug> debugController;

        SilkMarshal.ThrowHResult(D3D12.GetDebugInterface(out debugController));

        debugController.EnableDebugLayer();

        debugController.Dispose();
#endif

        SilkMarshal.ThrowHResult(D3D12.CreateDevice(ref Unsafe.NullRef<IUnknown>(), D3DFeatureLevel.Level110, out Device));

#if DEBUG
        Device.SetInfoQueueCallback(message => Logger.WriteLine(message.Description, message.LogSeverity));
#endif

        Device.SetName("Cool Device");
    }

    internal void Render()
    {
        CpuDescriptorHandle rtvHandle = new CpuDescriptorHandle(RenderTargetViewHeap.GetCPUDescriptorHandleForHeapStart().Ptr + FrameIndex * RtvDescriptorSize);

        float[] clearColor = new float[] { 0f, 1f, 0f, 1f };
        
        CommandList.ClearRenderTargetView(rtvHandle, clearColor, 0, (Silk.NET.Maths.Box2D<int>*)null);

        CommandQueue.NativeCommandQueue.ExecuteCommandLists(1, ref CommandList);

        SwapChain.NativeSwapChain.Present(1, 0);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                
            }

            Device.Dispose();
            SwapChain.Dispose();

            Disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}