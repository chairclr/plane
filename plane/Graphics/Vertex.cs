using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace plane.Graphics;

public struct Vertex
{
    public Vector3 Position;

    public Vector2 UV;

    public Vector3 Normal;

    public Vertex(Vector3 position, Vector2 uv, Vector3 normal)
    {
        Position = position;
        UV = uv;
        Normal = normal;
    }
}