using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;

namespace plane.Graphics;

public class Blob : IDisposable
{
    internal ComPtr<ID3D10Blob> NativeBlob = default;

    public bool IsNull => Unsafe.IsNullRef(ref NativeBlob.Get());

    public nuint Size => NativeBlob.GetBufferSize();

    internal unsafe void* BufferPointer => NativeBlob.GetBufferPointer();

    private bool Disposed;

    public unsafe string? AsString()
    {
        return SilkMarshal.PtrToString((nint)NativeBlob.GetBufferPointer());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {

            }

            NativeBlob.Dispose();
            Disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}