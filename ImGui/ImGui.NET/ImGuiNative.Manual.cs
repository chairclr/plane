using System;
using System.Runtime.InteropServices;

namespace ImGuiNET;

public static unsafe partial class ImGuiNative
{
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuiPlatformIO_Set_Platform_GetWindowPos(ImGuiPlatformIO* platform_io, IntPtr funcPtr);
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuiPlatformIO_Set_Platform_GetWindowSize(ImGuiPlatformIO* platform_io, IntPtr funcPtr);
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ImGui_ImplDX11_Init(IntPtr device, IntPtr device_context);
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGui_ImplDX11_Shutdown();
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGui_ImplDX11_NewFrame();
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGui_ImplDX11_RenderDrawData(ImDrawDataPtr draw_data);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ImGui_ImplSDL2_InitForD3D(IntPtr window);
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGui_ImplSDL2_NewFrame();
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGui_ImplSDL2_ProcessEvent(IntPtr sdlEvent);
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ImGui_ImplSDL2_Shutdown();
}