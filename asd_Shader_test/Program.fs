namespace shader_test

open System
open Global

module Program =
    [<EntryPoint>]
    let main argv =
        asd.Engine.Initialize("Shader Test", 800, 600, new asd.EngineOption())
            |> ignore
        
        let scene = new asd.Scene()
        let layer_back = new asd.Layer2D()
        let layer = new asd.Layer2D()

        let obj_back =
            let da = new asd.RectF(0.0f, 0.0f, Width, Height)
            let rect = new asd.RectangleShape(DrawingArea=da)
            let col = new asd.Color(10uy, 10uy, 255uy, 255uy)
            new asd.GeometryObject2D(Shape=rect, Color=col)
        
        
        let obj_tx =
            let tx = asd.Engine.Graphics.CreateTexture2D("yozora800600.png")
            new asd.TextureObject2D(Texture=tx)
        

        let obj = 
            let da = new asd.RectF(100.0f, 100.0f, Width / 10.0f, Height / 10.0f)
            let rect = new asd.RectangleShape(DrawingArea=da)
            let col = new asd.Color(255uy, 250uy, 255uy, 155uy)
            new asd.GeometryObject2D(Shape=rect, Color=col)

        let player = new Player()
        
        let objects_data = new Shader_Objects()
        objects_data.Rectangle_Objects.Add(new Rectangle_obj(new asd.Vector2DF(100.0f, 100.0f), WindowSize / 2.0f))
        
        scene.AddLayer layer_back
        // scene.AddLayer layer

        layer_back.AddObject obj_back

        layer_back.AddObject player

        layer.AddObject obj


        layer_back.AddPostEffect(new Custom_Post_Effect(player, objects_data))

        asd.Engine.ChangeScene scene

        let rec loop() =
            if Global.KeyPush asd.Keys.Escape then()
            elif asd.Engine.DoEvents() then
                asd.Engine.Update()
                loop()
        loop()

        asd.Engine.Terminate()
        0