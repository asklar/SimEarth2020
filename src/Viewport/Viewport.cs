using Environment;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
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

        private void DrawProc(Action<ICellDisplay2D, float, float> action, float w, float h)
        {
            var x0 = viewportStart.X / CellSize;
            var y0 = viewportStart.Y / CellSize;

            float xf = (viewportStart.X + w + CellSize - 1) / CellSize;
            float yf = (viewportStart.Y + h + CellSize - 1) / CellSize;

            for (var y = y0; y < yf; y++)
            {
                for (var x = x0; x < xf; x++)
                {
                    var renderX = x - x0;
                    var renderY = y - y0;
                    // When scrolling fast things can get out of range quickly, so correct for 16x
                    int xindex = ((int)x + 16 * N) % N;
                    int yindex = ((int)y + 16 * N) % N;
                    var cellDisplay = (ICellDisplay2D)(world.Cells[xindex, yindex].Display);
                    action(cellDisplay, renderX, renderY);
                }
            }

        }

        public bool UseBlitting { get; set; } = true;
        public object Canvas { get; set; }
        public void Draw(object arg)
        {
            /// Draw works in one of two modes:
            /// In blitting mode, we create a bitmap where we draw one copy of the world,
            /// then we blit that bitmap over and over inside the canvas.
            /// 
            /// In non-blitting mode, we just draw once, repeating each cell until we run out of canvas.
            /// However we're going to be calling fill/draw for cells we have already rendered.

            var session = arg as CanvasDrawingSession;
            CanvasRenderTarget renderTarget = null;

            if (UseBlitting && (Width > ScaledMapWidth || Height > ScaledMapWidth))
            {
                renderTarget = new CanvasRenderTarget(Canvas as ICanvasResourceCreatorWithDpi, ScaledMapWidth, ScaledMapWidth);
                session = renderTarget.CreateDrawingSession();
            }

            Clear(session);

            DrawForegroundAndBackground(session);

            session = arg as CanvasDrawingSession;
            if (renderTarget != null)
            {
                // We can save a lot of draw cycles by just tile-blitting a world bitmap
                Clear(session); // now we clear the whole screen
                for (float y = 0; y < Height; y += ScaledMapWidth)
                {
                    Debug.WriteLine($"blt: {y}");
                    for (float x = 0; x < Width; x += ScaledMapWidth)
                    {
                        session.DrawImage(renderTarget, x, y);
                    }
                }
            }

            if (session != null)
            {
                session.FillRectangle(new Rect(0, 0, 100, 20), Colors.BlanchedAlmond);
                session.DrawText($"{viewportStart}", new Vector2(0, 0), Colors.Black, format);
            }

        }

        private async Task Save(CanvasRenderTarget renderTarget)
        {
            var file = await KnownFolders.SavedPictures.CreateFileAsync("1.png", CreationCollisionOption.ReplaceExisting);
            {
                using (var fs = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await renderTarget.SaveAsync(file.Path, CanvasBitmapFileFormat.Png, 1f);
                }
            }
        }

        private void DrawForegroundAndBackground(CanvasDrawingSession session)
        {
            float w = UseBlitting ? Math.Min(ScaledMapWidth, Width) : Width;
            float h = UseBlitting ? Math.Min(ScaledMapWidth, Height) : Height;
            DrawProc((cellDisplay, renderX, renderY) =>
            {
                cellDisplay.DrawBackground(session, renderX, renderY, CellSize);
            }, w, h);

            DrawProc((cellDisplay, renderX, renderY) =>
            {
                cellDisplay.DrawForeground(session, renderX, renderY, CellSize);
            }, w, h);
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
