namespace shader_test

open System
open System.Collections.Generic
open System.Linq
open Global

type Edit_Mode =
    Free
    |Selectable
    |Selected
    |Move
    |Moving
    |Camera

type Edit_Layer(obj_data, camera_pos) =
    inherit asd.Layer2D()

    let mutable edit_mode = Edit_Mode.Free

    let obj_data : Shader_Objects = obj_data

    let mutable mouse_pos_diff = new asd.Vector2DF(0.0f, 0.0f)

    let camera_pos : Camera_Pos = camera_pos

    override this.OnAdded() =
        let frame_width = (Width - Height)
        let frame = 
            let da = new asd.RectF(Width - frame_width, 0.0f, frame_width, Height)
            let rect = new asd.RectangleShape(DrawingArea = da)
            let col = new asd.Color(20uy, 20uy, 20uy, 230uy)
            new asd.GeometryObject2D(Color=col, Shape = rect)
        // this.AddObject frame
        this.AddObject <| new Edit_Camera(camera_pos)
        ()
    
    member this.ChangeMode (obj_option : Base_Shader_Object option) = function
        | Edit_Mode.Free ->
            obj_data.selected_obj |> function
            | Some(obj) ->
                obj.change_color_free()
            | None -> ()
            edit_mode <- Edit_Mode.Free
            obj_data.selected_obj <- None

        | Edit_Mode.Move -> 
            edit_mode |> function
            | Edit_Mode.Selected | Edit_Mode.Moving ->
                edit_mode <- Edit_Mode.Move
            | _ -> ()

        | Edit_Mode.Selected ->
            edit_mode |> function
            | Edit_Mode.Move ->
                obj_data.selected_obj |> function
                | Some(obj) ->
                    obj.change_color_selected()
                    edit_mode <- Edit_Mode.Selected
                | None -> ()
            | Edit_Mode.Selectable | Edit_Mode.Free ->
                obj_option |> function
                | Some(obj) ->
                    obj.change_color_selected()
                    edit_mode <- Edit_Mode.Selected
                    obj_data.selected_obj <- obj_option
                | None -> ()
            | _ -> ()

        | Edit_Mode.Moving ->
            edit_mode |> function
            | Edit_Mode.Move | Edit_Mode.Selected ->
                edit_mode <- Edit_Mode.Moving
            | _ -> ()

        | Edit_Mode.Camera ->
            edit_mode |> function
            | Edit_Mode.Free ->
                edit_mode <- Edit_Mode.Camera
            | _ -> ()

        | Edit_Mode.Selectable ->
            edit_mode |> function
            | Edit_Mode.Free ->
                edit_mode <- Edit_Mode.Selectable
            | Edit_Mode.Selected ->
                edit_mode <- Edit_Mode.Selectable
                obj_data.selected_obj |> function
                | Some(obj) ->
                    obj.change_color_free()
                | None -> ()
                obj_data.selected_obj <- None
            | _ -> ()

    member this.Menu_Window () =
        let title = "Menu"
        if asd.Engine.Tool.Begin title then
            edit_mode |> function
            | Edit_Mode.Free ->
                asd.Engine.Tool.Text "Edit Mode\n"
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
                
                if asd.Engine.Tool.Button("Vertex(not impl)") then
                    ()

                if asd.Engine.Tool.Button("Circle") then
                    let obj = new Circle_obj(center, 50.0f)
                    obj_data.Add(obj) |> ignore
                    let obj_option = Some(obj :> Base_Shader_Object)
                    this.ChangeMode obj_option Edit_Mode.Selected
                    
                if asd.Engine.Tool.Button("Light") then
                    let obj = new Light_obj(center, 0.05f)
                    obj_data.Add(obj) |> ignore
                    let obj_option = Some(obj :> Base_Shader_Object)
                    this.ChangeMode obj_option Edit_Mode.Selected
                    
            | _ ->
                if asd.Engine.Tool.Button("Back (Menu)") then
                    this.ChangeMode None Edit_Mode.Free

            asd.Engine.Tool.End()
    
    member this.Edit_Window () =
        let title = "Edit"
        let mode_name name =
            asd.Engine.Tool.Text("Mode: " + name)
        let selected_type() =
            let objtype_name = obj_data.selected_obj |> function
                | Some(x) -> x.class_name
                | None -> "None"
                in
            asd.Engine.Tool.Text("Object Type: " + objtype_name)
        
        if asd.Engine.Tool.Begin title then
            edit_mode |> function
            | Edit_Mode.Free -> 
                mode_name "Menu"
                selected_type()
            | Edit_Mode.Camera ->
                mode_name "Camera"
                asd.Engine.Tool.Text("Object Type: Camera")

                if asd.Engine.Tool.Button("Enter") then
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
                
            | Edit_Mode.Selectable ->
                mode_name "Selectable"
                selected_type()
                if asd.Engine.Tool.Button("Enter") then 
                    this.ChangeMode None Edit_Mode.Free

            | Edit_Mode.Selected ->
                mode_name "Selected"
                selected_type()
                if asd.Engine.Tool.Button("Enter") then
                    this.ChangeMode None Edit_Mode.Selectable
                if asd.Engine.Tool.Button("Move(Mouse)") then
                    this.ChangeMode None Edit_Mode.Move
                
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
                        let size_x = [|obj.Size.X |> int|]
                        let size_y = [|obj.Size.Y |> int|]
                        if asd.Engine.Tool.InputInt("Width", size_x) then
                            obj.Size <- new asd.Vector2DF(size_x.[0] |> float32, size_y.[0] |> float32)
                        if asd.Engine.Tool.InputInt("Height", size_y) then
                            obj.Size <- new asd.Vector2DF(size_x.[0] |> float32, size_y.[0] |> float32)
                        
                    | :? Vertex_obj as obj ->
                        angle_update()
                        asd.Engine.Tool.Text "\n"

                    | :? Circle_obj as obj ->
                        asd.Engine.Tool.Text "\n"
                        let rad_l = [|obj.Radius |> int|]
                        if asd.Engine.Tool.InputInt("Radius", rad_l) then
                            obj.Radius <- float32 <| max 1 rad_l.[0]
                        
                    | :? Light_obj as obj ->
                        asd.Engine.Tool.Text "\n"
                        let bri_l = [|(obj.Brightness * 1000.0f) |> int|]
                        if asd.Engine.Tool.InputInt("Brightness", bri_l) then
                            obj.Brightness <- (float32 <| bri_l.[0]) / 1000.0f
                        
                    | _ -> ()
                | None -> ()
                
                if asd.Engine.Tool.Button("Delete") then
                    obj_data.selected_obj |> function
                    | Some(x) ->
                        let x = x
                        let downcastBase_remove (b1 : Base_Shader_Object) =
                            b1 |> function
                            | :? Rectangle_obj as obj -> obj_data.Remove obj
                            | :? Vertex_obj as obj -> obj_data.Remove obj
                            | :? Circle_obj as obj -> obj_data.Remove obj
                            | :? Light_obj as obj -> obj_data.Remove obj
                            | _ -> ()
                        downcastBase_remove x
                        printfn "Remove"
                    | None -> ()
                    this.ChangeMode None Edit_Mode.Free
            
            | Edit_Mode.Move ->
                mode_name "Move"
                selected_type()
                if asd.Engine.Tool.Button("Enter") then 
                    this.ChangeMode None Edit_Mode.Selected
                
            | Edit_Mode.Moving ->
                mode_name "Moving"
                selected_type()
            asd.Engine.Tool.End()
        
    override this.OnUpdated() =
        let mouse_pos = asd.Engine.Mouse.Position
        edit_mode |> function
        | Edit_Mode.Free -> ()
        | Edit_Mode.Selectable ->
            if asd.Engine.Mouse.GetButtonInputState asd.MouseButtons.ButtonLeft = asd.MouseButtonState.Push then
                let rectos = obj_data.Rectangle_Objects.OfType<Base_Shader_Object>()
                let vertexos = obj_data.Vertex_Objects.OfType<Base_Shader_Object>()
                let circleos = obj_data.Circle_Objects.OfType<Base_Shader_Object>()
                let lightos = obj_data.Light_Objects.OfType<Base_Shader_Object>()

                let a = new System.Collections.Generic.List<Base_Shader_Object>()
                a.AddRange rectos
                a.AddRange vertexos
                a.AddRange circleos
                a.AddRange lightos
                let a = a.OrderBy(fun i -> Guid.NewGuid()).Where(fun x -> x.has_point_inside (mouse_pos + camera_pos.position.To2DF()))
                if a.Count() > 0 then
                    let r = Some(a.First())
                    this.ChangeMode r Edit_Mode.Selected
        
        | Edit_Mode.Selected ->
            obj_data.selected_obj |> function
            | Some(obj) ->
                if asd.Engine.Mouse.GetButtonInputState asd.MouseButtons.ButtonLeft = asd.MouseButtonState.Push then
                    if obj.has_point_inside (mouse_pos + camera_pos.position.To2DF()) then
                        this.ChangeMode None Edit_Mode.Moving

            | None -> ()
        
        | Edit_Mode.Move -> 
            obj_data.selected_obj |> function
            | Some(obj) ->
                if asd.Engine.Mouse.GetButtonInputState asd.MouseButtons.ButtonLeft = asd.MouseButtonState.Push then
                    if obj.has_point_inside (mouse_pos + camera_pos.position.To2DF()) then
                        mouse_pos_diff <- obj.Position - mouse_pos
                        this.ChangeMode None Edit_Mode.Moving
            | None -> ()
        
        | Edit_Mode.Moving ->
            obj_data.selected_obj |> function
            | Some(obj) ->
                obj.Position <- mouse_pos + mouse_pos_diff
                if asd.Engine.Mouse.GetButtonInputState asd.MouseButtons.ButtonLeft = asd.MouseButtonState.Push then
                    this.ChangeMode None Edit_Mode.Move
            |None -> ()
        | Edit_Mode.Camera ->
            ()
        
        this.Edit_Window()
        this.Menu_Window()