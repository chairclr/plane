using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using plane.Diagnostics;
using plane.Graphics.Providers;
using Silk.NET.Assimp;
using Silk.NET.Direct3D.Compilers;
using SixLabors.ImageSharp.PixelFormats;

namespace plane.Graphics;

public class Model : IDisposable
{
    private readonly Renderer Renderer;

    public readonly List<Mesh> Meshes;

    private string? ModelImportDirectory;

    public Model(Renderer renderer)
    {
        Renderer = renderer;

        Meshes = new List<Mesh>();
    }

    public Model(Renderer renderer, string modelPath)
    {
        Renderer = renderer;

        Meshes = new List<Mesh>();

        LoadModelFromPath(modelPath);
    }

    private unsafe void LoadModelFromPath(string modelPath)
    {
        string path = Path.GetFullPath(modelPath);

        ModelImportDirectory = Path.GetDirectoryName(path);

        ref Scene scene = ref Unsafe.AsRef<Scene>(AssimpProvider.Assimp.Value.ImportFile(path, (uint)(PostProcessSteps.Triangulate | PostProcessSteps.SortByPrimitiveType | PostProcessSteps.FlipWindingOrder | PostProcessSteps.FlipUVs)));

        ref Node rootNode = ref Unsafe.AsRef<Node>(scene.MRootNode);

        ProcessRootNode(rootNode, scene);

        AssimpProvider.Assimp.Value.FreeScene(scene);
    }

    private unsafe void ProcessRootNode(in Node rootNode, in Scene scene)
    {
        for (uint i = 0; i < rootNode.MNumMeshes; i++)
        {
            ref Silk.NET.Assimp.Mesh mesh = ref Unsafe.AsRef<Silk.NET.Assimp.Mesh>(scene.MMeshes[i]);

            if ((mesh.MPrimitiveTypes & (uint)PrimitiveType.Triangle) != 0)
            {
                Meshes.Add(ProcessMesh(mesh, scene));
            }
        }

        for (uint i = 0; i < rootNode.MNumChildren; i++)
        {
            ref Node childNode = ref Unsafe.AsRef<Node>(rootNode.MChildren[i]);

            for (uint j = 0; j < childNode.MNumMeshes; j++)
            {
                ref Silk.NET.Assimp.Mesh mesh = ref Unsafe.AsRef<Silk.NET.Assimp.Mesh>(scene.MMeshes[j]);

                if ((mesh.MPrimitiveTypes & (uint)PrimitiveType.Triangle) != 0)
                {
                    Meshes.Add(ProcessMesh(mesh, scene));
                }
            }
        }
    }

    private unsafe Mesh ProcessMesh(in Silk.NET.Assimp.Mesh mesh, in Scene scene)
    {
        List<Vertex> vertices = new List<Vertex>((int)mesh.MNumVertices);

        List<int> indicies = new List<int>((int)mesh.MNumFaces * 3);

        for (int i = 0; i < mesh.MNumVertices; i++)
        {
            Vertex vertex = new Vertex
            {
                Position = mesh.MVertices[i]
            };

            if (mesh.MTextureCoords[0] != null)
            {
                vertex.UV.X = mesh.MTextureCoords[0][i].X;
                vertex.UV.Y = mesh.MTextureCoords[0][i].Y;
            }

            if (mesh.MNormals != null)
            {
                vertex.Normal = mesh.MNormals[i];
            }

            vertices.Add(vertex);
        }

        for (int i = 0; i < mesh.MNumFaces; i++)
        {
            Face face = mesh.MFaces[i];

            for (int j = 0; j < face.MNumIndices; j++)
            {
                indicies.Add((int)face.MIndices[j]);
            }
        }

        ref Material material = ref Unsafe.AsRef<Material>(scene.MMaterials[mesh.MMaterialIndex]);

        List<Texture2D> textures = ProcessMaterialTextures(material, scene);

        return new Mesh(Renderer, vertices, indicies, textures);
    }

    private unsafe List<Texture2D> ProcessMaterialTextures(in Material material, in Scene scene)
    {
        List<Texture2D> textures = new List<Texture2D>();

        textures.AddRange(ProcessMaterialTexturesDiffuse(material, scene));

        return textures;
    }

    private unsafe List<Texture2D> ProcessMaterialTexturesDiffuse(in Material material, in Scene scene)
    {
        List<Texture2D> textures = new List<Texture2D>();

        uint textureCount = AssimpProvider.Assimp.Value.GetMaterialTextureCount(material, Silk.NET.Assimp.TextureType.Diffuse);

        if (textureCount == 0)
        {
            Vector4 color = default;

            Return ret = AssimpProvider.Assimp.Value.GetMaterialColor(material, Assimp.MatkeyColorDiffuse, 0, 0, ref color);

            if (ret == Return.Success)
            {
                Texture2D planeTexture = Texture2D.GetSinglePixelTexture(Renderer, new Rgba32(color));
                planeTexture.NativeTexture.SetObjectName($"{scene.MName.AsString}MaterialDiffuseColor");    
                textures.Add(planeTexture);
            }
        }
        else
        {
            for (int i = 0; i < textureCount; i++)
            {
                MaterialProperty* property = null;

                if (AssimpProvider.Assimp.Value.GetMaterialProperty(material, Assimp.MatkeyTextureBase, (uint)Silk.NET.Assimp.TextureType.Diffuse, (uint)i, property) == Return.Success)
                {
                    string fileRelative = ((AssimpString*)property->MData)->AsString;

                    if (fileRelative.StartsWith(@"/") || fileRelative.StartsWith(@"\"))
                        fileRelative = fileRelative.Remove(0, 1);

                    string fullPath = Path.Combine(ModelImportDirectory!, fileRelative);
                    Texture2D planeTexture = Texture2D.LoadFromFile(Renderer, fullPath);
                    planeTexture.NativeTexture.SetObjectName($"{scene.MName.AsString}MaterialTextureBase");
                    textures.Add(planeTexture);
                }
            }
        }

        return textures;
    }

    public void Render()
    {
        foreach (Mesh mesh in Meshes)
        {
            mesh.Render();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (Mesh mesh in Meshes)
        {
            mesh.Dispose();
        }
    }
}
