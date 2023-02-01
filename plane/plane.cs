using System.Numerics;
using plane.Graphics;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace plane;

public abstract class Plane : IDisposable
{
    public IWindow Window { get; private set; }

    public Renderer? Renderer { get; private set; }

    public float DeltaTime { get; private set; }

    public double PreciseDeltaTime { get; private set; }

    public Plane(string windowName)
    {
        Window = Silk.NET.Windowing.Window.Create(new WindowOptions()
        {
            Size = new Vector2(1280, 720).ToGeneric().As<int>(),
            IsVisible = true,
            Title = windowName,
            WindowClass = "PlaneWindowClass",
            VSync = true,
            API = GraphicsAPI.None
        });

        Window.Load += InternalLoad;

        Window.Render += InternalRender;
    }

    public void Run()
    {
        Window.Run();
    }

    private void InternalLoad()
    {
        Window.Center();

        Renderer = new Renderer(Window);

        Load();
    }

    private void InternalRender(double deltaTime)
    {
        PreciseDeltaTime = deltaTime;

        DeltaTime = (float)deltaTime;


        Renderer ??= new Renderer(Window);

        Update();

        Renderer.Render();

        Render();
    }

    public virtual void Load()
    {

    }

    public virtual void Render()
    {

    }

    public virtual void Update()
    {

    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Window.Dispose();
    }
}