using Environment;

namespace Viewport2D
{
    public interface ICellDisplay2D : ICellDisplay
    {
        void DrawBackground(object s, float x, float y, float cellSize);
        void DrawForeground(object s, float x, float y, float cellSize);
    }
}
