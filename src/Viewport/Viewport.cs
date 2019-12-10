using Environment;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
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
        private const float cellSize = 20;
        public float ScaledMapWidth => CellSize * N;
        DisplacementDirection lastDisplacementDirection = DisplacementDirection.None;
        private const float maxDisplacementSpeed = 30;
        private const float initialDisplacementSpeed = 1f;
        private Stopwatch displacementTimer = new Stopwatch();
        public float CellSize => (RenderScale * cellSize);


        public float X { get => viewportStart.X; }
        public float Y { get => viewportStart.Y; }

        public float Width { get; set; }
        public float Height { get; set; }
        public void Draw(CanvasAnimatedDrawEventArgs args)
        {
            args.DrawingSession.Clear(Colors.CornflowerBlue);
            args.DrawingSession.Antialiasing = CanvasAntialiasing.Aliased;

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
                    var cellDisplay = world.Cells[xindex, yindex].Display as ICellDisplay2D;
                    cellDisplay.Draw(args.DrawingSession, renderX, renderY, CellSize);
                }
            }

            args.DrawingSession.FillRectangle(new Rect(0, 0, 100, 20), Colors.BlanchedAlmond);
            args.DrawingSession.DrawText($"{viewportStart}", new Vector2(0, 0), Colors.Black, format);
        }

        private CanvasTextFormat format = new CanvasTextFormat() { FontSize = 8 };

        public void StopScrolling()
        {
            lastDisplacementDirection = DisplacementDirection.None;
            displacementTimer = null;
        }

        Vector2 viewportStart = new Vector2();
        public void Scroll(DisplacementDirection dir)
        {
            float displacementSpeed;
            if (lastDisplacementDirection == dir)
            {
                displacementSpeed = GetSpeed();
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

        private IEasing Easing = new LinearEasing();
        private float GetSpeed()
        {
            displacementTimer.Stop();
            long ms = displacementTimer.ElapsedMilliseconds;
            displacementTimer.Start();
            const float MaxTime = 600;
            return initialDisplacementSpeed + (maxDisplacementSpeed - initialDisplacementSpeed) * Easing.GetValue((float)ms / MaxTime);
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
    }

}
