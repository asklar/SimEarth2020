using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimEarth2020
{
    public class Cell : TextBlock
    {
        public Cell(World world, int x, int y)
        {
            this.world = world;
            X = x;
            Y = y;
            Background = new SolidColorBrush(Colors.Wheat);
            Foreground = new SolidColorBrush(Colors.White);
            TextAlignment = System.Windows.TextAlignment.Center;
            VerticalAlignment = System.Windows.VerticalAlignment.Center;
            Text = " ";
            Margin = new System.Windows.Thickness(0.4);
        }
        public bool IsOcean { get; set; }
        public Terrain Terrain
        {
            get => terrain;
            set
            {
                terrain = value;
                Background = GetBackground();
                Foreground = GetForeground();
            }
        }

        internal string LatLongString()
        {
            return $"{Math.Abs(Lat.Degrees):N0}∘ {(Lat.Degrees > 0 ? 'N' : 'S')}, {Math.Abs(Long.Degrees):N0}∘ {(Long.Degrees > 0 ? 'E' : 'W')}";
        }

        private Brush GetForeground()
        {
            Color bg = (Background as SolidColorBrush).Color;
            if (((double)bg.R + (double)bg.G + (double)bg.B) / 3 > (double)255/3)
            {
                return new SolidColorBrush(Colors.Black);
            }
            return new SolidColorBrush(Colors.White);
        }
        private Brush GetBackground()
        {
            Color c;
            switch (terrain.Kind)
            {
                case TerrainKind.Tundra:
                    c = Colors.White; break;
                case TerrainKind.Taiga:
                    c = Colors.Azure; break;
                case TerrainKind.Desert:
                    c = Colors.Peru; break;
                case TerrainKind.Forest:
                    c = Colors.ForestGreen; break;
                case TerrainKind.Grass:
                    c = Colors.LawnGreen; break;
                case TerrainKind.Jungle:
                    c = Colors.LimeGreen; break;
                case TerrainKind.Rock:
                    c = Colors.DimGray; break;
                case TerrainKind.Swamp:
                    c = Colors.DarkOliveGreen; break;
                case TerrainKind.Ocean:
                    c = Colors.MidnightBlue; break;
            }
            return new SolidColorBrush(c);
        }

        public AnimalPack Animal
        {
            get => animal;
            set
            {
                animal = value;
                Text = animal == null ? "" : animal.Kind.ToString()[0] + "";
            }
        }
        public TechTool TechTool { get; set; }
        public long Elevation { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        private World world;
        private AnimalPack animal;
        private Terrain terrain;

        public Angle Lat
        {
            get
            {
                return new Angle(ToSpherical()[1]);
            }
        }
        public Angle Long
        {
            get
            {
                return new Angle(ToSpherical()[2]);
            }
        }

        private double[] ToSpherical()
        {
            double circumference = 2 * Math.PI * world.Radius;
            double x = (X + .5 - world.Width / 2) * (circumference / world.Width);
            double y = (world.Height / 2 - Y - .5) * (circumference / 2 / world.Height);
            double z = Elevation + world.Radius;

            return new double[] { z, y / z, x / z };
        }

        internal void DoClick()
        {
            //Util.FindParent<MainWindow>(this).Click(this);
            world.Controller.Click(this);
        }

        internal (int, int) GetMoveCandidate(long tick, Random rand)
        {
            if (Animal != null && Animal.LastTick < tick)
            {
                var stats = Animal.Stats;
                int x = X;
                int y = Y;
                int r = rand.Next(10);
                switch (r % 3)
                {
                    case 0: x--; break;
                    case 1: break;
                    case 2: x++; break;
                }
                switch (r / 3)
                {
                    case 0: y--; break;
                    case 1: break;
                    case 2: y++; break;
                }

                x = (x + world.Width) % world.Width;
                y = (y + world.Height) % world.Height;
                // TODO: The animal wants to move, figure out how to get the destination cell and see if we can swap it
                return (x, y);
            }
            return (X, Y);
        }

        public double Temperature { get; set; }
        public Angle WindDirection { get; set; }
    }
}