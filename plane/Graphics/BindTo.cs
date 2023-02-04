using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plane.Graphics;
public enum BindTo
{
    None,
    VertexShader,
    PixelShader,
    GeometryShader,
    ComputeShader,
    HullShader,
    DomainShader,
}
