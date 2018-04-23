namespace shader_test

open System.Collections.Generic

type VertexInterface =
    abstract vertex_pos : int -> asd.Vector2DF

type Rectangle_obj(size, pos) as this =
    inherit asd.GeometryObject2D()
    let mutable size : asd.Vector2DF = size

    do
        this.Shape <-
            let da = new asd.RectF(-size / 2.0f, size)
            new asd.RectangleShape(DrawingArea=da)
        this.Position <- pos
        this.Color <- new asd.Color(50uy, 50uy, 50uy, 255uy)
    
    member this.Size
        with get() = size
        and set(value) = size <- value

    interface VertexInterface with
        member this.vertex_pos index =
            let d = size / 2.0f
            let rec dvec vi = vi |> function
                | 0 -> new asd.Vector2DF(d, Degree=this.Angle)
                | 1 -> new asd.Vector2DF(d.X, -d.Y, Degree=this.Angle)
                | 2 -> new asd.Vector2DF(d.X, d.Y, Degree=this.Angle)
                | 3 -> new asd.Vector2DF(-d.X, d.Y, Degree=this.Angle)
                | n -> dvec (vi % 4)
            

            this.Position + d * dvec index
    

type Vertex_obj(vertex_list) as this =
    inherit asd.GeometryObject2D()
    let vertex_list : List<asd.Vector2DF> = vertex_list

    do
        this.Shape <- 
            let polygon = new asd.PolygonShape()
            vertex_list.ForEach(fun x -> polygon.AddVertex(x))
            polygon

    member this.Vertex_List
        with get() = vertex_list

    interface VertexInterface with
        member this.vertex_pos index =
            vertex_list.[index % vertex_list.Count]


type Circle_obj(center, radius) as this =
    inherit asd.GeometryObject2D()
    let radius : float32 = radius

    do
        this.Shape <-
            new asd.CircleShape(OuterDiameter=radius * 2.0f)
        this.Position <- center
    
    member this.Radius
        with get() = radius


type Light_obj(position, brightness) =
    let position : asd.Vector2DF = position
    let brightness : float32 = brightness

    member this.Position
        with get() = position
    
    member this.Brightness
        with get() = brightness


type Shader_Objects() =
    let rectangle_objects = new List<Rectangle_obj>()
    let vertex_objects = new List<Vertex_obj>()
    let circle_objects = new List<Circle_obj>()
    let light_objects = new List<Light_obj>()
    let mutable updated_state = true

    member this.Rectangle_Objects
        with get() = rectangle_objects
    
    member this.Vertex_Objects
        with get() = vertex_objects

    member this.circle_Objects
        with get() = circle_objects

    member this.Light_Objects
        with get() = light_objects

    member this.Updated_State
        with get() = updated_state
        and set(value) = updated_state <- value

