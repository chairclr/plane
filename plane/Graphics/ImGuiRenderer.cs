using System.Runtime.InteropServices;
using ImGuiNET;
using plane.Diagnostics;
using Silk.NET.SDL;

namespace plane.Graphics;

public class ImGuiRenderer : IDisposable
{
    private readonly Renderer Renderer;

    private readonly object EventLock = new object();

    private readonly Queue<Event> EventQueue = new Queue<Event>();

    public unsafe ImGuiRenderer(Renderer renderer)
    {
        Renderer = renderer;

        Logger.WriteLine("Create ImGuiRenderer: ImGui Context");
        ImGui.CreateContext();

        Logger.WriteLine("Create ImGuiRenderer: Init");
        ImGuiNative.ImGui_ImplDX11_Init((nint)Renderer.Device.Handle, (nint)Renderer.Context.Handle);
        ImGuiNative.ImGui_ImplSDL2_InitForD3D(Renderer.Window.Native!.Sdl!.Value);

        SdlProvider.SDL.Value.AddEventWatch(new PfnEventFilter((x, y) =>
        {
            Event e = *y;
            lock (EventLock)
            {
                EventQueue.Enqueue(e);
            }
            return 0;
        }), null);

        ImGui.GetIO().Fonts.AddFontFromFileTTF("C:\\Windows\\Fonts\\Tahoma.ttf", 18f);
    }

    private string CoolString = "";

    public void Render()
    {
        lock (EventLock)
        {
            while (EventQueue.Count > 0)
            {
                Event e = EventQueue.Dequeue();

                unsafe
                {
                    ImGuiNative.ImGui_ImplSDL2_ProcessEvent((nint)(&e));
                }
            }
        }

        ImGuiNative.ImGui_ImplDX11_NewFrame();
        ImGuiNative.ImGui_ImplSDL2_NewFrame();
        ImGui.NewFrame();

        ImGui.Begin("Test");

        ImGui.Text("Test 2");

        ImGui.InputText("Test 3", ref CoolString, 2000);

        ImGui.End();

        ImGui.Render();
        ImGuiNative.ImGui_ImplDX11_RenderDrawData(ImGui.GetDrawData());
    }


    static ImGuiRenderer()
    {
        Logger.WriteLine("Create ImGuiRenderer: Dll Resolver");

        NativeLibrary.SetDllImportResolver(typeof(ImGui).Assembly, (libraryName, assembly, z) =>
        {
            string nativePath = Path.Combine(Path.GetDirectoryName(typeof(ImGuiRenderer).Assembly.Location)!, $"Native");

            nint libraryHandle = 0;

            if (libraryName == "cimgui")
            {
                if (!NativeLibrary.TryLoad(Path.Combine(nativePath, $"{libraryName}x{(Environment.Is64BitProcess ? "64" : "86")}"), out libraryHandle))
                {
                    throw new DllNotFoundException($"Could not find '{libraryName}'");
                }
            }

            return libraryHandle;
        });
    }

    public void Dispose()
    {
        ImGuiNative.ImGui_ImplDX11_Shutdown();
        ImGuiNative.ImGui_ImplSDL2_Shutdown();

        GC.SuppressFinalize(this);
    }
}