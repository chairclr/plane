using Silk.NET.Core.Native;

namespace plane.Graphics.Shaders;

public abstract class Shader : IDisposable
{
    internal ComPtr<ID3D10Blob> ShaderData = default;

    internal abstract void Create(Renderer renderer);

    public abstract void Bind(Renderer renderer);

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing) 
        {
            ShaderData.Dispose();
        }
    }
}