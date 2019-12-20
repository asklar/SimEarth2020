using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Windows.Foundation;

[assembly: InternalsVisibleTo("Tests")]
namespace Environment
{
    public class World
    {
        internal const float RotationalFactor = 4f; // 4 for rapidly rotating bodies (Earth), 2 for slowly rotating bodies.
        internal const float SolarLuminosity = 3.828e26f;
        internal const float DistanceToTheSun = 1.495978707e11f;
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
                case Speed.Paused: return 0;
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
