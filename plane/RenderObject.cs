using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using plane.Graphics;
using Silk.NET.Direct3D11;

namespace plane;

public abstract class RenderObject : IDisposable
{
    public Transform Transform;

    protected VertexShaderBuffer VertexShaderData;

    private readonly Buffer<VertexShaderBuffer> VertexShaderDataBuffer;

    protected Renderer Renderer;

    public RenderObject(Renderer renderer)
    {
        Transform = new Transform();
        Renderer = renderer;

        VertexShaderDataBuffer = new Buffer<VertexShaderBuffer>(Renderer, ref VertexShaderData, BindFlag.ConstantBuffer, usage: Usage.Dynamic, cpuAccessFlags: CpuAccessFlag.Write);
    }

    public virtual void Render(Camera camera)
    {
        VertexShaderData.World = Transform.WorldMatrix;
        VertexShaderData.ViewProjection = camera.ViewMatrix * camera.ProjectionMatrix;

        VertexShaderData.World = Matrix4x4.Transpose(VertexShaderData.World);
        VertexShaderData.ViewProjection = Matrix4x4.Transpose(VertexShaderData.ViewProjection);

        VertexShaderDataBuffer.WriteData(ref VertexShaderData);

        VertexShaderDataBuffer.Bind(0, BindTo.VertexShader);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        VertexShaderDataBuffer.Dispose();
    }
}
