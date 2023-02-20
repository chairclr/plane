using System.Runtime.InteropServices;
using plane.Graphics.Buffers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace plane.Graphics;

public class Mesh : IDisposable
{
    private readonly Renderer Renderer;

    private readonly VertexBuffer<Vertex> VertexBuffer;

    private readonly IndexBuffer<int> IndexBuffer;

    public readonly List<Texture2D> Textures;

    public Mesh(Renderer renderer, List<Vertex> vertices, List<int> indicies, List<Texture2D> textures)
    {
        Renderer = renderer;

        VertexBuffer = new VertexBuffer<Vertex>(renderer, CollectionsMarshal.AsSpan(vertices));

        IndexBuffer = new IndexBuffer<int>(renderer, CollectionsMarshal.AsSpan(indicies));

        Textures = textures;
    }

    public unsafe void Render()
    {
        uint offset = 0;

        foreach (Texture2D tex in Textures)
        {
            if (tex.TextureType == TextureType.Diffuse)
            {
                Renderer.Context.PSSetShaderResources(0, 1, ref tex.ShaderResourceView);
                break;
            }
        }

        Renderer.Context.IASetVertexBuffers(0, 1, ref VertexBuffer.NativeBuffer, VertexBuffer.Stride, offset);
        Renderer.Context.IASetIndexBuffer(IndexBuffer.NativeBuffer, Format.FormatR32Uint, 0);
        Renderer.Context.DrawIndexed(IndexBuffer.Length, 0, 0);
    }

    public void Dispose()
    {
        VertexBuffer.Dispose();

        IndexBuffer.Dispose();

        foreach (Texture2D texture in Textures)
        {
            texture.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}