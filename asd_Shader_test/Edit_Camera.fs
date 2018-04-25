namespace shader_test

open System
open Global

type Camera_Pos =
    val mutable position : asd.Vector2DI
    new() = {position = new asd.Vector2DI(0, 0)}

type Edit_Camera(camera_pos) =
    inherit asd.CameraObject2D()
    let cameta_pos : Camera_Pos = camera_pos

    override this.OnAdded() =
        this.Dst <- new asd.RectI(new asd.Vector2DI(0, 0), WindowSize.To2DI())
    override this.OnUpdate() =
        this.Src <- new asd.RectI(camera_pos.position, WindowSize.To2DI())
        ()