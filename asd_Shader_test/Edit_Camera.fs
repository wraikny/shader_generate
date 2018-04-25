namespace shader_test

open System
open Global

type Edit_Camera() =
    inherit asd.CameraObject2D()

    let mutable position = new asd.Vector2DF(0.0f, 0.0f)

    member this.Position
        with get() = position
        and set(value) = position <- value

    override this.OnUpdate() =
        this.Src <- new asd.RectI(position.To2DI(), WindowSize.To2DI())
        ()