using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;

namespace plane.Graphics;

public class Blob : IDisposable
{
    internal ComPtr<ID3D10Blob> NativeBlob = default;

    public bool IsNull => Unsafe.IsNullRef(ref NativeBlob.Get());

    public nuint Size => NativeBlob.GetBufferSize();

    internal unsafe void* BufferPointer => NativeBlob.GetBufferPointer();

    public unsafe string? AsString()
    {
        return SilkMarshal.PtrToString((nint)NativeBlob.GetBufferPointer());
    }

    public void Dispose()
    {
        NativeBlob.Dispose();

        GC.SuppressFinalize(this);
    }
}
