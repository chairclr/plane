using Silk.NET.Direct3D.Compilers;

namespace plane.Graphics.Direct3D11;

public class D3DCompilerProvider
{
    public static Lazy<D3DCompiler> D3DCompiler { get; private set; }

    static D3DCompilerProvider()
    {
        D3DCompiler = new Lazy<D3DCompiler>(() => Silk.NET.Direct3D.Compilers.D3DCompiler.GetApi());
    }
}
