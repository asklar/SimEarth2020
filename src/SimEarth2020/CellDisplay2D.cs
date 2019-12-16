using Environment;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Numerics;
using Viewport2D;
using Windows.Foundation;
using Windows.UI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimEarth2020
{

    public class CellDisplay2D : CellDisplay, ICellDisplay2D
    {
        public CellDisplay2D(Cell cell) : base(cell) { }

        class CacheParams : IEquatable<CacheParams>
        {
            public CacheParams(IViewport viewport)
            {
                this.viewport = viewport;
            }
            public float X;
            public float Y;
            public float CellSize;
            public Color Background;
            public AnimalPack AnimalPack;
            private IViewport viewport;
            public bool Equals(CacheParams other)
            {
                return other != null && viewport == other.viewport &&
                    viewport.UseDiffing && viewport.IsDiffingCachePresent &&
                    (X == other.X &&
                    Y == other.Y && 
                    CellSize == other.CellSize && 
                    Background == other.Background &&
                    AnimalPack == other.AnimalPack);
            }
        }
        CacheParams cache;
        public void DrawBackground(object arg, float x, float y, float cellSize)
        {
            CacheParams newParams = new CacheParams(cell.World.Viewport) { X = x, Y = y, CellSize = cellSize, Background = Background, AnimalPack = cell.Animal };
            if (!newParams.Equals(cache))
            {
                var s = arg as CanvasDrawingSession;
                s.FillRectangle(new Rect(x * cellSize, y * cellSize, cellSize, cellSize),
                    Background);
            }
            cache = newParams;
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
