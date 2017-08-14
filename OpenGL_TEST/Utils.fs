module Utils
open System.Runtime.InteropServices
open System.ComponentModel

type Vec3 = OpenTK.Vector3
type Vec2 = OpenTK.Vector2
type Mat4 = OpenTK.Matrix4
type Mat3 = OpenTK.Matrix3

let getModel path =
    use context = new Assimp.AssimpContext()
    let scene = context.ImportFile path
    scene.Meshes

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
type Vertex = {
    Position:Vec3
    Normal:Vec3
    TexCoord:Vec2
}

type Camera = {
    Position : Vec3
    mutable Front : Vec3
    mutable Up : Vec3
    mutable Right : Vec3
    WorldUp : Vec3
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
        camera.Front <- Vec3.Normalize(Vec3(fx, fy, fz))
        camera.Right <- Vec3.Normalize(Vec3.Cross(camera.Front, camera.WorldUp))
        camera.Up <- Vec3.Normalize(Vec3.Cross(camera.Right, camera.Front))

    let processMouseInputConstrained (camera:byref<Camera>) xoffset yoffset =
        camera.Yaw <- camera.Yaw - xoffset * camera.MouseSensitivity
        let newPitch =
            let p = camera.Pitch + yoffset * camera.MouseSensitivity
            if p > 89.0 then 89.0
            elif p < -89.0 then -89.0
            else p
        camera.Pitch <- newPitch
        updateCameraVectors &camera

    let getViewMatrix camera = Mat4.LookAt(camera.Position, camera.Position + camera.Front, camera.Up)

    let create position up yaw pitch front movementSpeed mouseSensitivity zoom =
        let mutable camera = 
            {   Position = position
                Front = front
                Up = Vec3.Zero
                Right = Vec3.Zero
                WorldUp = up
                Yaw = yaw
                Pitch = pitch
                MovementSpeed = movementSpeed
                MouseSensitivity = mouseSensitivity
                Zoom = zoom }
        updateCameraVectors &camera
        camera

    let createDefault () =
        create (Vec3.Zero) (Vec3(0.f, 1.f, 0.f)) -90.0 0. (Vec3(0.f, 0.f, 1.f)) 2.5 0.1 45.

module Vertex =
    let offsetof name = Marshal.OffsetOf(typeof<Vertex>, name)
    let offset_Position = offsetof "Position@"
    let offset_Normal = offsetof "Normal@"
    let offset_TexCoord = offsetof "TexCoord@"
    let size = Marshal.SizeOf<Vertex>()

