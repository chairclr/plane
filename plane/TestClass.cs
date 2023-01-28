using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl;

namespace plane;

public class TestClass
{
    private static IWindow? TestWindow;

    public static void Run()
    {
        SdlWindowing.Use();

        WindowOptions options = WindowOptions.Default;

        options.Size = new Vector2(1280, 720).ToGeneric().As<int>();
        options.Title = "Test Window";

        TestWindow = Window.Create(options);

        TestWindow.Load += WindowLoad;

        TestWindow.Run();
    }

    private static void WindowLoad()
    {
        TestWindow!.Center();
    }
}
