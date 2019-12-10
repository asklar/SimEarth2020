using Environment;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Viewport2D;
using Windows.Foundation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimEarth2020
{

    public class CellDisplay2D : CellDisplay, ICellDisplay2D
    {
        public CellDisplay2D(Cell cell) : base(cell) { }

        public void Draw(CanvasDrawingSession s, float x, float y, float cellSize)
        {
            s.FillRectangle(new Rect(x * cellSize, y * cellSize, cellSize, cellSize),
                Background);

            if (cell.Animal != null)
            {
                s.DrawText($"{Text}", x * cellSize, y * cellSize, Foreground, format);
            }
            // DEBUG
            // s.DrawText($"{cell.X}", x * cellSize, y * cellSize, Foreground, format);

        }
        static CanvasTextFormat format = new CanvasTextFormat() { FontSize = 6 };
    }
}
