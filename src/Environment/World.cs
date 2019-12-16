using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Foundation;

[assembly: InternalsVisibleTo("Tests")]
namespace Environment
{
    public class World
    {
        internal const double RotationalFactor = 4; // 4 for rapidly rotating bodies (Earth), 2 for slowly rotating bodies.
        internal const double SolarLuminosity = 3.828e26;
        internal const double DistanceToTheSun = 1.495978707e11;
        public World(IController controller, int Size)
        {
            Controller = controller;
            this.Size = Size;
            Cells = new Cell[Size, Size];

            controller.CurrentWorld = this; // must come before Controller.CreateViewport()
            Viewport = Controller.CreateViewport();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var cell = new Cell(this, x, y);
                    cell.Terrain = new Terrain(TerrainKind.Rock);
                    Cells[x, y] = cell;
                }
            }
            IsInited = true;
        }

        public bool IsInited { get; private set; } = false;
        public IViewport Viewport { get; private set; }

        private double age;
        private int energy;

        public IController Controller { get; set; }
        public string Name { get => name; set { name = value; Controller.RaisePropertyChanged("CurrentWorld"); } }

        public int Size { get; private set; }
        public int Width { get => Size; }
        public int Height { get => Width; }

        public double Age
        {
            get => age;
            set
            {
                age = value;
                Controller.RaisePropertyChanged("CurrentWorld");
            }
        }
        public double Radius { get; set; }
        private string GetAge()
        {
            string[] prefixes = new string[] { "", "k", "M", "G", "T" };
            int index = 0;
            double age = Age;
            while (age >= 1000 && index < prefixes.Length - 1)
            {
                index++;
                age /= 1000;
            }
            return $"{age:N2} {prefixes[index]}";
        }
        public override sealed string ToString()
        {
            return $"{Name}: {GetAge()}yr";
        }
        private long tick = 0;
        public long CurrentTick { get => tick; }

        internal Queue<Census> CensusHistory = new Queue<Census>();


        private const int MaxCensusHistory = 30;
        public Census CurrentCensus { get; set; }
        public void Tick()
        {
            var start = DateTime.Now;
            Age += YearsFromSpeed(Controller.Speed);
            Energy = Math.Min(MaxEnergy, Energy + GetProducedEnergy());
            CurrentCensus = new Census();
            if (CensusHistory.Count == MaxCensusHistory)
            {
                CensusHistory.Dequeue();
            }
            foreach (var cell in Cells)
            {
                cell.TickAnimal();
                TickTerrain(cell);
            }
            CensusHistory.Enqueue(CurrentCensus);
            var duration = DateTime.Now - start;
            //Controller.SetStatus($"Tick {tick}: {duration.TotalMilliseconds} ms");
            tick++;
        }

        private double YearsFromSpeed(Speed speed)
        {
            switch (speed)
            {
                case Speed.Slow: return 1e2;
                case Speed.Medium: return 1e3;
                case Speed.Fast: return 1e4;
            }
            throw new InvalidOperationException();
        }

        private void TickTerrain(Cell cell)
        {
            cell.TickTerrain();
            if (cell.Terrain != null)
            {
                CurrentCensus.AddTerrain(cell.Terrain);
            }
        }

        private Random rand = new Random();
        private string name;

        internal Random Random { get => rand; }

        public void Terraform()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            // Oceans
            CoverWithTerrain(Width * 15 / 40, 2 / 3.0, .8, TerrainKind.Ocean, Angle.FromDegrees(90));
            // Lakes
            CoverWithTerrain(3, 2 / 3.0, .4, TerrainKind.Ocean, Angle.FromDegrees(75));
            // Tundra
            MakeLatitudeTerrain(Angle.FromDegrees(90), 2, 15, TerrainKind.Tundra, true);
            // Taiga
            MakeLatitudeTerrain(Angle.FromDegrees(55), 2, 8, TerrainKind.Taiga, false);
            MakeLatitudeTerrain(Angle.FromDegrees(-55), 2, 8, TerrainKind.Taiga, false);
            // Forests
            MakeLatitudeTerrain(Angle.FromDegrees(45), 2, 15, TerrainKind.Forest, false);
            MakeLatitudeTerrain(Angle.FromDegrees(-45), 2, 15, TerrainKind.Forest, false);
            // Jungle
            MakeLatitudeTerrain(Angle.FromDegrees(0), 2, 20, TerrainKind.Jungle, false);
            // Grasslands
            MakeLatitudeTerrain(Angle.FromDegrees(30), 7, 20, TerrainKind.Grass, false);
            MakeLatitudeTerrain(Angle.FromDegrees(-30), 7, 20, TerrainKind.Grass, false);
            // Deserts
            CoverWithTerrain(5, 4, .13, TerrainKind.Desert, Angle.FromDegrees(30));
            // Swamps
            CoverWithTerrain(2, 1.5, .3, TerrainKind.Swamp, Angle.FromDegrees(60));
            stopwatch.Stop();
            Util.Debug($"Terraform: {stopwatch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Creates terrain of the required kind around a particular latitude
        /// </summary>
        /// <param name="latitude">The Latitude to create terrain around. Note that you must call this function again with the negative Latitude value to produce somewhat symmetrical terrains</param>
        /// <param name="Sigma">Standard deviation of how far to stray from the central latitude</param>
        /// <param name="Coverage">How densely to cover with this terrain</param>
        /// <param name="kind">The kind of terrain</param>
        /// <param name="onWater">Indicates whether to put terrain over Ocean. False will not overwrite Ocean terrain.</param>
        internal void MakeLatitudeTerrain(Angle latitude, double Sigma, double Coverage, TerrainKind kind, bool onWater)
        {
            int budget = (int)(Width * Coverage);
            int y0 = LatitudeToY(latitude);
            while (budget-- > 0)
            {
                int x = rand.Next(0, Width);
                int y = (int)(rand.GetNormal() * Sigma + y0);
                y = (y + 16 * Height) % Height;
                if (onWater || Cells[x, y].Terrain.Kind != TerrainKind.Ocean)
                {
                    Cells[x, y].Terrain = new Terrain(kind);
                    // Util.Debug($"Set {kind} at latitude {cells[x, y].Lat.Degrees}°");
                }
            }
        }

        public int LatitudeToY(Angle latitude)
        {
            double y = -(2 * latitude.Radians / Math.PI) * (Height / 2) + (Height / 2);
            return (int)(y + .5);
        }

        public Angle YToLatitude(int y)
        {
            if (y < 0 || y > Height) throw new ArgumentException($"Y must be between 0 and {Height}");
            double a = (y - Height / 2.0) / (-2 * Math.PI);
            return new Angle(a);
        }


        /// <summary>
        /// Creates blotch-like terrains of the specified kind
        /// </summary>
        /// <param name="MeanRadius">Mean blotch radius</param>
        /// <param name="Sigma">Standard deviation of the blotch radius</param>
        /// <param name="Coverage">How densely to cover</param>
        /// <param name="kind">The kind of terrain to use</param>
        /// <param name="maxLatitude">The maximum absolute latitude allowed</param>
        private void CoverWithTerrain(int MeanRadius, double Sigma, double Coverage, TerrainKind kind, Angle maxLatitude)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int Area = Width * Height;
            int budget = (int)(Area * Coverage);
            int ymax = LatitudeToY(maxLatitude);
            int ymin = LatitudeToY(-maxLatitude);
            while (budget > 0)
            {
                int x = rand.Next(0, Width);
                int y = rand.Next(0, Height);
                y = Math.Clamp(y, ymax, ymin); // screen coordinates are upside down
                var radius = (MeanRadius + (rand.NextDouble() - .5) * 3 * Sigma);
                budget -= SetTerrainAround(x, y, radius, kind);
            }
            sw.Stop();
            // Util.Debug($"CoverWithTerrain {kind} {sw.ElapsedMilliseconds} ms");
        }


        /// <summary>
        /// Creates a small blotch of terrain around a particular location in the map
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        private int SetTerrainAround(int x, int y, double radius, TerrainKind kind)
        {
            // Util.Debug($"Set {kind} around ({x}, {y}) radius={radius}");
            const double CoverPct = .6;
            int cover = (int)(Math.PI * radius * radius * CoverPct);
            int ret = cover;
            while (cover-- > 0)
            {
                int candX = (int)(x + rand.GetNormal() * radius / 6);
                int candY = (int)(y + rand.GetNormal() * radius / 6);
                candX = (candX + Width) % Width;
                candY = (candY + Height) % Height;
                if (Cells[candX, candY].Terrain == null || Cells[candX, candY].Terrain.Kind != TerrainKind.Ocean)
                {
                    Cells[candX, candY].Terrain = new Terrain(kind);
                }
            }
            return ret;
        }

        public int GetProducedEnergy()
        {
            // TODO: Calculate produced energy
            return 5;
        }

        public int MaxEnergy { get; } = 5000;
        public int Energy
        {
            get => energy;
            set
            {
                energy = value;
                Controller.RaisePropertyChanged("Energy");
            }
        }

        public Cell[,] Cells { get; private set; }

        public Cell CorrectCellForAnimalMicroMovement(double px, double py)
        {
            Cell minCell = null;
            float minDist = float.PositiveInfinity;
            for (double y = py - Viewport.CellSize; y < py + Viewport.CellSize; y++)
            {
                for (double x = px - Viewport.CellSize; x < px + Viewport.CellSize; x++)
                {
                    Cell c = Viewport.GetCellAtPoint(new Point(x, y));
                    Point animalPos = c.Animal != null ? c.Animal.Location : new Point();
                    float effectiveX = (float)(c.X + .5f + animalPos.X);
                    float effectiveY = (float)(c.Y + .5f + animalPos.Y);
                    // Now convert the effective X, Y into screen coordinates
                    Point screenCoords = Viewport.CellIndexToScreenCoords(effectiveX, effectiveY);
                    float newDist = DistanceSq(px, py, screenCoords);
                    if (newDist < minDist)
                    {
                        minCell = c;
                        minDist = newDist;
                    }
                }
            }
            return minCell;
        }

        private static float DistanceSq(double px, double py, Point screenCoords)
        {
            return (float)((px - screenCoords.X).Squared() + (py - screenCoords.Y).Squared());
        }

    }
}
