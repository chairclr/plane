using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using plane;
using Plane = plane.Plane;

namespace PlaneTestProject;

public class TestPlaneGame : Plane
{
    public RenderModel? CubeModel;

    public TestPlaneGame(string windowName) 
        : base(windowName)
    {

    }

    public override void Load()
    {
        CubeModel = new RenderModel(Renderer!, "Models/cube.obj");

        Renderer!.RenderObjects.Add(CubeModel);

        Renderer!.Camera.Translation = new Vector3(0f, 0f, -0.6f);
    }

    private Vector3 CubeRotation = Vector3.Zero;

    public override void Render()
    {
        //CubeRotation.X -= DeltaTime;
        //CubeRotation.Y += DeltaTime;

        //CubeModel!.Transform.EulerRotation = CubeRotation;

        Console.WriteLine($"FPS: {1 / PreciseDeltaTime:F2}");
    }

    public override void Update()
    {

    }
}
