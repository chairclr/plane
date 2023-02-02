using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Assimp;
using Silk.NET.Direct3D.Compilers;

namespace plane.Graphics.Providers;

public class AssimpProvider
{
    public static Lazy<Assimp> Assimp { get; private set; }

    static AssimpProvider()
    {
        Assimp = new Lazy<Assimp>(() => Silk.NET.Assimp.Assimp.GetApi());
    }
}
