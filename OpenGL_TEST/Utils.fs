module Utils
open System.Runtime.InteropServices
open System.ComponentModel


let getModel path =
    use context = new Assimp.AssimpContext()
    let scene = context.ImportFile path
    scene.Meshes

type Shader = {
    ID : int
}

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type Vertex = {
    Position:OpenTK.Vector3
    Normal:OpenTK.Vector3
    TexCoord:OpenTK.Vector2
}

type Camera = {
    Position : OpenTK.Vector3
    mutable Front : OpenTK.Vector3
    mutable Up : OpenTK.Vector3
    mutable Right : OpenTK.Vector3
    WorldUp : OpenTK.Vector3
    mutable Yaw : float
    mutable Pitch : float
    MovementSpeed : float
    MouseSensitivity : float
    Zoom : float
}

module Camera =
    open OpenTK
    open OpenTK

    let updateCameraVectors (camera:byref<Camera>) =
        let yaw, pitch = MathHelper.DegreesToRadians camera.Yaw, MathHelper.DegreesToRadians camera.Pitch
        let fx = (cos yaw) * (cos pitch) |> float32
        let fy = sin pitch |> float32
        let fz = sin yaw * cos pitch |> float32
        camera.Front <- Vector3.Normalize(Vector3(fx, fy, fz))
        camera.Right <- Vector3.Normalize(Vector3.Cross(camera.Front, camera.WorldUp))
        camera.Up <- Vector3.Normalize(Vector3.Cross(camera.Right, camera.Front))

    let processMouseInputConstrained (camera:byref<Camera>) xoffset yoffset =
        camera.Yaw <- camera.Yaw - xoffset * camera.MouseSensitivity
        let newPitch =
            let p = camera.Pitch + yoffset * camera.MouseSensitivity
            if p > 89.0 then 89.0
            elif p < -89.0 then -89.0
            else p
        camera.Pitch <- newPitch
        updateCameraVectors &camera

    let getViewMatrix camera = Matrix4.LookAt(camera.Position, camera.Position + camera.Front, camera.Up)

    let create position up yaw pitch front movementSpeed mouseSensitivity zoom =
        let mutable camera = 
            {   Position = position
                Front = front
                Up = Vector3.Zero
                Right = Vector3.Zero
                WorldUp = up
                Yaw = yaw
                Pitch = pitch
                MovementSpeed = movementSpeed
                MouseSensitivity = mouseSensitivity
                Zoom = zoom }
        updateCameraVectors &camera
        camera

    let createDefault () =
        create (Vector3.Zero) (Vector3(0.f, 1.f, 0.f)) -90.0 0. (Vector3(0.f, 0.f, 1.f)) 2.5 0.1 45.

module Vertex =
    let offsetof name = Marshal.OffsetOf(typeof<Vertex>, name)
    let offset_Position = offsetof "Position@"
    let offset_Normal = offsetof "Normal@"
    let offset_TexCoord = offsetof "TexCoord@"
    let size = Marshal.SizeOf<Vertex>()

[<Struct>]
type Mesh = {
    Vertices : Vertex[]
    Indices : uint32 []
    VAO : int
    VBO : int
    EBO : int
}

module Mesh =
    open OpenTK
    open OpenTK.Graphics.OpenGL4
    open System
    open System.Runtime.InteropServices

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
    
    let createOpenTKVector3 (vec3:Assimp.Vector3D) = OpenTK.Vector3(vec3.X, vec3.Y, vec3.Z)
    
    let createFromAssimpMesh (mesh:Assimp.Mesh) =
        let vertices = 
            [|   for i in 0..(mesh.VertexCount - 1) do
                    let pos = createOpenTKVector3 mesh.Vertices.[i]
                    let normal = createOpenTKVector3 mesh.Normals.[i]
                    yield { Position =  pos; Normal = normal; TexCoord = OpenTK.Vector2(0.f, 1.f)} |]
        let indicies = mesh.GetUnsignedIndices()
        create vertices indicies
    
    let draw (mesh:Mesh) (shader:Shader) = 
        do GL.BindVertexArray(mesh.VAO)
        do GL.UseProgram shader.ID
        do GL.DrawElements(BeginMode.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0)
        do GL.BindVertexArray(0)

type Model = {
    Meshes : Mesh list
}

module Model = 
    open OpenTK
    open OpenTK.Graphics.OpenGL4
    
    let create path =
        let absPath = System.IO.Path.GetFullPath path
        let model = getModel absPath
        let meshes = model |> Seq.map (fun mesh -> Mesh.createFromAssimpMesh mesh) |> Seq.toList
        { Meshes = meshes }
