using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Graphics;
public class Sampler
{
    private readonly Renderer Renderer;

    internal ComPtr<ID3D11SamplerState> NativeSamplerState = default;

    public SamplerDesc Description;

    public Sampler(Renderer renderer, in SamplerDesc desc)
    {
        Renderer = renderer;

        Description = desc;

        Create(desc);
    }

    public void Bind(int slot, BindTo to)
    {
        switch (to)
        {
            case BindTo.VertexShader:
                Renderer.Context.VSSetSamplers((uint)slot, 1, ref NativeSamplerState);
                break;
            case BindTo.PixelShader:
                Renderer.Context.PSSetSamplers((uint)slot, 1, ref NativeSamplerState);
                break;
            case BindTo.GeometryShader:
                Renderer.Context.GSSetSamplers((uint)slot, 1, ref NativeSamplerState);
                break;
            case BindTo.ComputeShader:
                Renderer.Context.CSSetSamplers((uint)slot, 1, ref NativeSamplerState);
                break;
            case BindTo.HullShader:
                Renderer.Context.HSSetSamplers((uint)slot, 1, ref NativeSamplerState);
                break;
            case BindTo.DomainShader:
                Renderer.Context.DSSetSamplers((uint)slot, 1, ref NativeSamplerState);
                break;
            default:
                throw new ArgumentException($"Invalid binding target {to}.", nameof(to));
        }
    }

    public void Create(in SamplerDesc desc)
    {
        Description = desc;

        SilkMarshal.ThrowHResult(Renderer.Device.CreateSamplerState(desc, ref NativeSamplerState));
    }
}
