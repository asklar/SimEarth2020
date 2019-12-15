using Environment;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using Viewport2D;
using Windows.Foundation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimEarth2020
{

    public class CellDisplay2D : CellDisplay, ICellDisplay2D
    {
        public CellDisplay2D(Cell cell) : base(cell) { }

        public void DrawBackground(object arg, float x, float y, float cellSize)
        {
            var s = arg as CanvasDrawingSession;
            s.FillRectangle(new Rect(x * cellSize, y * cellSize, cellSize, cellSize),
                Background);
        }

        public void DrawForeground(object arg, float x, float y, float cellSize)
        {
            var s = arg as CanvasDrawingSession;
            if (cell.Animal != null)
            {
                Environment.Util.Debug($"Draw cell: {cell.X},{cell.Y}   rel loc: {cell.Animal.Location}");
                if (Math.Abs(cell.Animal.Location.X) > 1)
                {
                    // Something went wrong
                }
                float rx = (x + (float)cell.Animal.Location.X) * cellSize;
                float ry = (y + (float)cell.Animal.Location.Y) * cellSize;
                CanvasBitmap b = BitmapManager.Animals[(int)cell.Animal.Kind];
                if (b != null)
                {
                    s.DrawImage(b, new Rect(rx, ry, cellSize, cellSize));
                }
                else
                {
                    s.DrawText($"{Text}", rx, ry, Foreground, format);
                }
            }
            // DEBUG
            // s.DrawText($"{cell.X}", x * cellSize, y * cellSize, Foreground, format);

        }
        static CanvasTextFormat format = new CanvasTextFormat() { FontSize = 6 };
    }
}
