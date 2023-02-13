using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Graphics;

public class Rasterizer : IDisposable
{
    private readonly Renderer Renderer;

    internal ComPtr<ID3D11RasterizerState> RasterizerState = default;

    public RasterizerDesc Description;

    public unsafe Rasterizer(Renderer renderer, RasterizerDesc desc)
    {
        Renderer = renderer;

        Description = desc;

        Create(Description);
    }

    public void Bind()
    {
        Renderer.Context.RSSetState(RasterizerState);
    }

    public unsafe void Create(RasterizerDesc desc)
    {
        Description = desc;

        SilkMarshal.ThrowHResult(Renderer.Device.CreateRasterizerState(Description, ref RasterizerState));
    }

    public void Dispose()
    {
        RasterizerState.Dispose();

        GC.SuppressFinalize(this);
    }
}