using System.Numerics;
using plane.Graphics;
using plane.Graphics.Buffers;

namespace plane;

public abstract class RenderObject : IDisposable
{
    protected readonly Renderer Renderer;

    public Transform Transform;

    protected readonly ConstantBuffer<VertexShaderBuffer> VertexShaderDataBuffer;

    public RenderObject(Renderer renderer)
    {
        Transform = new Transform();
        Renderer = renderer;

        VertexShaderDataBuffer = new ConstantBuffer<VertexShaderBuffer>(Renderer);
    }

    public virtual void Render(Camera camera)
    {
        VertexShaderDataBuffer.Data.World = Transform.WorldMatrix;
        VertexShaderDataBuffer.Data.ViewProjection = camera.ViewMatrix * camera.ProjectionMatrix;

        VertexShaderDataBuffer.Data.World = Matrix4x4.Transpose(VertexShaderDataBuffer.Data.World);
        VertexShaderDataBuffer.Data.ViewProjection = Matrix4x4.Transpose(VertexShaderDataBuffer.Data.ViewProjection);

        VertexShaderDataBuffer.WriteData();

        VertexShaderDataBuffer.Bind(0, BindTo.VertexShader);
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            VertexShaderDataBuffer.Dispose();
        }
    }
}