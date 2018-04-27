namespace shader_test


open System.Collections.Generic
open System.Linq
open Global

type VertexInterface =
    abstract vertex_pos : int -> asd.Vector2DF

[<AbstractClass>]
type Base_Shader_Object() as this =
    inherit asd.GeometryObject2D()

    [<DefaultValue>] val mutable color : asd.Color
    [<DefaultValue>] val mutable selected_color : asd.Color

    do
        this.color <- new asd.Color(120uy, 120uy, 120uy, 255uy)
        this.selected_color <- new asd.Color(250uy, 255uy, 0uy, 255uy)

        this.Color <- this.color

    abstract change_color_free : unit -> unit
    default this.change_color_free () =
        this.Color <- this.color
    
    abstract change_color_selected : unit -> unit
    default this.change_color_selected () =
        this.Color <- this.selected_color

    abstract has_point_inside : asd.Vector2DF -> bool

    abstract class_name : string
    default this.class_name = "Base_Shader_Object"


type Rectangle_obj(size, pos, angle) =
    inherit Base_Shader_Object()
    let mutable size : asd.Vector2DF = size

    override this.OnAdded() =
        let da = new asd.RectF(-size / 2.0f, size) in
        this.Shape <- new asd.RectangleShape(DrawingArea=da)
        this.Position <- pos
        this.Angle <- angle
    
    member this.Size
        with get() = size
        and set(value) =
            let da = new asd.RectF(-value / 2.0f, value) in
            this.Shape <- new asd.RectangleShape(DrawingArea=da)
            size <- value
    
    interface VertexInterface with
        member this.vertex_pos index =
            let index = index % 4
            let d = size / 2.0f
            let dv = 
                if index = 0 then
                    new asd.Vector2DF(-d.X, -d.Y)
                else if index = 1 then
                    new asd.Vector2DF(d.X, -d.Y)
                else if index = 2 then
                    new asd.Vector2DF(d.X, d.Y)
                else if index = 3 then
                    new asd.Vector2DF(-d.X, d.Y)
                else new asd.Vector2DF(0.0f, 0.0f)
            
            this.Position + new asd.Vector2DF(dv.X, dv.Y, Degree = dv.Degree + this.Angle)
    
    override this.has_point_inside point =
        let v_pos index =
            (this :> VertexInterface).vertex_pos index
        let inside_p = (v_pos 0 + v_pos 2) / 2.0f

        let f x = otherside_of_line point inside_p (v_pos x) <| v_pos (x+1)

        [0..3].Any(fun x -> f x) |> not

    override this.class_name =
        "Rectangle"


