using System.Text;
using plane.Graphics.Direct3D11;
using Silk.NET.Core.Native;

namespace plane.Graphics.Shaders;

public class ShaderCompiler
{
    public unsafe static T CompileFromSourceCode<T>(string src, string entryPoint, string shaderModel)
        where T : Shader, new()
    {
        T shader = new T();

        using ComPtr<ID3D10Blob> shaderErrors = default;

        uint flags = 0;

#if DEBUG

#endif

        int hr = D3DCompilerProvider.D3DCompiler.Value.Compile((void*)SilkMarshal.StringToPtr(src), (nuint)src.Length, (string?)null, null, null, entryPoint, shaderModel, flags, 0, shader.ShaderData.GetAddressOf(), shaderErrors.GetAddressOf());

        if (HResult.IndicatesFailure(hr))
        {
            if (shaderErrors.Handle is not null)
            {
                byte* stringPointer = (byte*)shaderErrors.Get().GetBufferPointer();
                int stringLength = (int)shaderErrors.Get().GetBufferSize();

                string compilerErrors = Encoding.UTF8.GetString(stringPointer, stringLength);

                shaderErrors.Dispose();

                throw new Exception($"Failed to compile shader.\n'{compilerErrors}'");
            }
            else
            {
                SilkMarshal.ThrowHResult(hr);
            }
        }

        return shader;
    }
}