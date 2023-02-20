using Silk.NET.Assimp;

namespace plane.Graphics.Providers;

public class AssimpProvider
{
    public static Lazy<Assimp> Assimp { get; private set; }

    static AssimpProvider()
    {
        Assimp = new Lazy<Assimp>(() => Silk.NET.Assimp.Assimp.GetApi());
    }
}