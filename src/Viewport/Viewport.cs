using Environment;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Diagnostics;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Viewport2D
{
    public partial class Viewport : IViewport
    {
        private World world;
        public Viewport(World world) { this.world = world; }
        public float RenderScale { get; set; } = 1;
        private int N { get => world.Size; }
        public const float CellSize_0 = 20;
        public float ScaledMapWidth => CellSize * N;
        DisplacementDirection lastDisplacementDirection = DisplacementDirection.None;
        private const float maxDisplacementSpeed = 20f;
        private const float initialDisplacementSpeed = 2f;
        private const float MaxTime = 900;
        private Stopwatch displacementTimer = new Stopwatch();
        public float CellSize => (RenderScale * CellSize_0);


        public float X { get => viewportStart.X; }
        public float Y { get => viewportStart.Y; }

        public float Width { get; set; }
        public float Height { get; set; }

        public void Clear(object arg)
        {
            var session = arg as CanvasDrawingSession;
            if (session != null)
            {
                session.Clear(Colors.CornflowerBlue);
                session.Antialiasing = CanvasAntialiasing.Aliased;
            }
        }

        private void DrawProc(Action<ICellDisplay2D, float, float> action)
        {
            var x0 = viewportStart.X / CellSize;
            var y0 = viewportStart.Y / CellSize;

            float xf = (viewportStart.X + Width + CellSize - 1) / CellSize;
            float yf = (viewportStart.Y + Height + CellSize - 1) / CellSize;

            for (int y = (int)y0 - 1; y <= yf; y++)
            {
                for (int x = (int)x0 - 1; x <= xf; x++)
                {
                    var renderX = x - x0;
                    var renderY = y - y0;
                    // When scrolling fast things can get out of range quickly, so correct for 16x
                    int xindex = (x + 16 * N) % N;
                    int yindex = (y + 16 * N) % N;
                    var cellDisplay = (ICellDisplay2D)(world.Cells[xindex, yindex].Display);
                    action(cellDisplay, renderX, renderY);
                }
            }

        }
        public void Draw(object arg)
        {
            Clear(arg);
            DrawProc((cellDisplay, renderX, renderY) =>
            {
                cellDisplay.DrawBackground(arg, renderX, renderY, CellSize);
            });

            DrawProc((cellDisplay, renderX, renderY) =>
            {
                cellDisplay.DrawForeground(arg, renderX, renderY, CellSize);
            });

            var session = arg as CanvasDrawingSession;
            if (session != null)
            {
                session.FillRectangle(new Rect(0, 0, 100, 20), Colors.BlanchedAlmond);
                session.DrawText($"{viewportStart}", new Vector2(0, 0), Colors.Black, format);
            }
        }

        private CanvasTextFormat format = new CanvasTextFormat() { FontSize = 8 };

        Vector2 viewportStart = new Vector2();
        public void Scroll(DisplacementDirection dir)
        {
            float displacementSpeed;
            if (lastDisplacementDirection == dir)
            {
                displacementSpeed = GetSpeed() * (EasingIsPositive ? 1 : -.1f);
            }
            else
            {
                lastDisplacementDirection = dir;
                displacementTimer = Stopwatch.StartNew();
                displacementSpeed = GetSpeed();
            }

            switch (dir)
            {
                case DisplacementDirection.Down:
                    viewportStart.Y += displacementSpeed; break;
                case DisplacementDirection.Up:
                    viewportStart.Y -= displacementSpeed; break;
                case DisplacementDirection.Left:
                    viewportStart.X -= displacementSpeed; break;
                case DisplacementDirection.Right:
                    viewportStart.X += displacementSpeed; break;
            }

            viewportStart.X = (viewportStart.X + ScaledMapWidth) % ScaledMapWidth;
            viewportStart.Y = (viewportStart.Y + ScaledMapWidth) % ScaledMapWidth;
        }

        public bool EasingIsPositive
        {
            get => easingIsPositive;
            set
            {
                easingIsPositive = value; displacementTimer = Stopwatch.StartNew();
            }
        }
        private IEasing Easing = new CosEasing();
        private bool easingIsPositive = true;

        private float GetSpeed()
        {
            displacementTimer.Stop();
            long ms = displacementTimer.ElapsedMilliseconds;
            displacementTimer.Start();
            return CellSize * (initialDisplacementSpeed + (maxDisplacementSpeed - initialDisplacementSpeed) * Easing.GetValue((float)ms / MaxTime));
        }

        public Cell GetCellAtPoint(Point pt)
        {
            var x = (int)((pt.X + viewportStart.X + .5f) / CellSize);
            var y = (int)((pt.Y + viewportStart.Y + .5f) / CellSize);
            x = (x + N) % N;
            y = (y + N) % N;
            return world.Cells[x, y];
        }

        public void DbgTerrainAround(int x0, int y0, int radius = 2)
        {
            for (int y = y0 - radius; y <= y0 + radius; y++)
            {
                for (int x = x0 - radius; x <= x0 + radius; x++)
                {
                    int ix = (x + N) % N;
                    int iy = (y + N) % N;
                    Debug.WriteLine($"{ix},{iy}    {world.Cells[ix, iy].Terrain.Kind.ToString()}");
                }
            }
        }

        public Point CellIndexToScreenCoords(float effectiveX, float effectiveY)
        {
            return new Point(
                (effectiveX * CellSize - viewportStart.X + Width) % Width,
                (effectiveY * CellSize - viewportStart.Y + Height) % Height
                );
        }
    }
}
