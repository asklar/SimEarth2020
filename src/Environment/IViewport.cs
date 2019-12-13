using System.Runtime.CompilerServices;
using Windows.Foundation;

[assembly: InternalsVisibleTo("Tests")]
namespace Environment
{
    public interface IViewport
    {
        float RenderScale { get; set; }
        float Height { get; set; }
        float Width { get; set; }

        void Scroll(DisplacementDirection dir);
        void StopScrolling();
        Cell GetCellAtPoint(Point pt);
        void Draw(object arg);
    }
}
