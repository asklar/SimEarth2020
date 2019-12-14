using System.Runtime.CompilerServices;
using Windows.Foundation;

[assembly: InternalsVisibleTo("Tests")]
namespace Environment
{
    public interface IViewport : IDrawable
    {
        float RenderScale { get; set; }
        float Height { get; set; }
        float Width { get; set; }
        float CellSize { get; }
        bool EasingIsPositive { get; set; }

        void Scroll(DisplacementDirection dir);
        Cell GetCellAtPoint(Point pt);
        void Clear(object arg);
        Point CellIndexToScreenCoords(float effectiveX, float effectiveY);
    }
}
