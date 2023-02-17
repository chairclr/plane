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
    public List<RenderModel> CubeModels = new List<RenderModel>();

    public TestPlaneGame(string windowName)
        : base(windowName)
    {

    }

    public override void Load()
    {
        for (int i = 0; i < 5; i++)
        {
            CubeModels.Add(new RenderModel(Renderer!, Path.Combine(Path.GetDirectoryName(typeof(TestPlaneGame).Assembly.Location)!, "Models/cube.obj")));

            Vector3 pos = new Vector3(4f - (i * 2f), 0, 0);

            CubeModels.Last().Transform.Translation = pos;
        }

        Renderer!.RenderObjects.AddRange(CubeModels);

        Renderer!.Camera.Translation = new Vector3(0f, 0f, -4f);
    }

    private Vector3 CubeRotation = Vector3.Zero;

    public override void Render()
    {
        if (!ImGui.IsKeyDown(ImGuiKey.Space))
        {
            CubeRotation.X -= DeltaTime;
            CubeRotation.Y += DeltaTime;


            for (int i = 0; i < 5; i++)
            {
                CubeModels[i].Transform.EulerRotation = CubeRotation + new Vector3(i, -i, 0);
            }
        }
    }

    public override void RenderImGui()
    {
        ImGui.Begin("Test");

        ImGui.SliderInt("Blur Size", ref Renderer!.ComputeShaderBuffer.Data.BlurSize, 1, 64);

        ImGui.End();
    }

    public override void Update()
    {

    }
}