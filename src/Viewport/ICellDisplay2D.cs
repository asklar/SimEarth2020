using Environment;
using Microsoft.Graphics.Canvas;

namespace Viewport2D
{
    public interface ICellDisplay2D : ICellDisplay
    {
        void Draw(CanvasDrawingSession s, float x, float y, float cellSize);
    }
}