type Polygon_obj(pos, vertex_list, ui_layer) =
    inherit Base_Shader_Object()

    let mutable vertex_list : List<asd.Vector2DF> = vertex_list

    let mutable selected_vertex_index = 0

    let vertex_selected_circle = new asd.GeometryObject2D()

    let vertex_selected_circle_radius = 40.0f

    let ui_layer : asd.Layer2D = ui_layer



    override this.OnAdded() =
        this.Position <- pos
        this.Shape <- 
            let polygon = new asd.PolygonShape()
            vertex_list.ForEach(fun x -> polygon.AddVertex(x))
            polygon

        vertex_selected_circle.Position <- vertex_list.[selected_vertex_index]
        vertex_selected_circle.Shape <- new asd.CircleShape(OuterDiameter=vertex_selected_circle_radius, InnerDiameter=vertex_selected_circle_radius - 5.0f)
        vertex_selected_circle.Color <- new asd.Color(0uy, 255uy, 255uy, 0uy) // new asd.Color(0uy, 255uy, 255uy, 255uy)

        this.AddChild(vertex_selected_circle,
                      asd.ChildManagementMode.Disposal,
                      asd.ChildTransformingMode.All)

        ui_layer.AddObject vertex_selected_circle

    override this.change_color_free () =
        this.Color <- this.color
        vertex_selected_circle.Color <- new asd.Color(0uy, 255uy, 255uy, 0uy)
    
    override this.change_color_selected () =
        this.Color <- this.selected_color
        vertex_selected_circle.Color <- new asd.Color(0uy, 255uy, 255uy, 255uy)
    
    member this.Vertex_List
        with get() = vertex_list
        and set(value) = vertex_list <- value
    
    static member Initialize(n, radius, pos, layer) =
        new Polygon_obj(pos, [0..n-1].Select(fun x -> new asd.Vector2DF(radius, 0.0f, Degree=360.0f / (float32 n) * float32 x)).ToList(), layer)
    
    member this.init_list(n, radius) =
        this.Vertex_List <- [0..n-1].Select(fun x -> new asd.Vector2DF(radius, 0.0f, Degree=360.0f / (float32 n) * float32 x)).ToList()
        this.Shape <- 
            let polygon = new asd.PolygonShape()
            vertex_list.ForEach(fun x -> polygon.AddVertex(x))
            polygon
    
    member this.Find_Vrtex_Index vec =
        if vertex_list.Contains vec then
            Some(vertex_list.IndexOf vec)
        else None
       
    member this.Select_Vertex_Index 
        with get() = selected_vertex_index
        and set(value) =
            vertex_selected_circle.Position <- vertex_list.[selected_vertex_index]
            selected_vertex_index <- value

    member this.Change_Vertex_Pos pos =
        this.Vertex_List.Item(selected_vertex_index) <- pos
        this.Shape <- 
            let polygon = new asd.PolygonShape()
            vertex_list.ForEach(fun x -> polygon.AddVertex(x))
            polygon
        
        vertex_selected_circle.Position <- vertex_list.[selected_vertex_index]

    member this.Remove_Vertex () =
        if this.Vertex_List.Count > 3 then
            this.Vertex_List.RemoveAt this.Select_Vertex_Index
            this.Shape <- 
                let polygon = new asd.PolygonShape()
                vertex_list.ForEach(fun x -> polygon.AddVertex(x))
                polygon
            this.Select_Vertex_Index <- 0
    
    
    interface VertexInterface with
        member this.vertex_pos index =
            let vec = vertex_list.[index % vertex_list.Count]
            this.Position + new asd.Vector2DF(vec.X, vec.Y, Degree=vec.Degree + this.Angle)
    
    override this.has_point_inside point =
        let v_pos index =
            (this :> VertexInterface).vertex_pos index
        let inside_p = 
            let vec = vertex_list.Aggregate(new asd.Vector2DF(0.0f, 0.0f), fun sum x -> sum + x)
            this.Position + new asd.Vector2DF(vec.X, vec.Y, Degree=vec.Degree + this.Angle) / float32 vertex_list.Count

        let f x = otherside_of_line point inside_p (v_pos x) <| v_pos (x+1)

        [0..vertex_list.Count-1].Any(fun x -> f x) |> not
    
    override this.class_name =
        "Vertex"
        

type Circle_obj(center, radius) =
    inherit Base_Shader_Object()

    let mutable radius = radius

    override this.OnAdded() =
        this.Shape <- new asd.CircleShape(OuterDiameter=radius * 2.0f)
        this.Position <- center
    

    member this.Radius
        with get() = radius
        and set(value) =
            this.Shape <- new asd.CircleShape(OuterDiameter=value * 2.0f)
            radius <- value

    override this.has_point_inside point =
        (this.Position - point).SquaredLength < radius * radius
    
    override this.class_name =
        "Circle"
        


