module Mesh
open OpenTK
open OpenTK.Graphics.OpenGL4
open System
open System.Runtime.InteropServices
open Utils
open Shader

[<Struct>]
type Mesh = {
    Vertices : Vertex[]
    Indices : uint32 []
    VAO : int
    VBO : int
    EBO : int
}

let create (vertices:Vertex []) (indices:uint32 []) =
    let vao, vbo, ebo = GL.GenVertexArray(), GL.GenBuffer(), GL.GenBuffer()
    do GL.BindVertexArray vao
    do GL.BindBuffer(BufferTarget.ArrayBuffer, vbo)
    do GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof<Vertex>, vertices, BufferUsageHint.StaticDraw)

    do GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo)
    do GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof<int>, indices, BufferUsageHint.StaticDraw)

    do GL.EnableVertexAttribArray(0)
    do GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.size, Vertex.offset_Position)
    do GL.EnableVertexAttribArray(1)
    do GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.size, Vertex.offset_Normal)
    do GL.EnableVertexAttribArray(1)
    do GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Vertex.size, Vertex.offset_TexCoord)

    do GL.BindVertexArray(0)

    {   Vertices = vertices
        Indices = indices
        VAO = vao
        VBO = vbo
        EBO = ebo   }
    
let createOpenTKVector3 (vec3:Assimp.Vector3D) = Vec3(vec3.X, vec3.Y, vec3.Z)
    
let createFromAssimpMesh (mesh:Assimp.Mesh) =
    let vertices = 
        [|   for i in 0..(mesh.VertexCount - 1) do
                let pos = createOpenTKVector3 mesh.Vertices.[i]
                let normal = createOpenTKVector3 mesh.Normals.[i]
                yield { Position =  pos; Normal = normal; TexCoord = Vec2(0.f, 1.f)} |]
    let indicies = mesh.GetUnsignedIndices()
    create vertices indicies
    
let draw (mesh:Mesh) (ShaderProgram shaderProgramID) = 
    do GL.BindVertexArray(mesh.VAO)
    do GL.UseProgram shaderProgramID
    do GL.DrawElements(BeginMode.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0)
    do GL.BindVertexArray(0)