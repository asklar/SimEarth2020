using System.Runtime.CompilerServices;
using Windows.Foundation;

[assembly: InternalsVisibleTo("Tests")]
namespace Environment
{
    public class DiffingStats
    {
        public int TotalCells { get; set; }
        public int ReusedCells { get; set; }
    }


    public interface IViewport : IDrawable
    {
        float RenderScale { get; set; }
        float Height { get; set; }
        float Width { get; set; }
        float CellSize { get; }
        bool EasingIsPositive { get; set; }
        object Canvas { get; set; }
        bool UseBlitting { get; set; }
        bool UseDiffing { get; set; }
        bool IsDiffingCachePresent { get; }
        DiffingStats DiffingStats { get; set; }
        float X { get; }
        float Y { get; }

        void Scroll(DisplacementDirection dir);
        Cell GetCellAtPoint(Point pt);
        void Clear(object arg);
        Point CellIndexToScreenCoords(float effectiveX, float effectiveY);
    }
}
