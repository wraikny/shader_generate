namespace shader_test

open Global

type Button_Action =
    AddRect

type Button(size, pos, col, text, action) =
    inherit asd.GeometryObject2D()

    let size = size
    let col = col
    let text = text
    let action : Button_Action = action

    override this.OnAdded() =
        this.Position <- pos
        this.Shape <-
            let da = new asd.RectF(-size/2.0f, size)
            new asd.RectangleShape(DrawingArea = da)
        this.Color <- col

        let textobj = new asd.TextObject2D()
