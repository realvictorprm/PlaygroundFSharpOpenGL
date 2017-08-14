// Weitere Informationen zu F# unter "http://fsharp.org".
// Weitere Hilfe finden Sie im Projekt "F#-Tutorial".
open OpenTK
open OpenTK.Graphics.OpenGL4
open System
open OpenTK.Input

let startGame () =
    OpenTK.Toolkit.Init(ToolkitOptions.Default) |> ignore
    
    use window = new GameWindow(800, 800, Graphics.GraphicsMode.Default, "F# OpenGL Test", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, Graphics.GraphicsContextFlags.Debug ||| Graphics.GraphicsContextFlags.ForwardCompatible)
    window.Visible <- true
    window.MakeCurrent()
    let glContext = window.Context
    glContext.LoadAll()
    glContext.ErrorChecking <- true
    GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill)
    GL.Enable(EnableCap.DepthTest)
    GL.Viewport(0, 0, 800, 800)
    
    
    printfn "%A" (GL.GetError())
    
    
    let createShaderProgram shaders =
        let program = GL.CreateProgram()
        shaders |> List.iter(fun shader -> GL.AttachShader(program, shader))
        do GL.LinkProgram(program)
        printfn "%A" (GL.GetError())
        let res = GL.GetProgramInfoLog(program)
        printfn "%A" res
        do GL.UseProgram(program)
        printfn "%A" (GL.GetError())
        program
    
    let vertShader, fragShader = 
        @"../../../shaders/FirstChapter/SimpleLight.vert",
        @"../../../shaders/FirstChapter/SimpleLight.frag"

    let shaderProgram = Shader.create [| ShaderType.FragmentShader, fragShader; ShaderType.VertexShader, vertShader |]
    let projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.f), 100.f / 100.f, 0.01f, 100.0f)
    let modelMatrix = Matrix4.CreateScale(0.1f) * Matrix4.CreateTranslation(0.f, 0.f, -1.f)// + Matrix4.CreateTranslation(2.f, 0.f, 2.f)
    let normalMatrix = Matrix4.CreateScale(0.1f) * Matrix4.CreateTranslation(0.f, 0.f, -1.f)
    let mutable camera = Utils.Camera.createDefault ()
    let viewMat = Utils.Camera.getViewMatrix camera
    normalMatrix.Transpose()
    let lightPos = Vector3(1.0f, 1.0f, 2.0f)
    let objectColor = Vector3(1.f, 1.f, 1.f)
    let lightColor = Vector3(1.f, 1.f, 1.f)
    printfn "%A" (GL.GetError())

    let currShader = Shader.ShaderProgramBuilder(shaderProgram)

    let updateShaderUniforms modelMatrix projectionMatrix camera normalMatrix lightPos objectColor =
        currShader {
            do! Shader.setMat4 "model" modelMatrix
            do! Shader.setMat4 "projection" projectionMatrix
            do! Shader.setMat4 "view" (Utils.Camera.getViewMatrix camera)
            do! Shader.setMat4 "normalModel" normalMatrix
            do! Shader.setVec3 "lightPos" lightPos
            do! Shader.setVec3 "objectColor" objectColor
            do! Shader.setVec3 "viewPos" camera.Position
        }

    updateShaderUniforms modelMatrix projectionMatrix camera normalMatrix lightPos objectColor

    printfn "%A" (GL.GetError())
    
    printfn "%A" (GL.GetError())

    let model = Model.create "../../../models/nanosuit/nanosuit.obj"
    printfn "%A" (GL.GetError())

    let rec gameLoop lastMouseX lastMouseY =
        let timeout = System.DateTime.Now.AddMilliseconds(32.)
        window.ProcessEvents()
        if window.Mouse.[MouseButton.Left] then
            let dx = lastMouseX - window.Mouse.X |> float
            let dy = lastMouseY - window.Mouse.Y |> float
            Utils.Camera.processMouseInputConstrained &camera dx dy
            updateShaderUniforms modelMatrix projectionMatrix camera normalMatrix lightPos objectColor
            printfn "Mouse moved"
        let mouseX, mouseY = window.Mouse.X, window.Mouse.Y
        if window.IsExiting |> not then
            GL.ClearColor(System.Drawing.Color.Black)
            GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
            model.Meshes |> Seq.iter (fun mesh -> Mesh.draw mesh shaderProgram)
            window.SwapBuffers()
            //printfn "%A" (GL.GetError())

            System.Threading.Thread.Sleep (int(max ((timeout - System.DateTime.Now).TotalMilliseconds) 0.))
            gameLoop mouseX mouseY
        else ()
    gameLoop (window.Mouse.X) (window.Mouse.Y)

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    printfn "%A" (System.IO.Directory.GetCurrentDirectory())
    startGame ()
    System.Console.ReadKey() |> ignore
    0 // Integer-Exitcode zurückgeben
