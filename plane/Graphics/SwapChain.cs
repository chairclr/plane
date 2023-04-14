using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using plane.Graphics.Providers;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using Silk.NET.Windowing;

namespace plane.Graphics;

public class SwapChain : DXGIProvider, IDisposable
{
    internal ComPtr<IDXGISwapChain1> NativeSwapChain;

    private bool Disposed;

    public unsafe SwapChain(CommandQueue commandQueue, IWindow window, uint bufferCount, Format format, SwapEffect swapEffect)
    {
        SilkMarshal.ThrowHResult(DXGI.CreateDXGIFactory(out ComPtr<IDXGIFactory4> dxgiFactory));

        SwapChainDesc1 swapChainDesc = new SwapChainDesc1()
        {
            BufferCount = bufferCount,
            Format = format,
            SwapEffect = swapEffect,
            BufferUsage = DXGI.UsageRenderTargetOutput,
            SampleDesc = new SampleDesc(1, 0),
        };

        SilkMarshal.ThrowHResult(dxgiFactory.CreateSwapChainForHwnd(commandQueue.NativeCommandQueue, window.Native!.DXHandle!.Value, swapChainDesc, null, ref Unsafe.NullRef<IDXGIOutput>(), ref NativeSwapChain));

        dxgiFactory.Dispose();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {

            }

            NativeSwapChain.Dispose();
            Disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}