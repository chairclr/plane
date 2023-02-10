﻿#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace plane.Generators;

[Generator]
public class StructAlignmentGenerator : ISourceGenerator
{
    private static readonly string AlignAttributeName = "StructAlign16Attribute";
    private static readonly string AlignAttributeNameShort = "StructAlign16";

    public void Initialize(GeneratorInitializationContext context)
    {

    }

    public void Execute(GeneratorExecutionContext context)
    {
        Compilation compilation = context.Compilation;
        IEnumerable<SyntaxNode> allNodes = compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
        IEnumerable<StructDeclarationSyntax> allStructs = allNodes
            .Where(d => d.IsKind(SyntaxKind.StructDeclaration))
            .OfType<StructDeclarationSyntax>();

        IEnumerable<StructDeclarationSyntax> alignableStructs = allStructs.Where(x => x.AttributeLists.SelectMany(y => y.Attributes).Where(attr => attr.Name.ToString() == AlignAttributeName || attr.Name.ToString() == AlignAttributeNameShort).Any());

        foreach (StructDeclarationSyntax structToAlign in alignableStructs)
        {
            int structSize = SizeofStruct(structToAlign);

            if (structSize % 16 == 0)
                continue;

            int alignedStructSize = structSize + (16 - (structSize % 16));

            string structNamespace = GetNamespace(structToAlign);

            context.AddSource($"{structToAlign.Identifier}.g.cs", 
                $$"""
                  // <auto-generated/>
                  using System.Runtime.InteropServices;

                  {{(structNamespace.Length > 0 ? $"namespace {structNamespace};" : "")}}
                  
                  [StructLayout(LayoutKind.Sequential, Size = {{alignedStructSize}})]
                  public partial struct {{structToAlign.Identifier}} { }
                  """);

        }
    }


    private static int SizeofStruct(StructDeclarationSyntax s)
    {
        int size = 0;

        foreach (FieldDeclarationSyntax field in s.Members.Where(x => x.IsKind(SyntaxKind.FieldDeclaration)))
        {
            VariableDeclarationSyntax variable = field.Declaration;

            size += SizeofTypeName(variable.Type.ToString());
        }

        return size;
    }

    private static int SizeofTypeName(string typeName) => 
        typeName switch
        {
            "bool" or "int" or "uint" or "float" => 4,
            "long" or "ulong" or "double" => 8,
            "Vector2" => Unsafe.SizeOf<Vector2>(),
            "Vector3" => Unsafe.SizeOf<Vector3>(),
            "Vector4" => Unsafe.SizeOf<Vector4>(),
            "Matrix3x2" => Unsafe.SizeOf<Matrix3x2>(),
            "Matrix4x4" => Unsafe.SizeOf<Matrix4x4>(),
            _ => throw new NotSupportedException($"Cannot compute size of {typeName}"),
        };

    private static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        string nameSpace = string.Empty;

        SyntaxNode? potentialNamespaceParent = syntax.Parent;

        while (potentialNamespaceParent != null &&
                potentialNamespaceParent is not NamespaceDeclarationSyntax
                && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            nameSpace = namespaceParent.Name.ToString();

            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        return nameSpace;
    }
}