using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D12;

namespace plane.Graphics.Providers;

public class D3D12Provider
{
    public static D3D12 D3D12 { get; private set; }

    static D3D12Provider()
    {
        D3D12 = D3D12.GetApi();
    }
}