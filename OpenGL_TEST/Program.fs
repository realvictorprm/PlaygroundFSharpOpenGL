// Weitere Informationen zu F# unter "http://fsharp.org".
// Weitere Hilfe finden Sie im Projekt "F#-Tutorial".
open OpenTK
open OpenTK.Graphics.OpenGL4
open System

let startGame () =
    use window = new GameWindow(400, 400, Graphics.GraphicsMode.Default, "F# OpenGL Test", GameWindowFlags.Default)
    window.Visible <- true
    let glContext = window.Context
    
    let loadAndCompileShader (shaderType:ShaderType) path =
        let content = System.IO.File.ReadAllText path
        let shader = GL.CreateShader(shaderType)
        do GL.ShaderSource(shader, content)
        do GL.CompileShader shader
        shader

    let createFragmentShader path = loadAndCompileShader ShaderType.FragmentShader path
    let createVertexShader path = loadAndCompileShader ShaderType.VertexShader path
    
    let createShaderProgram shaders =
        let program = GL.CreateProgram()
        shaders |> List.iter(fun shader -> GL.AttachShader(program, shader))
        do GL.LinkProgram(program)
        program
    
    let vertShader, fragShader = 
        createVertexShader @"../../../shaders/FirstChapter/SimpleLight.vert",
        createFragmentShader @"../../../shaders/FirstChapter/SimpleLight.frag"
    
    let shaderProgram = createShaderProgram [vertShader; fragShader]
    let shader = { Utils.Shader.ID = shaderProgram }

    let model = Utils.Model.create "../../../models/nanosuit/nanosuit.obj"

    let rec gameLoop dt =
        let measure = dt
        let timeout = System.DateTime.Now.AddMilliseconds(16.).Millisecond
        model.Meshes |> Seq.tryHead |> Option.iter (fun mesh -> Utils.Mesh.draw mesh shader)

        System.Threading.Thread.Sleep(timeout - System.DateTime.Now.Millisecond)
        gameLoop measure
    gameLoop 0.016

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    printfn "%A" (System.IO.Directory.GetCurrentDirectory())
    startGame ()
    System.Console.ReadKey() |> ignore
    0 // Integer-Exitcode zurückgeben
