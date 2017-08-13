module Utils
open System.Runtime.InteropServices


let getModel path =
    use context = new Assimp.AssimpContext()
    let scene = context.ImportFile path
    scene.Meshes

type Shader = {
    ID : int
}

[<Struct>]
type Vertex = {
    Position:OpenTK.Vector3
    Normal:OpenTK.Vector3
}

module Vertex =
    let offset_Position = Marshal.OffsetOf(typeof<Vertex>, "Position")
    let offset_Normal = Marshal.OffsetOf(typeof<Vertex>, "Normal")
    let size = Marshal.SizeOf<Vertex>()

[<Struct>]
type Mesh = {
    Vertices : Vertex[]
    Indices : int []
    VAO : int
    VBO : int
    EBO : int
}

module Mesh =
    open OpenTK
    open OpenTK.Graphics.OpenGL4
    open OpenTK.Graphics.OpenGL4
    open System
    open System.Runtime.InteropServices

    let create (vertices:Vertex []) (indices:int []) =
        let vao, vbo, ebo = GL.GenVertexArray(), GL.GenBuffer(), GL.GenBuffer()
        do GL.BindVertexArray vao
        do GL.BindBuffer(BufferTarget.ArrayBuffer, vbo)
        do GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length, vertices, BufferUsageHint.StaticDraw)

        do GL.BindBuffer(BufferTarget.ArrayBuffer, ebo)
        do GL.BufferData(BufferTarget.ArrayBuffer, indices.Length, indices, BufferUsageHint.StaticDraw)

        do GL.EnableVertexAttribArray(0)
        do GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.size, Vertex.offset_Position)
        do GL.EnableVertexAttribArray(1)
        do GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.size, Vertex.offset_Normal)

        do GL.BindVertexArray(0)

        {   Vertices = vertices
            Indices = indices
            VAO = vao
            VBO = vbo
            EBO = ebo   }
    
    let createOpenTKVector3 (vec3:Assimp.Vector3D) = OpenTK.Vector3(vec3.X, vec3.Y, vec3.Z)
    
    let createFromAssimpMesh (mesh:Assimp.Mesh) =
        let vertices = 
            [|   for i in 0..(mesh.VertexCount - 1) do
                    let pos = createOpenTKVector3 mesh.Vertices.[i]
                    let normal = createOpenTKVector3 mesh.Normals.[i]
                    yield { Position =  pos; Normal = normal} |]
        let indicies = mesh.GetIndices()
        create vertices indicies
    
    let draw (mesh:Mesh) (shader:Shader) = 
        do GL.BindVertexArray(mesh.VAO)
        do GL.UseProgram shader.ID
        do GL.DrawElements(BeginMode.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0)
        do GL.BindVertexArray(0)

type Model = {
    Meshes : Mesh seq
}

module Model = 
    open OpenTK
    open OpenTK.Graphics.OpenGL4
    
    let create path =
        let absPath = System.IO.Path.GetFullPath path
        let model = getModel absPath
        { Meshes = model |> Seq.map (fun mesh -> Mesh.createFromAssimpMesh mesh) }
