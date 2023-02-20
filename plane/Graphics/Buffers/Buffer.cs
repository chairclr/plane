using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Graphics.Buffers;

public abstract unsafe class Buffer : IDisposable
{
    protected readonly Renderer Renderer;

    public ComPtr<ID3D11Buffer> NativeBuffer = default;

    protected Buffer(Renderer renderer)
    {
        Renderer = renderer;
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            NativeBuffer.Dispose();
        }
    }
}