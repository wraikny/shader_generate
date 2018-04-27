namespace shader_test

open System
open System.Collections.Generic
open System.Linq
open Global

type Edit_Mode =
      Free
    | Selectable
    | Selected
    | Moving
    | Camera
    | Transform
    | Moving_Transform

type Edit_Layer(obj_data, camera_pos) =
    inherit asd.Layer2D()

    let mutable edit_mode = Edit_Mode.Free

    let obj_data : Shader_Objects = obj_data

    let camera_pos : Camera_Pos = camera_pos

    let mutable mouse_pos_diff = new asd.Vector2DF(0.0f, 0.0f)

    let mutable camera_pos_save = camera_pos.position

    let click_mergin = 30.0f

    override this.OnAdded() =
        let frame_width = (Width - Height)
        let frame = 
            let da = new asd.RectF(Width - frame_width, 0.0f, frame_width, Height)
            let rect = new asd.RectangleShape(DrawingArea = da)
            let col = new asd.Color(20uy, 20uy, 20uy, 230uy)
            new asd.GeometryObject2D(Color=col, Shape = rect)
        // this.AddObject frame
        this.AddObject <| new Edit_Camera(camera_pos)

    
    member this.ChangeMode (obj_option : Base_Shader_Object option) =
        function
        | Edit_Mode.Free as mode ->
            obj_data.selected_obj |> function
            | Some(obj) ->
                obj.change_color_free()
            | None -> ()
            edit_mode <- mode
            obj_data.selected_obj <- None

        | Edit_Mode.Selected as mode ->
            edit_mode |> function
            | Edit_Mode.Selectable | Edit_Mode.Free ->
                obj_option |> function
                | Some(obj) ->
                    obj.change_color_selected()
                    edit_mode <- mode
                    obj_data.selected_obj <- obj_option
                | None -> ()
            | Edit_Mode.Moving | Edit_Mode.Transform ->
                edit_mode <- mode
            | _ -> ()

        | Edit_Mode.Moving as mode ->
            edit_mode |> function
            | Edit_Mode.Selected ->
                edit_mode <- mode
            | _ -> ()

        | Edit_Mode.Camera as mode ->
            edit_mode |> function
            | Edit_Mode.Free ->
                edit_mode <- mode
            | _ -> ()

        | Edit_Mode.Selectable as mode ->
            edit_mode |> function
            | Edit_Mode.Free ->
                edit_mode <- mode
            | Edit_Mode.Selected ->
                edit_mode <- mode
                obj_data.selected_obj |> function
                | Some(obj) ->
                    obj.change_color_free()
                | None -> ()
                obj_data.selected_obj <- None
            | _ -> ()
        
        | Edit_Mode.Transform as mode ->
            edit_mode |> function
            | Edit_Mode.Selected | Edit_Mode.Moving_Transform ->
                edit_mode <- Edit_Mode.Transform
            | _ -> ()
        
        | Edit_Mode.Moving_Transform as mode ->
            edit_mode |> function
            | Edit_Mode.Transform ->
                edit_mode <- Edit_Mode.Moving_Transform
            | _ -> ()
    
    
    member this.Menu_Window () =

        let title = "Menu"
        let mode_name name =
            asd.Engine.Tool.Text("Mode: " + name)
        
        let selected_type() =
            let objtype_name = obj_data.selected_obj |> function
                | Some(x) -> x.class_name
                | None -> "None"
                in
            asd.Engine.Tool.Text("Object Type: " + objtype_name)

        
        edit_mode |> function
        | Edit_Mode.Free ->
            if asd.Engine.Tool.Begin title then
                mode_name "Menu"
                selected_type()

                let mouse_pos = asd.Engine.Mouse.Position
                if asd.Engine.Tool.Button("Edit") then
                    this.ChangeMode None Edit_Mode.Selectable
                
                if asd.Engine.Tool.Button("Camera") then
                    this.ChangeMode None Edit_Mode.Camera
                
                asd.Engine.Tool.Text "Add Object"

                let center = WindowSize / 2.0f + camera_pos.position.To2DF()

                if asd.Engine.Tool.Button("Rectangle") then
                    let obj = new Rectangle_obj(new asd.Vector2DF(60.0f, 60.0f), center, 0.0f)
                    obj_data.Add(obj) |> ignore
                    let obj_option = Some(obj :> Base_Shader_Object)
                    this.ChangeMode obj_option Edit_Mode.Selected
                
                if asd.Engine.Tool.Button("Polygon") then
                    let obj = Polygon_obj.Initialize(3, 60.0f, WindowSize / 2.0f + camera_pos.position.To2DF(), this)
                    obj_data.Add(obj) |> ignore
                    let obj_option = Some(obj :> Base_Shader_Object)
                    this.ChangeMode obj_option Edit_Mode.Selected

                if asd.Engine.Tool.Button("Circle") then
                    let obj = new Circle_obj(center, 50.0f)
                    obj_data.Add(obj) |> ignore
                    let obj_option = Some(obj :> Base_Shader_Object)
                    this.ChangeMode obj_option Edit_Mode.Selected
                    
                if asd.Engine.Tool.Button("Light") then
                    let obj = new Light_obj(center, 50.0f, this)
                    obj_data.Add(obj) |> ignore
                    let obj_option = Some(obj :> Base_Shader_Object)
                    this.ChangeMode obj_option Edit_Mode.Selected

                asd.Engine.Tool.End()
        
        | Edit_Mode.Camera ->
            if asd.Engine.Tool.Begin title then
                mode_name "Camera"
                asd.Engine.Tool.Text("Object Type: Camera")

                if asd.Engine.Tool.Button("Enter") || KeyPush asd.Keys.Enter then
                    this.ChangeMode None Edit_Mode.Free

                let pos = camera_pos.position
                let x_list = [|pos.X|]
                let y_list = [|pos.Y|]

                if asd.Engine.Tool.InputInt("X", x_list) then
                    camera_pos.position <- new asd.Vector2DI(x_list.[0], pos.Y)
                    Console.WriteLine("({0}, {1})", pos.X, pos.Y)
                
                if asd.Engine.Tool.InputInt("Y", y_list) then
                    camera_pos.position <- new asd.Vector2DI(pos.X, y_list.[0])
                    Console.WriteLine("({0}, {1})", pos.X, pos.Y)
                asd.Engine.Tool.End()

        | Edit_Mode.Selectable ->
            if asd.Engine.Tool.Begin title then
                mode_name "Select"
                selected_type()

                if asd.Engine.Tool.Button("Enter") || KeyPush asd.Keys.Enter then
                    this.ChangeMode None Edit_Mode.Free

                asd.Engine.Tool.End()

        | Edit_Mode.Selected ->
            if asd.Engine.Tool.Begin title then
                mode_name "Selected"
                selected_type()

                if asd.Engine.Tool.Button("Enter") || KeyPush asd.Keys.Enter then
                    this.ChangeMode None Edit_Mode.Free

                if asd.Engine.Tool.Button("Another") then
                    this.ChangeMode None Edit_Mode.Selectable
                
                
                asd.Engine.Tool.Text"Position"
                obj_data.selected_obj |> function
                | Some(obj) ->
                    let pos_x = [|obj.Position.X |> int|]
                    let pos_y = [|obj.Position.Y |> int|]
                    if asd.Engine.Tool.InputInt("X", pos_x) then
                        obj.Position <- new asd.Vector2DF(pos_x.[0] |> float32, pos_y.[0] |> float32)
                    if asd.Engine.Tool.InputInt("Y", pos_y) then
                        obj.Position <- new asd.Vector2DF(pos_x.[0] |> float32, pos_y.[0] |> float32)

                    let angle_update () =
                        let angle_l = [|obj.Angle |> int|]
                        if asd.Engine.Tool.InputInt("Angle", angle_l) then
                            obj.Angle <- angle_l.[0] |> float32
                    
                    obj |> function
                    | :? Rectangle_obj as obj ->
                        angle_update()
                        asd.Engine.Tool.Text "\n"
                        if asd.Engine.Tool.Button("Transform") then
                            this.ChangeMode None Edit_Mode.Transform

                        let size_x = [|obj.Size.X |> int|]
                        let size_y = [|obj.Size.Y |> int|]
                        if asd.Engine.Tool.InputInt("Width", size_x) then
                            obj.Size <- new asd.Vector2DF(size_x.[0] |> float32, size_y.[0] |> float32)
                        if asd.Engine.Tool.InputInt("Height", size_y) then
                            obj.Size <- new asd.Vector2DF(size_x.[0] |> float32, size_y.[0] |> float32)
                        
                        
                        
                    | :? Polygon_obj as obj ->
                        angle_update()
                        asd.Engine.Tool.Text "\n"

                        if asd.Engine.Tool.Button("Transform") then
                            this.ChangeMode None Edit_Mode.Transform
                        
                        let v_cnt = [|obj.Vertex_List.Count|]
                        if asd.Engine.Tool.InputInt("Vertex Num", v_cnt) then
                            obj.init_list(max 3 v_cnt.[0], obj.Vertex_List.Select(fun x -> x.Length).Average())
                            obj_data.Updated_State <- true
                    
                    | :? Circle_obj as obj ->
                        asd.Engine.Tool.Text "\n"

                        if asd.Engine.Tool.Button("Transform") then
                            this.ChangeMode None Edit_Mode.Transform
                        
                        let rad_l = [|obj.Radius |> int|]
                        if asd.Engine.Tool.InputInt("Radius", rad_l) then
                            obj.Radius <- float32 <| max 10 rad_l.[0]
                        
                    | :? Light_obj as obj ->
                        asd.Engine.Tool.Text "\n"

                        if asd.Engine.Tool.Button("Transform") then
                            this.ChangeMode None Edit_Mode.Transform
                        
                        let bri_l = [|int obj.Brightness|]
                        if asd.Engine.Tool.InputInt("Brightness", bri_l) then
                            obj.Brightness <- (float32 <| max 10 bri_l.[0])
                        
                    | _ -> ()

                    asd.Engine.Tool.Text "\n"
                    if asd.Engine.Tool.Button("Delete") then
                        obj_data.Remove obj
                        printfn "Remove"
                        this.ChangeMode None Edit_Mode.Free

                | None -> ()

                asd.Engine.Tool.End()


            | Edit_Mode.Moving ->
                if asd.Engine.Tool.Begin title then
                    mode_name "Moving"
                    selected_type()

                    asd.Engine.Tool.Button("Enter") |> ignore
                    
                    asd.Engine.Tool.Button("Another") |> ignore

                    asd.Engine.Tool.Text"Position"
                    obj_data.selected_obj |> function
                    | Some(obj) ->
                        let pos_x = [|obj.Position.X |> int|]
                        let pos_y = [|obj.Position.Y |> int|]
                        if asd.Engine.Tool.InputInt("X", pos_x) then
                            obj.Position <- new asd.Vector2DF(pos_x.[0] |> float32, pos_y.[0] |> float32)
                        if asd.Engine.Tool.InputInt("Y", pos_y) then
                            obj.Position <- new asd.Vector2DF(pos_x.[0] |> float32, pos_y.[0] |> float32)

                        obj |> function
                        | :? Rectangle_obj as obj ->
                            asd.Engine.Tool.InputInt("Angle", [|obj.Angle |> int|]) |> ignore
                            asd.Engine.Tool.Text "\n"
                            asd.Engine.Tool.Button("Another") |> ignore
                            asd.Engine.Tool.InputInt("Width", [|int obj.Size.X|]) |> ignore
                            asd.Engine.Tool.InputInt("Height", [|int obj.Size.Y|]) |> ignore

                            ()
                        | :? Polygon_obj as obj ->
                            asd.Engine.Tool.InputInt("Angle", [|obj.Angle |> int|]) |> ignore
                            asd.Engine.Tool.Text "\n"
                            asd.Engine.Tool.Button("Another") |> ignore
                            asd.Engine.Tool.InputInt("Vertex Num", [|obj.Vertex_List.Count|]) |> ignore

                        | :? Circle_obj as obj ->
                            asd.Engine.Tool.Text "\n"
                            asd.Engine.Tool.Button("Another") |> ignore
                            asd.Engine.Tool.InputInt("Radius", [|int obj.Radius|]) |> ignore

                        | :? Light_obj as obj ->
                            asd.Engine.Tool.Text "\n"
                            asd.Engine.Tool.Button("Another") |> ignore
                            asd.Engine.Tool.InputInt("Brightness", [|int obj.Brightness|]) |> ignore

                        | _ -> ()


                    | None -> ()

                    asd.Engine.Tool.End()

            | Edit_Mode.Transform ->
                if asd.Engine.Tool.Begin title then
                    mode_name "Transform"
                    selected_type()
                    if asd.Engine.Tool.Button("Enter") || KeyPush asd.Keys.Enter then 
                        this.ChangeMode None Edit_Mode.Selected
                    
                    obj_data.selected_obj |> function
                    | Some(x) ->
                        x |> function
                        | :? Rectangle_obj as obj ->
                            asd.Engine.Tool.Text "not impl"

                        | :? Polygon_obj as obj ->
                            asd.Engine.Tool.Text "Transform: Click"
                            asd.Engine.Tool.Text "\n"
                            if asd.Engine.Tool.Button("Delete Vertex") then
                                obj.Remove_Vertex()
                                obj_data.Updated_State <- true


                        | :? Circle_obj as obj ->
                            asd.Engine.Tool.Text "Radius: Click"

                        | :? Light_obj as obj ->
                            asd.Engine.Tool.Text "Brightness: Click"
                        | _ -> ()
                    | None -> ()

                    asd.Engine.Tool.End()

            | Edit_Mode.Moving_Transform ->
                if asd.Engine.Tool.Begin title then
                    mode_name "Transforming"
                    selected_type()
                    asd.Engine.Tool.End()
                
        
    override this.OnUpdated() =
        let mouse_pos = asd.Engine.Mouse.Position
        let mouse_camera_pos = mouse_pos + camera_pos.position.To2DF()

        if KeyPush asd.Keys.Escape then
            this.ChangeMode None Edit_Mode.Free

        edit_mode |> function
        | Edit_Mode.Free -> ()
        | Edit_Mode.Selectable ->
            if MouseLeftReleased() then
                let rectos = obj_data.Rectangle_Objects.OfType<Base_Shader_Object>()
                let vertexos = obj_data.Polygon_Objects.OfType<Base_Shader_Object>()
                let circleos = obj_data.Circle_Objects.OfType<Base_Shader_Object>()
                let lightos = obj_data.Light_Objects.OfType<Base_Shader_Object>()

                let a = new System.Collections.Generic.List<Base_Shader_Object>()
                a.AddRange rectos
                a.AddRange vertexos
                a.AddRange circleos
                a.AddRange lightos
                let a = a.OrderBy(fun i -> Guid.NewGuid()).Where(fun x -> x.has_point_inside mouse_camera_pos)
                if a.Count() > 0 then
                    let r = Some(a.First())
                    this.ChangeMode r Edit_Mode.Selected
        
        | Edit_Mode.Transform ->
            obj_data.selected_obj |> function
            | Some(x) ->
                if MouseLeftPushed() then
                    x |> function
                    | :? Rectangle_obj as obj ->

                        ()
                    | :? Polygon_obj as obj ->
                        let length_shorter_mergin (vec : asd.Vector2DF) = vec.SquaredLength < click_mergin * click_mergin
                        let a = obj.Vertex_List.OrderBy(fun i -> Guid.NewGuid()).Where(fun x -> (obj.Position + x - mouse_camera_pos) |> length_shorter_mergin)
                        
                        if a.Count() > 0 then
                            obj.Find_Vrtex_Index(a.First()) |> function
                            | Some(index) ->
                                obj.Select_Vertex_Index <- index
                            | None -> ()

                            this.ChangeMode None Edit_Mode.Moving_Transform
                    
                    | :? Circle_obj as obj ->
                        if ((obj.Position - mouse_camera_pos).Length - obj.Radius) < click_mergin then
                            this.ChangeMode None Edit_Mode.Moving_Transform
                    
                    | :? Light_obj as obj ->
                        if ((obj.Position - mouse_camera_pos).Length - obj.Radius) < click_mergin then
                            this.ChangeMode None Edit_Mode.Moving_Transform
                    | _ -> ()
            | None -> ()


        | Edit_Mode.Moving_Transform ->
            obj_data.selected_obj |> function
            | Some(x) ->
                x |> function
                | :? Rectangle_obj as obj ->
                    ()
                | :? Polygon_obj as obj ->
                    obj.Change_Vertex_Pos (mouse_pos + camera_pos.position.To2DF() - obj.Position)

                | :? Circle_obj as obj ->
                    obj.Radius <- max 10.0f <|  (obj.Position - mouse_camera_pos).Length
                
                | :? Light_obj as obj ->
                    obj.Radius <- (obj.Position - mouse_camera_pos).Length

                | _ -> ()
            | None -> ()

            if MouseLeftReleased() then
                this.ChangeMode None Edit_Mode.Transform

        | Edit_Mode.Selected ->
            obj_data.selected_obj |> function
            | Some(obj) ->
                if MouseLeftPushed() then
                    if obj.has_point_inside mouse_camera_pos then
                        mouse_pos_diff <- obj.Position - mouse_camera_pos
                        this.ChangeMode None Edit_Mode.Moving

            | None -> ()
        
        | Edit_Mode.Moving ->
            obj_data.selected_obj |> function
            | Some(obj) ->
                obj.Position <- mouse_camera_pos + mouse_pos_diff
                if MouseLeftReleased () then
                    this.ChangeMode None Edit_Mode.Selected
            |None -> ()
        | Edit_Mode.Camera ->
            if MouseLeftPushed() then
                mouse_pos_diff <- new asd.Vector2DF(abs mouse_pos.X, abs mouse_pos.Y)
                camera_pos_save <- camera_pos.position
            else if MouseLeftHold() then
                camera_pos.position <- camera_pos_save - (mouse_pos - mouse_pos_diff).To2DI()
            ()
        
        this.Menu_Window()