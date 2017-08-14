module Shader
open OpenTK
open OpenTK.Graphics.OpenGL4
open Utils

[<Struct>]
type ShaderProgram = | ShaderProgram of int
        


let resetProgram () = GL.UseProgram 0

let getUniformLocation (ShaderProgram(p)) name =
    GL.GetUniformLocation(p, name)


let useProgram (ShaderProgram(p):ShaderProgram) = GL.UseProgram p

let setMat4 name value (ShaderProgram(p) as program) = 
    let mutable m = value
    let loc = getUniformLocation program name
    useProgram program
    GL.UniformMatrix4(loc, false, &m)
#if DEBUG
    printfn "updated uniform %A with value %A" name value
#endif
    resetProgram ()

let setVec3 name (value:Vec3) (ShaderProgram(p) as program) = 
    let loc = getUniformLocation program name
    useProgram program
    GL.Uniform3(loc, value)

let create shaderSpecifiers =
    let loadAndCompileShader (shaderType:ShaderType) path =
        let content = System.IO.File.ReadAllText path
        let shader = GL.CreateShader(shaderType)
        do GL.ShaderSource(shader, content)
        do GL.CompileShader shader
#if DEBUG
        let res = GL.GetShaderInfoLog shader
        printfn "%A" res
        printfn "%A" (GL.GetError())
#endif
        shader

    let shaders = 
        shaderSpecifiers 
        |> Array.map(fun (kind, path) -> loadAndCompileShader kind path)
    let program = GL.CreateProgram()
    for shader in shaders do GL.AttachShader(program, shader)
    GL.LinkProgram(program)
    for shader in shaders do GL.DeleteShader(shader)
    program |> ShaderProgram



type ShaderProgramBuilder(shaderProgram:ShaderProgram) = 
    let id = match shaderProgram with ShaderProgram(id) -> id

    member self.Bind(m, a) =
        printfn "Binding %A" m
        printfn "Value %A" a
        m shaderProgram
        a ()

    member self.Zero () = resetProgram ()