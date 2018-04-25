namespace shader_test


open System.Collections.Generic
open System.Linq
open Global

type VertexInterface =
    abstract vertex_pos : int -> asd.Vector2DF

[<AbstractClass>]
type Base_Shader_Object() =
    inherit asd.GeometryObject2D()
    member this.color = new asd.Color(50uy, 50uy, 50uy, 255uy)
    // member this.selected_color = new asd.Color(100uy, 250uy, 100uy, 255uy)
    member this.selected_color = new asd.Color(50uy, 50uy, 50uy, 255uy)

    abstract change_color_free : unit -> unit
    default this.change_color_free () =
        this.Color <- this.color
    
    abstract change_color_selected : unit -> unit
    default this.change_color_selected () =
        this.Color <- this.selected_color

    abstract has_point_inside : asd.Vector2DF -> bool

    abstract rotate : float32 -> unit

    abstract class_name : unit -> string
    default this.class_name() =
        "Base_Shader_Object"


type Rectangle_obj(size, pos, angle) as this =
    inherit Base_Shader_Object()
    let mutable size : asd.Vector2DF = size

    do

        let da = new asd.RectF(-size / 2.0f, size) in
        this.Shape <- new asd.RectangleShape(DrawingArea=da)
        this.Position <- pos
        this.Color <- this.color
        this.Angle <- angle
    
    member this.Size
        with get() = size
        and set(value) = size <- value
    
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
    
    override this.rotate dangle =
        this.Angle <- this.Angle + dangle

    override this.class_name() =
        "Rectangle"


type Vertex_obj(pos, vertex_list) as this =
    inherit Base_Shader_Object()

    let vertex_list : List<asd.Vector2DF> = vertex_list

    do
        this.Position <- pos
        this.Shape <- 
            let polygon = new asd.PolygonShape()
            vertex_list.ForEach(fun x -> polygon.AddVertex(x))
            polygon
        this.Color <- this.color


    member this.Vertex_List
        with get() = vertex_list

    interface VertexInterface with
        member this.vertex_pos index =
            this.Position + vertex_list.[index % vertex_list.Count]
    
    override this.has_point_inside point =
        let v_pos index =
            (this :> VertexInterface).vertex_pos index
        let inside_p = 
            this.Position + vertex_list.Aggregate(new asd.Vector2DF(0.0f, 0.0f), fun sum x -> sum + x) / float32 vertex_list.Count

        let f x = otherside_of_line point inside_p (v_pos x) <| v_pos (x+1)

        [0..3].Any(fun x -> f x) |> not

    override this.rotate dangle =
        this.Angle <- this.Angle + dangle

    override this.class_name() =
        "Vertex"
        

type Circle_obj(center, radius) as this =
    inherit Base_Shader_Object()

    do
        this.Shape <- new asd.CircleShape(OuterDiameter=radius * 2.0f)
        this.Position <- center
        this.Color <- this.color

    member this.radius = radius

    override this.has_point_inside point =
        (this.Position - point).SquaredLength < radius * radius

    override this.rotate dangle =
        this.Angle <- this.Angle + dangle

    override this.class_name() =
        "Circle"
        


type Light_obj(position, brightness) as this =
    inherit Base_Shader_Object()

    let mutable position : asd.Vector2DF = position
    let brightness : float32 = brightness

    do
        this.Shape <- new asd.CircleShape(OuterDiameter=10.0f * 2.0f)
        this.Position <- position
        this.Color <- new asd.Color(0uy, 0uy, 0uy, 0uy)

    member this.Position
        with get() = position
        and set(value) = position <- value
    
    member this.Brightness
        with get() = brightness
    
    override this.has_point_inside point =
        let radius = 5.0f
        (this.Position - point).SquaredLength < radius * radius

    override this.change_color_free() =
        this.Color <- new asd.Color(255uy, 0uy, 0uy, 0uy)

    override this.rotate dangle =
        this.Angle <- this.Angle + dangle

    override this.class_name() =
        "Light"


type Shader_Objects(layer : asd.Layer2D) as this =
    let layer = layer

    let rectangle_objects = new List<Rectangle_obj>()
    let vertex_objects = new List<Vertex_obj>()
    let circle_objects = new List<Circle_obj>()
    let light_objects = new List<Light_obj>()

    [<DefaultValue>] val mutable selected_obj : Option<Base_Shader_Object>
    [<DefaultValue>] val mutable updated_state : bool

    do
        this.updated_state <- true
        this.selected_obj <- None
    
    member this.Rectangle_Objects with get() = rectangle_objects
    member this.Vertex_Objects with get() = vertex_objects
    member this.Circle_Objects with get() = circle_objects
    member this.Light_Objects with get() = light_objects
    
    member this.Add x =
        this.updated_state <- true
        this.Rectangle_Objects.Add x
        layer.AddObject x
        this
    
    member this.Remove x =
        this.updated_state <- true
        this.Rectangle_Objects.Remove x
            |> ignore
        layer.RemoveObject x

    member this.Add x =
        this.updated_state <- true
        this.Vertex_Objects.Add x
        layer.AddObject x
        this
    
    member this.Remove x =
        this.updated_state <- true
        this.Vertex_Objects.Remove x
            |> ignore
        layer.RemoveObject x

    member this.Add x =
        this.updated_state <- true
        this.Circle_Objects.Add x
        layer.AddObject x
        this
    
    member this.Remove x =
        this.updated_state <- true
        this.Circle_Objects.Remove x
            |> ignore
        layer.RemoveObject x

    member this.Add x =
        this.updated_state <- true
        this.Light_Objects.Add x
        layer.AddObject x
        this

    member this.Remove x =
        this.updated_state <- true
        this.Light_Objects.Remove x
            |> ignore
        layer.RemoveObject x
