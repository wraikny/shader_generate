namespace shader_test

open System
open System.Linq
open Global

type Editor_Scene() as this =
    inherit asd.Scene()

    let layer = new asd.Layer2D()

    let obj_back =
        let da = new asd.RectF(0.0f, 0.0f, Width, Height)
        let rect = new asd.RectangleShape(DrawingArea=da)
        let col = new asd.Color(10uy, 10uy, 255uy, 255uy)
        new asd.GeometryObject2D(Shape=rect, Color=col)
    
    let obj_data = new Shader_Objects(layer)

    let layer_edit = new Edit_Layer(obj_data)

    do
        obj_data
            .Add(new Rectangle_obj(new asd.Vector2DF(150.0f, 100.0f), WindowSize / 1.5f + new asd.Vector2DF(100.0f, 0.0f), 0.0f))
            .Add(new Rectangle_obj(new asd.Vector2DF(70.0f, 150.0f), new asd.Vector2DF(0.5f, 0.8f) * WindowSize, 0.0f))
            .Add(new Vertex_obj(new asd.Vector2DF(600.0f, 150.0f) , [0..4].Select(fun x -> new asd.Vector2DF(60.0f, 0.0f, Degree=72.0f * float32 x)).ToList()))
            .Add(new Circle_obj(new asd.Vector2DF(250.0f, 250.0f), 60.0f))
            .Add(new Light_obj(WindowSize / 2.0f, 0.05f))
            .Add(new Light_obj(WindowSize / 1.6f + new asd.Vector2DF(100.0f, 150.0f), 0.03f))
            .Add(new Light_obj(new asd.Vector2DF(100.0f, 450.0f), 0.03f))
            .Add(new Light_obj(new asd.Vector2DF(700.0f, 150.0f), 0.05f))
                |> ignore

        let add_softlight pos br =
            let n = 12
            let br = br / float32 n
            obj_data.Add(new Light_obj(pos, br)) |> ignore
            [1..n].ToList().ForEach(fun x -> obj_data.Add(new Light_obj(pos + new asd.Vector2DF(5.0f, 0.0f, Degree=360.0f / float32 n * float32 x), br)) |> ignore)
        
        layer.AddPostEffect(new Custom_Post_Effect(obj_data))

        this.AddLayer layer
        this.AddLayer layer_edit

        layer.AddObject obj_back
