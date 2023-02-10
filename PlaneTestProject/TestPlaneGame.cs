using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using plane;
using plane.Diagnostics;
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
        CubeModel = new RenderModel(Renderer!, Path.Combine(Path.GetDirectoryName(typeof(TestPlaneGame).Assembly.Location)!, "Models/cube.obj"));

        Renderer!.RenderObjects.Add(CubeModel);

        Renderer!.Camera.Translation = new Vector3(0f, 0f, -2f);
    }

    private Vector3 CubeRotation = Vector3.Zero;

    public override void Render()
    {
        CubeRotation.X -= DeltaTime;
        CubeRotation.Y += DeltaTime;

        CubeModel!.Transform.EulerRotation = CubeRotation;
    }

    public override void Update()
    {

    }
}