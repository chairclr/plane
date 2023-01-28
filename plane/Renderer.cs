using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Windowing;

namespace plane;

public class Renderer : IDisposable
{
    public Renderer(IWindow window)
    {

    }

    public void Render()
    {
        // Do Rendering Things
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
