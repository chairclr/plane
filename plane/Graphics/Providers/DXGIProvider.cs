using Silk.NET.DXGI;

namespace plane.Graphics.Providers;

public class DXGIProvider
{
    public static DXGI DXGI { get; private set; }

    static DXGIProvider()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        DXGI = DXGI.GetApi();
#pragma warning restore CS0618
    }
}