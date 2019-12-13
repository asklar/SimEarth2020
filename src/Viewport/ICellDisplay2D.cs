using Environment;

namespace Viewport2D
{
    public interface ICellDisplay2D : ICellDisplay
    {
        void Draw(object s, float x, float y, float cellSize);
    }
}
