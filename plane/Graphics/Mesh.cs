using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.SDL;

namespace plane.Graphics;

public class Mesh : IDisposable
{
    private readonly Renderer Renderer;

    private Buffer<Vertex> VertexBuffer;

    private Buffer<int> IndexBuffer;

    public readonly List<Texture2D> Textures;

    public Mesh(Renderer renderer, List<Vertex> vertices, List<int> indicies, List<Texture2D> textures)
    {
        Renderer = renderer;

        VertexBuffer = new Buffer<Vertex>(renderer, CollectionsMarshal.AsSpan(vertices), BindFlag.VertexBuffer);

        IndexBuffer = new Buffer<int>(renderer, CollectionsMarshal.AsSpan(indicies), BindFlag.IndexBuffer);

        Textures = textures;
    }

    public unsafe void Render()
    {
        uint offset = 0;

        foreach (Texture2D tex in Textures)
        {
            if (tex.TextureType == TextureType.Diffuse)
            {
                Renderer.Context.Get().PSSetShaderResources(0, 1, tex.ShaderResourceView.GetAddressOf());
                break;
            }
        }

        Renderer.Context.Get().IASetVertexBuffers(0, 1, VertexBuffer.DataBuffer.GetAddressOf(), ref VertexBuffer.Stride, ref offset);
        Renderer.Context.Get().IASetIndexBuffer(IndexBuffer.DataBuffer, Format.FormatR32Uint, 0);
        Renderer.Context.Get().DrawIndexed(IndexBuffer.Length, 0, 0);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        VertexBuffer.Dispose();

        IndexBuffer.Dispose();

        foreach (Texture2D texture in Textures)
        {
            texture.Dispose();
        }
    }
}
