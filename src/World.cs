using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SimEarth2020
{
    public class World
    {
        public const double RotationalFactor = 4; // 4 for rapidly rotating bodies (Earth), 2 for slowly rotating bodies.
        public const double SolarLuminosity = 3.828e26;
        public const double DistanceToTheSun = 1.495978707e11;
        public World(IController m)
        {
            Controller = m;
        }

        private UIElementCollection Cells
        {
            get
            {
                return Controller.Grid.Children;
            }
        }
        private double age;
        private int energy;

        internal IController Controller { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get => Width; }

        public double Age
        {
            get => age;
            set
            {
                age = value; Controller.RaisePropertyChanged("TitleString");
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
            return $"{age} {prefixes[index]}";
        }
        public override string ToString()
        {
            return $"{Name}: {GetAge()}yr";
        }
        private long tick = 0;
        public long CurrentTick { get => tick; }
        public Cell[,] cells;

        public Queue<Census> CensusHistory = new Queue<Census>();


        public const int MaxCensusHistory = 30;
        internal Census CurrentCensus;
        public void Tick()
        {
            var start = DateTime.Now;
            Age += Controller.Speed;
            Energy = Math.Min(MaxEnergy, Energy + GetProducedEnergy());
            CurrentCensus = new Census();
            if (CensusHistory.Count == MaxCensusHistory)
            {
                CensusHistory.Dequeue();
            }
            foreach (var cell in cells)
            {
                cell.TickAnimal();
                TickTerrain(cell);
            }
            CensusHistory.Enqueue(CurrentCensus);
            var duration = DateTime.Now - start;
            Controller.SetStatus($"Tick {tick}: {duration.TotalMilliseconds} ms");
            tick++;
        }

        private void TickTerrain(Cell cell)
        {
            cell.TickTerrain();
            if (cell.Terrain != null)
            {
                CurrentCensus.Add(cell.Terrain);
            }
        }

        private Random rand = new Random();
        public Random Random { get => rand; }
        public void Start()
        {
            ProgressBar progressBar = new ProgressBar() { IsIndeterminate = true };

            Popup popup = new Popup() { Placement = PlacementMode.Center, PlacementTarget = Controller as UIElement, Width = 200, Height = 80 };
            popup.Child = progressBar;
            popup.IsOpen = true;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            cells = new Cell[Width, Height];
            for (int i = 0; i < Width; i++)
            {
                Controller.Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(16) });
            }
            for (int i = 0; i < Height; i++)
            {
                Controller.Grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(16) });
            }
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var cell = new Cell(this, x, y);
                    Controller.AddToGrid(cell.Display);
                    cell.Terrain = new Terrain(TerrainKind.Rock);
                    cells[x, y] = cell;
                }
            }
            Terraform();
            watch.Stop();
            popup.IsOpen = false;
            Controller.SetStatus($"Created world in {watch.ElapsedMilliseconds} ms");
        }

        private void Terraform()
        {
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
        }

        private void MakeLatitudeTerrain(Angle latitude, double Sigma, double Coverage, TerrainKind kind, bool onWater)
        { 
            int budget = (int)(Width * Coverage);

            while (budget-- > 0)
            {
                int x = rand.Next(0, Width);
                int y = (int)(rand.GetNormal() * Sigma + LatitudeToY(latitude));
                y = (y + Height) % Height;
                if (onWater || cells[x, y].Terrain.Kind != TerrainKind.Ocean)
                {
                    cells[x, y].Terrain = new Terrain(kind);
                    Debug.WriteLine($"Set {kind} at latitude {cells[x,y].Lat.Degrees}°");
                }
            }
        }

        public int LatitudeToY(Angle latitude)
        {
            return (int)(Math.Sin(latitude.Radians) * Height / 2 + Height / 2);
        }

        private void CoverWithTerrain(int MeanRadius, double Sigma, double Coverage, TerrainKind kind, Angle maxLatitude)
        {
            int Area = Width * Height;
            int budget = (int)(Area * Coverage);
            int ymax = LatitudeToY(maxLatitude);
            int ymin = LatitudeToY(-maxLatitude);
            while (budget > 0)
            {
                int x = rand.Next(0, Width);
                int y = rand.Next(0, Height);
                y = Math.Clamp(y, ymin, ymax);
                var radius = (MeanRadius + (rand.NextDouble() - .5) * 3 * Sigma);
                budget -= SetTerrainAround(x, y, radius, kind);
            }
        }


        private int SetTerrainAround(int x, int y, double radius, TerrainKind kind)
        {
            Debug.WriteLine($"Set {kind} around ({x}, {y}) radius={radius}");
            const double CoverPct = .6;
            int cover = (int)(Math.PI * radius * radius * CoverPct);
            int ret = cover;
            while (cover-- > 0)
            {
                int candX = (int)(x + rand.GetNormal() * radius / 6);
                int candY = (int)(y + rand.GetNormal() * radius / 6);
                candX = (candX + Width) % Width;
                candY = (candY + Height) % Height;
                if (cells[candX, candY].Terrain == null || cells[candX, candY].Terrain.Kind != TerrainKind.Ocean)
                {
                    cells[candX, candY].Terrain = new Terrain(kind);
                }
            }
            return ret;
        }

        public int GetProducedEnergy()
        {
            // TODO: Calculate produced energy
            return 5;
        }

        public const int MaxEnergy = 5000;
        public int Energy
        {
            get => energy;
            set { energy = value; Controller.RaisePropertyChanged("Energy"); }
        }

    }
}
