namespace shader_test

type Player() as this =
    inherit asd.GeometryObject2D()

    let speed = 6.0f
    let angle_speed = 2.0f

    let size = new asd.Vector2DF(100.0f, 100.0f)
    do
        this.Shape <-
            let da = new asd.RectF(-50.0f, -50.0f, 100.0f, 100.0f)
            new asd.RectangleShape(DrawingArea=da)
        this.Position <- asd.Engine.WindowSize.To2DF() / 2.0f
        this.Color <- new asd.Color(50uy, 50uy, 50uy, 255uy)

    member this.get_point_pos index =
        let d = size / 2.0f
        this.Position + new asd.Vector2DF(d.X, d.Y, Degree=this.Angle - 135.0f + 90.0f * float32 index)


    override this.OnUpdate() =
        let held key =
            asd.Engine.Keyboard.GetKeyState key = asd.KeyState.Hold
        
        this.Position <- this.Position + 
            if held asd.Keys.Right then
                new asd.Vector2DF(speed, 0.0f)
            else if held asd.Keys.Left then
                new asd.Vector2DF(-speed, 0.0f)
            else if held asd.Keys.Up then
                new asd.Vector2DF(0.0f, -speed)
            else if held asd.Keys.Down then
                new asd.Vector2DF(0.0f, speed)
            else new asd.Vector2DF(0.0f, 0.0f)

        this.Angle <- this.Angle +
            if held asd.Keys.Period then
                angle_speed
            else if held asd.Keys.Comma then
                -angle_speed
            else 0.0f