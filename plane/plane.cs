using System.Numerics;
using ImGuiNET;
using plane.Diagnostics;
using plane.Graphics;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl;
using Renderer = plane.Graphics.Renderer;

namespace plane;

public abstract class Plane : IDisposable
{
    public IWindow Window { get; private set; }

    public Renderer? Renderer { get; private set; }

    public float DeltaTime { get; private set; }

    public double PreciseDeltaTime { get; private set; }

    private bool PendingResize = false;

    public Plane(string windowName)
    {
        SdlWindowing.Use();

        Window = Silk.NET.Windowing.Window.Create(new WindowOptions()
        {
            Size = new Vector2(1280, 720).ToGeneric().As<int>(),
            IsVisible = true,
            Title = windowName,
            WindowClass = "PlaneWindowClass",
            VSync = true,
            API = GraphicsAPI.None,
        });

        Window.Load += InternalLoad;

        Window.Render += InternalRender;

        Window.Resize += InternalResize;
    }

    public void Run()
    {
        Window.Run();
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

        if (PendingResize)
        {
            PendingResize = false;

            Resize();
        }

        Update();

        Renderer.Render();

        Render();
    }

    private void InternalResize(Vector2D<int> size)
    {
        PendingResize = true;
    }

    private void Resize()
    {
        Renderer?.Resize();
    }

    public void Dispose()
    {
        Renderer?.Dispose();

        Window.Dispose();

        GC.SuppressFinalize(this);
    }
}