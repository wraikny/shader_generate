namespace shader_test

open System
open System.Linq
open Global

type Editor_Scene() as this =
    inherit asd.Scene()

    let mutable camera_pos = new Camera_Pos()

    let layer = new asd.Layer2D()

    let obj_back =
        let da = new asd.RectF(-WindowSize, WindowSize * 2.0f)
        let rect = new asd.RectangleShape(DrawingArea=da)
        let col = new asd.Color(10uy, 10uy, 255uy, 255uy)
        new asd.GeometryObject2D(Shape=rect, Color=col)
    
    let obj_data = new Shader_Objects(layer)

    let layer_edit = new Edit_Layer(obj_data, camera_pos)

    do
        layer.AddObject obj_back
        let WindowSize_ = new asd.Vector2DF(Height, Height)
        obj_data
            .Add(new Rectangle_obj(new asd.Vector2DF(70.0f, 150.0f), new asd.Vector2DF(0.5f, 0.8f) * WindowSize_, 0.0f))
            .Add(new Polygon_obj(new asd.Vector2DF(500.0f, 150.0f) , [0..4].Select(fun x -> new asd.Vector2DF(60.0f, 0.0f, Degree=72.0f * float32 x)).ToList(), layer_edit))
            .Add(new Circle_obj(new asd.Vector2DF(250.0f, 250.0f), 60.0f))
            .Add(new Light_obj(new asd.Vector2DF(100.0f, 450.0f), 30.0f, layer_edit))
            .Add(new Light_obj(new asd.Vector2DF(400.0f, 150.0f), 50.0f, layer_edit))
            .Add(new Light_obj(new asd.Vector2DF(470.0f, 350.0f), 50.0f, layer_edit))
                |> ignore
        
        layer.AddPostEffect(new Custom_Post_Effect(obj_data, camera_pos))

        this.AddLayer layer
        this.AddLayer layer_edit

        layer.AddObject <| new Edit_Camera(camera_pos)
