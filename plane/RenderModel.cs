using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using plane.Graphics;

namespace plane;

public class RenderModel : RenderObject
{
    public readonly Model? Model;

    public virtual string? ModelPath { get; protected set; } = null;

    public RenderModel(Renderer renderer)
        : base(renderer)
    {
        if (ModelPath is not null)
        {
            Model = new Model(Renderer, ModelPath);
        }
    }

    public RenderModel(Renderer renderer, string modelPath)
        : base(renderer)
    {
        ModelPath = modelPath;

        Model = new Model(Renderer, ModelPath);
    }

    public override void Render(Camera camera)
    {
        base.Render(camera);

        Model?.Render();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing) 
        {
            Model?.Dispose();
        }
    }
}