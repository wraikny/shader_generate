namespace shader_test

open System
open System.Linq
open Global

module Program =
    [<EntryPoint>]
    let main argv =
        asd.Engine.Initialize("Shader Test", 800, 600, new asd.EngineOption())
            |> ignore
        asd.Engine.OpenTool()
        
        let scene = new Editor_Scene()

        asd.Engine.ChangeScene scene

        let rec loop() =
            if Global.KeyPush asd.Keys.Escape then()
            elif asd.Engine.DoEvents() then
                asd.Engine.Update()
                loop()
        loop()

        asd.Engine.CloseTool()
        asd.Engine.Terminate()
        0