type Light_obj(position, brightness, ui_layer) as this =
    inherit Base_Shader_Object()

    let mutable brightness : float32 = brightness
    let selected_circle = new asd.GeometryObject2D()

    let d_rad = 0.2f
    let odi_q = 1.5f
    let ui_layer : asd.Layer2D = ui_layer

    do
        this.Color <- new asd.Color(0uy, 0uy, 0uy, 0uy)
        selected_circle.Color <- new asd.Color(0uy, 0uy, 0uy, 0uy)
        selected_circle.Position <- new asd.Vector2DF(0.0f, 0.0f)
        this.Position <- position
    
    static member Least_Radius = 50.0f

    override this.OnAdded() =
        let radius = Light_obj.Least_Radius + (max brightness 0.0f) * d_rad
        selected_circle.Shape <- new asd.CircleShape(OuterDiameter=radius * odi_q, InnerDiameter=radius * odi_q - 10.0f)

        this.AddChild(selected_circle,
                      asd.ChildManagementMode.Disposal,
                      asd.ChildTransformingMode.Position)
        
        ui_layer.AddObject selected_circle


    override this.OnDispose() =
        selected_circle.Dispose()

    member this.Brightness
        with get() = brightness
        and set(value) =
            brightness <- value
            let radius = Light_obj.Least_Radius + (max brightness 0.0f) * d_rad
            selected_circle.Shape <- new asd.CircleShape(OuterDiameter=radius * odi_q, InnerDiameter=radius * odi_q - 10.0f)


    member this.Radius
        with get() = Light_obj.Least_Radius + (max brightness 0.0f) * d_rad
        and set(value) =
            let radius = (max Light_obj.Least_Radius value)
            selected_circle.Shape <- new asd.CircleShape(OuterDiameter=radius * odi_q, InnerDiameter=radius * odi_q - 10.0f)
            brightness <- max ((value - Light_obj.Least_Radius) / d_rad) 0.0f

    override this.change_color_free () =
        selected_circle.Color <- new asd.Color(0uy, 0uy, 0uy, 0uy)

    override this.change_color_selected () =
        selected_circle.Color <- new asd.Color(0uy, 255uy, 255uy, 255uy)
    
    override this.has_point_inside point =
        let radius = 5.0f
        (this.Position - point).SquaredLength < radius * radius

    override this.class_name =
        "Light"


type Shader_Objects(layer : asd.Layer2D) as this =
    let layer = layer

    let rectangle_objects = new List<Rectangle_obj>()
    let vertex_objects = new List<Polygon_obj>()
    let circle_objects = new List<Circle_obj>()
    let light_objects = new List<Light_obj>()
    let mutable updated_state = true

    [<DefaultValue>] val mutable selected_obj : Option<Base_Shader_Object>

    do
        this.selected_obj <- None
    
    member this.Rectangle_Objects with get() = rectangle_objects
    member this.Polygon_Objects with get() = vertex_objects
    member this.Circle_Objects with get() = circle_objects
    member this.Light_Objects with get() = light_objects

    member this.Updated_State
        with get() = updated_state
        and set(value) = updated_state <- value
    
    member this.Add (x : Base_Shader_Object) =

        x |> function
        | :? Rectangle_obj
        | :? Polygon_obj
        | :? Circle_obj
        | :? Light_obj ->
            this.Updated_State <- true
            layer.AddObject x

            ()


        | _ -> ()

        x |> function
        | :? Rectangle_obj as obj ->
            this.Rectangle_Objects.Add obj
        | :? Polygon_obj as obj ->
            this.Polygon_Objects.Add obj
        | :? Circle_obj as obj ->
            this.Circle_Objects.Add obj
        | :? Light_obj as obj ->
            this.Light_Objects.Add obj
        | _ -> ()

        this

    member this.Remove (x : Base_Shader_Object) =
        x |> function
        | :? Rectangle_obj
        | :? Polygon_obj
        | :? Circle_obj
        | :? Light_obj ->
            this.Updated_State <- true
            layer.AddObject x
        
        | _ -> ()

        x |> function
        | :? Rectangle_obj as obj ->
            this.Rectangle_Objects.Remove obj |> ignore
        | :? Polygon_obj as obj ->
            this.Polygon_Objects.Remove obj |> ignore
        | :? Circle_obj as obj ->
            this.Circle_Objects.Remove obj |> ignore
        | :? Light_obj as obj ->
            this.Light_Objects.Remove obj |> ignore
        | _ -> ()


        x.Dispose()