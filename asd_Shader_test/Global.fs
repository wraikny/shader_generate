namespace shader_test
open System

module Global =
    let Width = asd.Engine.WindowSize.To2DF().X
    let Height = asd.Engine.WindowSize.To2DF().Y
    let WindowSize = asd.Engine.WindowSize.To2DF()

    let random = new Random()

    let KeyPush key =
        asd.Engine.Keyboard.GetKeyState key = asd.KeyState.Push
    let KeyHold key =
        asd.Engine.Keyboard.GetKeyState key = asd.KeyState.Hold
    let KeyRelease key =
        asd.Engine.Keyboard.GetKeyState key = asd.KeyState.Release
    let KeyFree key =
        asd.Engine.Keyboard.GetKeyState key = asd.KeyState.Free

    let MouseLeftPushed () =
        asd.Engine.Mouse.GetButtonInputState asd.MouseButtons.ButtonLeft = asd.MouseButtonState.Push
    let MouseLeftHold () =
        asd.Engine.Mouse.GetButtonInputState asd.MouseButtons.ButtonLeft = asd.MouseButtonState.Hold
    let MouseLeftReleased () =
        asd.Engine.Mouse.GetButtonInputState asd.MouseButtons.ButtonLeft = asd.MouseButtonState.Release
    
    let HSVtoRGB(h : byte, s: byte, v: byte, a : byte) : asd.Color =
        let func x = float32(x) / 255.0f
        let h, s, v = func h, func s, float32 v

        if s > 0.0f then
            let h = 6.0f * h
            let i = int32(Math.Floor(float h))
            let f = h - (float32 i)

            let r, g, b, a = i |> function 
                |0 -> 0.0f, -s * (1.0f - f), -s, a
                |1 -> -s * f, 0.0f, -s, a
                |2 -> -s, 0.0f, -s * (1.0f - f), a
                |3 -> -s, -s * f, 0.0f, a
                |4 -> -s * (1.0f - f), -s, 0.0f, a
                |5 -> 0.0f, -s, -s * f, a
                |_ -> 0.0f, 0.0f, 0.0f, 0uy
            
            let func x = byte ((x + 1.0f) * v)
            let r, g, b = func r, func g, func b

            new asd.Color(r, g, b, a)
        else
            let v = byte v
            new asd.Color(v, v, v, a)

    let otherside_of_line p ls v1 v2 =
        let A : asd.Vector2DF = v2 - v1
        let B : asd.Vector2DF = p - ls
        let C = new asd.Vector2DF(A.Y, -A.X)
        let D = new asd.Vector2DF(B.Y, -B.X)

        let dot (a : asd.Vector2DF) (b : asd.Vector2DF) =
            a.X * b.X + a.Y * b.Y
        
        let dist = (dot (v1 - ls) C) / (dot B C)
        let range = (dot (ls - v1) D) / (dot A D)

        0.0f < dist && dist < 1.0f && 0.0f < range && range < 1.0f