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
    
    let loadAndCompileShader (shaderType:ShaderType) path =
        let content = System.IO.File.ReadAllText path
        let shader = GL.CreateShader(shaderType)
        do GL.ShaderSource(shader, content)
        printfn "%A" (GL.GetError())
        do GL.CompileShader shader
        let res = GL.GetShaderInfoLog shader
        printfn "%A" res
        printfn "%A" (GL.GetError())
        shader
    
    
    printfn "%A" (GL.GetError())

    let createFragmentShader path = loadAndCompileShader ShaderType.FragmentShader path
    let createVertexShader path = loadAndCompileShader ShaderType.VertexShader path
    
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
        createVertexShader @"../../../shaders/FirstChapter/SimpleLight.vert",
        createFragmentShader @"../../../shaders/FirstChapter/SimpleLight.frag"
    
    let shaderProgram = createShaderProgram [vertShader; fragShader]
    let shader = { Utils.Shader.ID = shaderProgram }
    let mutable projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.f), 100.f / 100.f, 0.01f, 100.0f)
    let mutable modelMatrix = Matrix4.CreateScale(0.1f) * Matrix4.CreateTranslation(0.f, 0.f, -1.f)// + Matrix4.CreateTranslation(2.f, 0.f, 2.f)
    let mutable normalMatrix = Matrix4.CreateScale(0.1f) * Matrix4.CreateTranslation(0.f, 0.f, -1.f)
    normalMatrix.Transpose()
    let lightPos = Vector3(1.0f, 1.0f, 2.0f)
    let objectColor = Vector3(1.f, 1.f, 1.f)
    let lightColor = Vector3(1.f, 1.f, 1.f)
    let locModel = GL.GetUniformLocation(shader.ID, "model")
    let locView = GL.GetUniformLocation(shader.ID, "view")
    let locProjection = GL.GetUniformLocation(shader.ID, "projection")
    let locLightPos = GL.GetUniformLocation(shader.ID, "lightPos")
    let locLightColor = GL.GetUniformLocation(shader.ID, "lightColor")
    let locObjectColor = GL.GetUniformLocation(shader.ID, "objectColor")
    let locViewPos = GL.GetUniformLocation(shader.ID, "viewPos")
    let locnormalModel = GL.GetUniformLocation(shader.ID, "normalModel")
    printfn "%A" (GL.GetError())
    
    let mutable camera = Utils.Camera.createDefault ()
    let mutable viewMat = Utils.Camera.getViewMatrix camera
    
    GL.UniformMatrix4(locModel, false, &modelMatrix)
    printfn "%A" (GL.GetError())
    GL.UniformMatrix4(locView, false, &viewMat)
    GL.UniformMatrix4(locProjection, false, &projectionMatrix)
    GL.UniformMatrix4(locnormalModel, false, &normalMatrix)
    GL.Uniform3(locViewPos, camera.Position)
    GL.Uniform3(locLightPos, lightPos)
    GL.Uniform3(locLightColor, lightColor)
    GL.Uniform3(locObjectColor, objectColor)
    printfn "%A" (GL.GetError())

    let model = Utils.Model.create "../../../models/nanosuit/nanosuit.obj"
    printfn "%A" (GL.GetError())

    let rec gameLoop lastMouseX lastMouseY =
        let timeout = System.DateTime.Now.AddMilliseconds(32.)
        window.ProcessEvents()
        if window.Mouse.[MouseButton.Left] then
            let dx = lastMouseX - window.Mouse.X |> float
            let dy = lastMouseY - window.Mouse.Y |> float
            Utils.Camera.processMouseInputConstrained &camera dx dy
            let mutable view = Utils.Camera.getViewMatrix camera
            GL.UniformMatrix4(locView, false, &view)
            GL.Uniform3(locLightPos, lightPos)
            printfn "Mouse moved"
        let mouseX, mouseY = window.Mouse.X, window.Mouse.Y
        if window.IsExiting |> not then
            GL.UniformMatrix4(locModel, false, &modelMatrix)
            GL.ClearColor(System.Drawing.Color.Black)
            GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
            model.Meshes |> Seq.iter (fun mesh -> Utils.Mesh.draw mesh shader)
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
