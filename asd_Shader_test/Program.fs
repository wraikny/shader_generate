namespace shader_test

open System
open Global

module Program =
    [<EntryPoint>]
    let main argv =
        asd.Engine.Initialize("Shader Test", 800, 600, new asd.EngineOption())
            |> ignore
        
        let scene = new asd.Scene()
        let layer = new asd.Layer2D()

        let obj_back =
            let da = new asd.RectF(0.0f, 0.0f, Width, Height)
            let rect = new asd.RectangleShape(DrawingArea=da)
            let col = new asd.Color(10uy, 10uy, 255uy, 255uy)
            new asd.GeometryObject2D(Shape=rect, Color=col)
        
        
        let obj_tx =
            let tx = asd.Engine.Graphics.CreateTexture2D("yozora800600.png")
            new asd.TextureObject2D(Texture=tx)
        
        
        
        scene.AddLayer layer

        layer.AddObject obj_back

        let obj_data = new Shader_Objects(layer)

        obj_data
            .Add(new Rectangle_obj(new asd.Vector2DF(150.0f, 100.0f), WindowSize / 1.5f, 0.0f))
            .Add(new Rectangle_obj(new asd.Vector2DF(70.0f, 150.0f), new asd.Vector2DF(0.5f, 0.8f) * WindowSize, 0.0f))
            .Add(new Circle_obj(new asd.Vector2DF(250.0f, 250.0f), 30.0f))
            .Add(new Light_obj(WindowSize / 2.0f, 0.08f))
            .Add(new Light_obj(WindowSize / 1.6f + new asd.Vector2DF(100.0f, 150.0f), 0.08f))
                |> ignore

        layer.AddPostEffect(new Custom_Post_Effect(obj_data))

        asd.Engine.ChangeScene scene

        let rec loop() =
            if Global.KeyPush asd.Keys.Escape then()
            elif asd.Engine.DoEvents() then
                asd.Engine.Update()
                loop()
        loop()

        asd.Engine.Terminate()
        0