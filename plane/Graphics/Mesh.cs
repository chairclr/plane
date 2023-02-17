using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using plane.Graphics.Buffers;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.SDL;

namespace plane.Graphics;

public class Mesh : IDisposable
{
    private readonly Renderer Renderer;

    private readonly Buffer<Vertex> VertexBuffer;

    private readonly Buffer<int> IndexBuffer;

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