using Environment;
using Microsoft.Graphics.Canvas;
using System;
using System.Diagnostics;
using System.IO;
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


        public float X
        {
            get => viewportStart.X;
            private set
            {
                if (viewportStart.X != value)
                {
                    lastBackgroundRenderTarget = null;
                }
                viewportStart.X = value;
            }
        }
        public float Y
        {
            get => viewportStart.Y; private set
            {
                if (viewportStart.Y != value)
                {
                    lastBackgroundRenderTarget = null;
                }
                viewportStart.Y = value;
            }
        }

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
            var x0 = X / CellSize;
            var y0 = Y / CellSize;

            float xf = (X + w + CellSize - 1) / CellSize;
            float yf = (Y + h + CellSize - 1) / CellSize;

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

        public bool UseBlitting { get; set; }
        public bool UseDiffing { get => useDiffing; set { useDiffing = value; lastBackgroundRenderTarget = null; } }
        public object Canvas { get; set; }

        private async Task _DumpCanvasRenderTarget(CanvasRenderTarget source, string fname)
        {
            using (var stream = await KnownFolders.PicturesLibrary.OpenStreamForWriteAsync(fname + ".png", CreationCollisionOption.FailIfExists))
            {
                await source.SaveAsync(stream.AsRandomAccessStream(), CanvasBitmapFileFormat.Png);
            }
        }

        public static void DumpCanvasRenderTarget(CanvasRenderTarget source, string filename)
        {
            if (source != null)
            {
                /// DEBUG ONLY
                /// source.SaveAsync(ApplicationData.Current.LocalFolder.Path + "\\" + frameNumber + "_" + filename + ".png", CanvasBitmapFileFormat.Png).AsTask().Wait();
            }
        }

        private int frameNumber = 0;
        public DiffingStats DiffingStats { get; set; }
        public void Draw(object arg)
        {
            frameNumber++;
            DiffingStats = new DiffingStats();
            /// Draw works in one of two modes:
            /// In blitting mode, we create a bitmap where we draw one copy of the world,
            /// then we blit that bitmap over and over inside the canvas.
            /// 
            /// In non-blitting mode, we just draw once, repeating each cell until we run out of canvas.
            /// However we're going to be calling fill/draw for cells we have already rendered.

            var session = arg as CanvasDrawingSession;
            CanvasRenderTarget blittingTarget = null;
            CanvasDrawingSession blittingSession = null;
            if (UseBlitting && (Width > ScaledMapWidth || Height > ScaledMapWidth))
            {
                blittingTarget = new CanvasRenderTarget(Canvas as ICanvasResourceCreatorWithDpi, Width, Height);
                blittingSession = blittingTarget.CreateDrawingSession();
            }

            DrawForegroundAndBackground(blittingSession ?? session);

            session = arg as CanvasDrawingSession;
            if (blittingTarget != null)
            {
                // We can save a lot of draw cycles by just tile-blitting a world bitmap
                Clear(session); // now we clear the whole screen
                for (float y = 0; y < Height; y += ScaledMapWidth)
                {
                    for (float x = 0; x < Width; x += ScaledMapWidth)
                    {
                        session.DrawImage(blittingTarget, x, y);
                    }
                }
            }

            /// DumpCanvasRenderTarget(currentRenderTarget, "6-blit_element");
            if (blittingSession != null)
            {
                blittingSession.Flush();
                blittingSession.Dispose();
                blittingSession = null;
            }
        }


        private CanvasRenderTarget lastBackgroundRenderTarget = null;
        private CanvasRenderTarget currentRenderTarget = null;
        public bool IsDiffingCachePresent { get => lastBackgroundRenderTarget != null; }

        private void DrawForegroundAndBackground(CanvasDrawingSession session)
        {
            float w = UseBlitting ? Math.Min(ScaledMapWidth, Width) : Width;
            float h = UseBlitting ? Math.Min(ScaledMapWidth, Height) : Height;

            Clear(session);
            if (UseDiffing && Canvas != null)
            {
                currentRenderTarget = new CanvasRenderTarget(Canvas as ICanvasResourceCreatorWithDpi, w, h);
                using (var backgroundSession = currentRenderTarget.CreateDrawingSession())
                {
                    Clear(backgroundSession);
                    DrawCachedBackground(backgroundSession);
                    DrawBackgroundUpdates(backgroundSession, w, h);
                    backgroundSession.Flush();
                }
                session.DrawImage(currentRenderTarget);
                lastBackgroundRenderTarget = currentRenderTarget;
            }
            else
            {
                DrawBackgroundUpdates(session, w, h);
            }

            DrawForeground(session, w, h);
        }

        private void DrawForeground(CanvasDrawingSession session, float w, float h)
        {
            DrawProc((cellDisplay, renderX, renderY) =>
                {
                    cellDisplay.DrawForeground(session, renderX, renderY, CellSize);
                }, w, h);
        }

        private void DrawBackgroundUpdates(CanvasDrawingSession session, float w, float h)
        {
            DrawProc((cellDisplay, renderX, renderY) =>
            {
                cellDisplay.DrawBackground(session, renderX, renderY, CellSize);
            }, w, h);

            session?.Flush();
            /// DumpCanvasRenderTarget(currentRenderTarget, "3-bkgupdate");
        }

        private void DrawCachedBackground(CanvasDrawingSession session)
        {
            if (lastBackgroundRenderTarget != null)
            {
                session.DrawImage(lastBackgroundRenderTarget);
            }
        }

        private Vector2 viewportStart = new Vector2();
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
                    Y += displacementSpeed; break;
                case DisplacementDirection.Up:
                    Y -= displacementSpeed; break;
                case DisplacementDirection.Left:
                    X -= displacementSpeed; break;
                case DisplacementDirection.Right:
                    X += displacementSpeed; break;
            }

            X = (X + ScaledMapWidth) % ScaledMapWidth;
            Y = (Y + ScaledMapWidth) % ScaledMapWidth;
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
        private bool useDiffing;

        private float GetSpeed()
        {
            displacementTimer.Stop();
            long ms = displacementTimer.ElapsedMilliseconds;
            displacementTimer.Start();
            return CellSize * (initialDisplacementSpeed + (maxDisplacementSpeed - initialDisplacementSpeed) * Easing.GetValue((float)ms / MaxTime));
        }

        public Cell GetCellAtPoint(Point pt)
        {
            var x = (int)((pt.X + X + .5f) / CellSize);
            var y = (int)((pt.Y + Y + .5f) / CellSize);
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
                (effectiveX * CellSize - X + Width) % Width,
                (effectiveY * CellSize - Y + Height) % Height
                );
        }
    }
}
