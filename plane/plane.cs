using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using plane.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace plane;

public class Plane : IDisposable
{
    public IWindow Window { get; private set; }

    public Renderer? Renderer { get; private set; }

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

        Window.Load += Load;

        Window.Render += Render;

        Window.Update += Update;
    }

    public void Run()
    {
        Window.Run();
    }

    private void Load()
    {
        Window.Center();

        Renderer = new Renderer(Window);
    }

    private void Render(double deltaTime)
    {
        Renderer ??= new Renderer(Window);

        Renderer.Render(); // Currently does nothing

        //Console.WriteLine($"Render | FPS: {(1 / deltaTime):F2}");
    }

    private void Update(double deltaTime)
    {
        //Console.WriteLine($"Update | UPS: {(1 / deltaTime):F2}");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        Window.Dispose();
    }
}
