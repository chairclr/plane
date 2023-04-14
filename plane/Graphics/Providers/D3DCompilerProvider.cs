using Silk.NET.Direct3D.Compilers;

namespace plane.Graphics.Providers;

public class D3DCompilerProvider
{
    public static D3DCompiler D3DCompiler { get; private set; }

    static D3DCompilerProvider()
    {
        D3DCompiler = D3DCompiler.GetApi();
    }
}