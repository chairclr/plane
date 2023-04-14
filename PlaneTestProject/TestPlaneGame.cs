using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using plane;
using plane.Diagnostics;
using Plane = plane.Plane;

namespace PlaneTestProject;

public class TestPlaneGame : Plane
{
    public TestPlaneGame(string windowName)
        : base(windowName)
    {

    }

    public override void Load()
    {
        Renderer!.Camera.Translation = new Vector3(0f, 0f, -4f);
    }

    public override void Render()
    {

    }

    public override void RenderImGui()
    {
        //ImGui.Begin("Test");
        //
        //ImGui.End();
    }

    public override void Update()
    {

    }
}