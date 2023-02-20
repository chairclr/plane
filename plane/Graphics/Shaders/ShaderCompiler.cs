using System.Text;
using plane.Diagnostics;
using plane.Graphics.Providers;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace plane.Graphics.Shaders;

public class ShaderCompiler
{
    public const uint SHADER_DEBUG = (1 << 0);
    public const uint SHADER_SKIP_VALIDATION = (1 << 1);
    public const uint SHADER_SKIP_OPTIMIZATION = (1 << 2);
    public const uint SHADER_PACK_MATRIX_ROW_MAJOR = (1 << 3);
    public const uint SHADER_PACK_MATRIX_COLUMN_MAJOR = (1 << 4);
    public const uint SHADER_PARTIAL_PRECISION = (1 << 5);
    public const uint SHADER_FORCE_VS_SOFTWARE_NO_OPT = (1 << 6);
    public const uint SHADER_FORCE_PS_SOFTWARE_NO_OPT = (1 << 7);
    public const uint SHADER_NO_PRESHADER = (1 << 8);
    public const uint SHADER_AVOID_FLOW_CONTROL = (1 << 9);
    public const uint SHADER_PREFER_FLOW_CONTROL = (1 << 10);
    public const uint SHADER_ENABLE_STRICTNESS = (1 << 11);
    public const uint SHADER_ENABLE_BACKWARDS_COMPATIBILITY = (1 << 12);
    public const uint SHADER_IEEE_STRICTNESS = (1 << 13);
    public const uint SHADER_WARNINGS_ARE_ERRORS = (1 << 18);
    public const uint SHADER_RESOURCES_MAY_ALIAS = (1 << 19);
    public const uint ENABLE_UNBOUNDED_DESCRIPTOR_TABLES = (1 << 20);
    public const uint ALL_RESOURCES_BOUND = (1 << 21);
    public const uint SHADER_DEBUG_NAME_FOR_SOURCE = (1 << 22);
    public const uint SHADER_DEBUG_NAME_FOR_BINARY = (1 << 23);
    public const uint SHADER_OPTIMIZATION_LEVEL0 = (1 << 14);
    public const uint SHADER_OPTIMIZATION_LEVEL1 = 0;
    public const uint SHADER_OPTIMIZATION_LEVEL2 = ((1 << 14) | (1 << 15));
    public const uint SHADER_OPTIMIZATION_LEVEL3 = (1 << 15);
    public const uint SHADER_FLAGS2_FORCE_ROOT_SIGNATURE_LATEST = 0;
    public const uint SHADER_FLAGS2_FORCE_ROOT_SIGNATURE_1_0 = (1 << 4);
    public const uint SHADER_FLAGS2_FORCE_ROOT_SIGNATURE_1_1 = (1 << 5);

    public unsafe static T CompileFromFile<T>(Renderer renderer, string path, string entryPoint, string shaderModel)
        where T : Shader
    {
        T shader = (T)Activator.CreateInstance(typeof(T), renderer)!;

        Blob shaderErrors = new Blob();

        uint flags = 0;

#if DEBUG
        flags |= SHADER_DEBUG | SHADER_SKIP_OPTIMIZATION;
#endif

        string src = File.ReadAllText(path);

        nint nativeSourceString = SilkMarshal.StringToPtr(src);

        int hr = D3DCompilerProvider.D3DCompiler.Value.Compile((void*)nativeSourceString, (nuint)src.Length, Path.GetFullPath(path), null, (ID3DInclude*)1, entryPoint, shaderModel, flags, 0, shader.ShaderData.NativeBlob.GetAddressOf(), shaderErrors.NativeBlob.GetAddressOf());

        SilkMarshal.FreeString(nativeSourceString);

        ErrorCheck(hr, shaderErrors);

        shader.Create();

        return shader;
    }

    public unsafe static T CompileFromSourceCode<T>(Renderer renderer, string src, string entryPoint, string shaderModel)
        where T : Shader, new()
    {
        T shader = (T)Activator.CreateInstance(typeof(T), renderer)!;

        Blob shaderErrors = new Blob();

        uint flags = 0;

#if DEBUG
        flags |= SHADER_DEBUG | SHADER_SKIP_OPTIMIZATION;
#endif

        nint nativeSourceString = SilkMarshal.StringToPtr(src);

        int hr = D3DCompilerProvider.D3DCompiler.Value.Compile((void*)nativeSourceString, (nuint)src.Length, (string?)null, null, null, entryPoint, shaderModel, flags, 0, shader.ShaderData.NativeBlob.GetAddressOf(), shaderErrors.NativeBlob.GetAddressOf());

        SilkMarshal.FreeString(nativeSourceString);

        ErrorCheck(hr, shaderErrors);

        shader.Create();

        return shader;
    }

    private static void ErrorCheck(int hr, Blob shaderErrors)
    {
        if (HResult.IndicatesFailure(hr))
        {
            if (!shaderErrors.IsNull)
            {
                string compilerErrors = shaderErrors.AsString()!;

                shaderErrors.Dispose();

                string[] errors = compilerErrors.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (string error in errors)
                {
                    Logger.WriteLine(error, LogSeverity.Error);
                }

                throw new Exception($"Failed to compile shader.\n'{compilerErrors}'");
            }
            else
            {
                SilkMarshal.ThrowHResult(hr);
            }
        }
    }
}