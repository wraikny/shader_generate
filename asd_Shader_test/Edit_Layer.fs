namespace shader_test

open System
open Global

type Edit_Layer(obj_data) =
    inherit asd.Layer2D()

    let obj_data = obj_data

    override this.OnAdded() =
        let frame_width = (Width - Height)
        let frame = 
            let da = new asd.RectF(Width - frame_width, 0.0f, frame_width, Height)
            let rect = new asd.RectangleShape(DrawingArea = da)
            let col = new asd.Color(20uy, 20uy, 20uy, 230uy)
            new asd.GeometryObject2D(Color=col, Shape = rect)
        this.AddObject frame

        let button_size = new asd.Vector2DF(0.8f, 0.3f) * frame_width
        let button_color = new asd.Color(255uy, 255uy, 255uy)
        let button_pos yi = new asd.Vector2DF(Width - frame_width / 2.0f, button_size.Y * float32 yi + frame_width / 2.0f)

        new Button(button_size, button_pos 0, button_color, "Rect", Button_Action.AddRect) |> this.AddObject

        ()