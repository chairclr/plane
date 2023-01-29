using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Graphics;

public class Rasterizer
{
    private ComPtr<ID3D11Device> Device = default;

    internal ComPtr<ID3D11RasterizerState> RasterizerState = default;

    public RasterizerDesc Description;

    public unsafe Rasterizer(Renderer renderer, RasterizerDesc desc)
    {
        Device = renderer.Device;

        Description = desc;

        Recreate(Description);
    }

    public unsafe void Recreate(RasterizerDesc desc)
    {
        Description = desc;

        SilkMarshal.ThrowHResult(Device.Get().CreateRasterizerState(ref Description, RasterizerState.GetAddressOf()));
    }
}
