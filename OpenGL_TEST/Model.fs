module Model

open OpenTK
open OpenTK.Graphics.OpenGL4
open Mesh

type Model = {
    Meshes : Mesh list
}

let create path =
    let absPath = System.IO.Path.GetFullPath path
    let model = Utils.getModel absPath
    let meshes = model |> Seq.map (fun mesh -> Mesh.createFromAssimpMesh mesh) |> Seq.toList
    { Meshes = meshes }