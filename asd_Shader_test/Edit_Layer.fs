namespace shader_test

open System
open System.Collections.Generic
open System.Linq
open Global

type Edit_Mode =
    Free
    |Selcted
    |Move
    |Moving
    |Camera

type Edit_Layer(obj_data) =
    inherit asd.Layer2D()

    let mutable edit_mode = Edit_Mode.Free

    let obj_data : Shader_Objects = obj_data

    let mutable mouse_pos_diff = new asd.Vector2DF(0.0f, 0.0f)

    let camera = new Edit_Camera()

    override this.OnAdded() =
        let frame_width = (Width - Height)
        let frame = 
            let da = new asd.RectF(Width - frame_width, 0.0f, frame_width, Height)
            let rect = new asd.RectangleShape(DrawingArea = da)
            let col = new asd.Color(20uy, 20uy, 20uy, 230uy)
            new asd.GeometryObject2D(Color=col, Shape = rect)
        // this.AddObject frame
        ()
    
    member this.ChangeMode (obj_option : Base_Shader_Object option ) = function
        | Edit_Mode.Free ->
            obj_data.selected_obj |> function
            | Some(obj) -> obj.change_color_free()
            | None -> ()
            edit_mode <- Edit_Mode.Free
            obj_data.selected_obj <- None
        | Edit_Mode.Move -> 
            edit_mode |> function
            | Edit_Mode.Selcted | Edit_Mode.Moving ->
                edit_mode <- Edit_Mode.Move
            | _ -> ()
        | Edit_Mode.Selcted ->
            edit_mode |> function
            | Edit_Mode.Move ->
                obj_data.selected_obj |> function
                | Some(obj) ->
                    obj.change_color_selected()
                    edit_mode <- Edit_Mode.Selcted
                | None -> ()
            | Edit_Mode.Free -> 
                obj_option |> function
                | Some(obj) ->
                    obj.change_color_selected()
                    edit_mode <- Edit_Mode.Selcted
                    obj_data.selected_obj <- obj_option
                | None -> ()
            | _ -> ()
        | Edit_Mode.Moving ->
            edit_mode |> function
            | Edit_Mode.Move | Edit_Mode.Selcted ->
                edit_mode <- Edit_Mode.Moving
            | _ -> ()
        | Edit_Mode.Camera ->
            edit_mode |> function
            | Edit_Mode.Free ->
                edit_mode <- Edit_Mode.Camera
            | _ -> ()

    member this.Add_Remove_Window () =
        let title = "Menu"
        if asd.Engine.Tool.Begin title then
            edit_mode |> function
            | Edit_Mode.Free ->
                    asd.Engine.Tool.Text "Add Object\n"
                    let mouse_pos = asd.Engine.Mouse.Position
                    if asd.Engine.Tool.Button("Rectangle") then
                        let obj = new Rectangle_obj(new asd.Vector2DF(100.0f, 100.0f), WindowSize / 2.0f, 0.0f)
                        obj_data.Add(obj) |> ignore
                        let obj_option = Some(obj :> Base_Shader_Object)
                        this.ChangeMode obj_option Edit_Mode.Selcted
                    
                    else if asd.Engine.Tool.Button("Vertex(not impl)") then
                        ()

                    else if asd.Engine.Tool.Button("Circle") then
                        let obj = new Circle_obj(WindowSize / 2.0f, 50.0f)
                        obj_data.Add(obj) |> ignore
                        let obj_option = Some(obj :> Base_Shader_Object)
                        this.ChangeMode obj_option Edit_Mode.Selcted
                        
                    else if asd.Engine.Tool.Button("Light") then
                        let obj = new Light_obj(WindowSize / 2.0f, 0.05f)
                        obj_data.Add(obj) |> ignore
                        let obj_option = Some(obj :> Base_Shader_Object)
                        this.ChangeMode obj_option Edit_Mode.Selcted
            | _ -> ()
            asd.Engine.Tool.End()
    
    member this.Edit_Window () =
        let title = "Edit"
        let mode_name name =
            asd.Engine.Tool.Text("Mode: " + name)
        let selected_type() =
            let objtype_name = obj_data.selected_obj |> function
                | Some(x) -> x.class_name()
                | None -> "None"
                in
            asd.Engine.Tool.Text("Object Type: " + objtype_name)
        
        if asd.Engine.Tool.Begin title then
            edit_mode |> function
            | Edit_Mode.Free -> 
                mode_name "Unselected"
                selected_type()
            | Edit_Mode.Camera ->
                mode_name "Camera"
                asd.Engine.Tool.Text("Object Type: Camera")

            | Edit_Mode.Selcted ->
                mode_name "Selected"
                selected_type()
                if asd.Engine.Tool.Button("Release") then
                    this.ChangeMode None Edit_Mode.Free
                else if asd.Engine.Tool.Button("Move") then
                    this.ChangeMode None Edit_Mode.Move
                else if asd.Engine.Tool.Button("Delete") then
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

                (*else if asd.Engine.Tool.
                    obj_data.selected_obj |> function
                    | Some(x) ->
                        let downcastBase_edit (b1 : Base_Shader_Object) =
                            b1 |> function
                           | :? Rectangle_obj as obj ->
                               ()
                           | :? Vertex_obj as obj ->
                               ()
                           | :? Circle_obj as obj ->
                               ()
                           | :? Light_obj as obj ->
                               ()
                           | _ -> ()
                        downcastBase_edite x
                    | None -> ()
                    ()

                *)
            
            | Edit_Mode.Move ->
                mode_name "Move"
                selected_type()
                if asd.Engine.Tool.Button("Release") then 
                    this.ChangeMode None Edit_Mode.Free
                else if asd.Engine.Tool.Button("Stop Move") then
                    this.ChangeMode None Edit_Mode.Selcted
            | Edit_Mode.Moving ->
                mode_name "Moving"
                selected_type()
            asd.Engine.Tool.End()
        
    override this.OnUpdated() =
        let mouse_pos = asd.Engine.Mouse.Position
        edit_mode |> function
        | Edit_Mode.Free ->
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
                let a = a.OrderBy(fun i -> Guid.NewGuid()).Where(fun x -> x.has_point_inside mouse_pos)
                if a.Count() > 0 then
                    let r = Some(a.First())
                    this.ChangeMode r Edit_Mode.Selcted
        
        | Edit_Mode.Selcted ->
            obj_data.selected_obj |> function
            | Some(obj) ->
                if asd.Engine.Mouse.GetButtonInputState asd.MouseButtons.ButtonLeft = asd.MouseButtonState.Push then
                    if obj.has_point_inside mouse_pos then
                        this.ChangeMode None Edit_Mode.Moving

                if KeyPush asd.Keys.Right then
                    obj.rotate 10.0f
                else if KeyPush asd.Keys.Left then
                    obj.rotate -10.0f
            | None -> ()
        
        | Edit_Mode.Move -> 
            obj_data.selected_obj |> function
            | Some(obj) ->
                if asd.Engine.Mouse.GetButtonInputState asd.MouseButtons.ButtonLeft = asd.MouseButtonState.Push then
                    let mode =
                        if obj.has_point_inside mouse_pos then
                            mouse_pos_diff <- obj.Position - mouse_pos
                            Edit_Mode.Moving
                        else
                            Edit_Mode.Selcted
                    this.ChangeMode None mode
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
        this.Add_Remove_Window()