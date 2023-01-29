using Silk.NET.Direct3D11;

namespace plane.Graphics.Direct3D11;

public class D3D11Provider
{
    public static Lazy<D3D11> D3D11 { get; private set; }

    static D3D11Provider()
    {
        D3D11 = new Lazy<D3D11>(() => Silk.NET.Direct3D11.D3D11.GetApi());
    }
}