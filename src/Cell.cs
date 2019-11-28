using System;
using System.Windows.Controls;

namespace SimEarth2020
{
    public partial class MainWindow
    {
        public class Cell : Button
        {
            public Cell(World world, int x, int y)
            {
                this.world = world;
                X = x;
                Y = y;
            }
            public bool IsOcean { get; set; }
            public Terrain Terrain { get; set; }
            public Animal Animal { get; set; }
            public TechTool TechTool { get; set; }
            public long Elevation { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            private World world;
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
                double x = (X +.5 - world.Width / 2) * (circumference / world.Width);
                double y = (world.Height / 2 - Y - .5) * (circumference / 2 / world.Height);
                double z = Elevation + world.Radius;

                return new double[] { z, y / z, x / z };
            }

            internal void DoClick()
            {
                Util.FindParent<MainWindow>(this).Click(this);
            }

            public double Temperature { get; set; }
            public Angle WindDirection { get; set; }
        }
    }
}
