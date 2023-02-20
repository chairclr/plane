namespace plane.Graphics.Shaders;

public abstract class Shader : IDisposable
{
    protected readonly Renderer Renderer;

    internal Blob ShaderData = new Blob();

    public Shader(Renderer renderer)
    {
        Renderer = renderer;
    }

    internal abstract void Create();

    public abstract void Bind();

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            ShaderData?.Dispose();
        }
    }
}