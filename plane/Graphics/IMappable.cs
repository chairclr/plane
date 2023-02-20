using Silk.NET.Direct3D11;

namespace plane.Graphics;

public interface IMappable
{
    public MappedSubresource MapRead(int subresource = 0);

    public MappedSubresource MapWrite(int subresource = 0);

    public ReadOnlySpan<T> MapReadSpan<T>(int subresource = 0);

    public Span<T> MapWriteSpan<T>(int subresource = 0);

    public void Unmap();
}