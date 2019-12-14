using System;
using Windows.Foundation;

namespace Environment
{
    public sealed class Cell
    {
        public ICellDisplay Display { get; set; }
        public Cell(World world, int x, int y)
        {
            this.World = world;
            X = x;
            Y = y;
            Display = world.Controller.GetCellDisplay(this);
        }

        public Terrain Terrain
        {
            get => terrain;
            set
            {
                terrain = value;
                Display.UpdateTerrain();
            }
        }

        public string LatLongString => $"{Math.Abs(Lat.Degrees):N0}° {(Lat.Degrees > 0 ? 'N' : 'S')}, {Math.Abs(Long.Degrees):N0}° {(Long.Degrees > 0 ? 'E' : 'W')}";

        public Temperature Temperature
        {
            get => Terrain.GetTemperature(CosLatitude);
        }

        public AnimalPack Animal
        {
            get => animal;
            set
            {
                animal = value;
                Display.UpdateAnimal();
            }
        }
        public TechTool TechTool { get; set; }
        public long Elevation { get; set; }
        public int X { get; set; }
        private int y;
        public int Y
        {
            get => y;
            set
            {
                y = value; 
                cached_CosLatitude = null; 
                cached_latitude = null;
            }
        }
        private World world;
        private AnimalPack animal;
        private Terrain terrain;

        private float? cached_CosLatitude = null;
        private Angle? cached_latitude = null;
        private Angle? cached_longitude = null;
        public Angle Lat
        {
            get
            {
                if (cached_latitude == null)
                {
                    cached_latitude = new Angle(ToSpherical()[1]);
                }
                return cached_latitude.Value;
            }
        }
        public Angle Long
        {
            get
            {
                if (cached_longitude == null)
                {
                    cached_longitude = new Angle(ToSpherical()[2]);
                }
                return cached_longitude.Value;
            }
        }

        public float CosLatitude
        {
            get
            {
                if (cached_CosLatitude == null)
                {
                    cached_CosLatitude = (float)Math.Cos(Lat.Radians);
                }
                return cached_CosLatitude.Value;
            }
        }

        private double[] ToSpherical()
        {
            double circumference = 2 * Math.PI * World.Radius;
            double x = (X + .5 - World.Width / 2.0) * (circumference / World.Width);
            double y = (World.Height / 2.0 - Y - .5) * (circumference / 2 / World.Height);
            double z = Elevation + World.Radius;

            return new double[] { z, y / z, x / z };
        }

        internal (int, int) GetMoveCandidate(long tick)
        {
            if (Animal != null && Animal.LastTick < tick)
            {
                var stats = Animal.Stats;
                int x = X;
                int y = Y;
                int r = World.Random.Next(10);
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

                x = (x + World.Width) % World.Width;
                y = (y + World.Height) % World.Height;
                // TODO: The animal wants to move, figure out how to get the destination cell and see if we can swap it
                return (x, y);
            }
            return (X, Y);
        }

        public Angle WindDirection { get; set; }
        public World World { get => world; set => world = value; }

        public void TickAnimal()
        {
            if (Animal == null)
                return;

            // 0) Census
            World.CurrentCensus.AddAnimal(Animal);

            // 1) Eat
            TickAnimal_Eat();
            if (Animal == null) return;

            // 2) Reproduce
            TickAnimal_Mate();

            // 3) Die
            TickAnimal_Die();
            if (Animal == null) return;

            // 4) Move
            TickAnimal_Move();
        }

        private static readonly Point ZeroPoint = new Point(0, 0);
        private void TickAnimal_Move()
        {
            if (Animal.Location != ZeroPoint)
            {
                if (World.Controller.MicroMoveEnabled)
                {
                    // we are already moving
                    if (Animal.Location.X.Squared() + Animal.Location.Y.Squared() < .02f)
                    {
                        Animal.Location = ZeroPoint;
                        Util.Debug("animal arrived");
                    }
                    else
                    {
                        Animal.Location.X *= (1 - Animal.Stats.Speed);
                        Animal.Location.Y *= (1 - Animal.Stats.Speed);
                        if (Math.Abs(Animal.Location.X) > 1)
                        {
                            // Something went wrong
                        }
                        Util.Debug($"animal micromove to {Animal.Location}");
                    }
                }
            }
            else
            {
                var (x, y) = GetMoveCandidate(World.CurrentTick);
                if (World.Cells[x, y].Animal != null)
                {
                    // already occupied, don't move for now
                    // TODO: Implement animal packs eating each other
                }
                else
                {
                    if (World.Cells[x, y].Terrain.Kind == TerrainKind.Ocean && !Animal.Stats.CanSwim)
                        return;

                    if (World.Cells[x, y].Terrain.Kind != TerrainKind.Ocean && !Animal.Stats.CanWalk)
                        return;

                    if (World.Controller.MicroMoveEnabled)
                    {
                        var dx = X - x;
                        var dy = Y - y;
                        if (dx > 1)
                        {
                            // This happens when the animal is crossing the zero longitude line
                            if (dx == World.Size - 1)
                            {
                                dx = -1;
                            }
                            else
                            {
                                // Something went wrong
                            }
                        }
                        else if (dx < -1)
                        {
                            if (dx == 1 - World.Size)
                            {
                                dx = 1;
                            }
                            else
                            {
                                // Something went wrong
                            }
                        }
                        if (dy > 1)
                        {
                            if (dy == World.Size - 1)
                            {
                                dy = -1;
                            }
                            else
                            {
                                // Something went wrong;
                            }
                        }
                        else if (dy < -1)
                        {
                            if (dy == 1 - World.Size)
                            {
                                dy = 1;
                            }
                            else
                            {
                                // Something went wrong;
                            }
                        }
                        if (Math.Abs(dx) > 1 || Math.Abs(dy) > 1)
                        {
                            // Something went wrong
                        }
                        Animal.Location = new Point(dx, dy);
                        Util.Debug($"start animal move {X},{Y} -> {x},{y} Rel loc: {Animal.Location}");
                    }
                    // Debug.WriteLine($"Moving {Animal.Kind}(LT {Animal.LastTick}) from ({X}, {Y}) to ({x}, {y})");
                    World.Cells[x, y].Animal = Animal;
                    World.Cells[x, y].Animal.LastTick = World.CurrentTick;
                    Animal = null;
                }
            }
        }

        private void TickAnimal_Mate()
        {
            const double Sigma = .25;
            double litterSize = Math.Max(0, World.Random.GetNormal() * Sigma + Animal.Stats.AverageLitterSize);

            var births = (Animal.Population / 2) * litterSize * Animal.Stats.PregnancyProbability;
            Animal.TotalHP += (int)(births * Animal.Stats.MaxHP);
        }

        private void TickAnimal_Die()
        {
            if (Animal.Population <= 0)
            {
                World.Controller.SetStatus($"{Animal.Kind} ({LatLongString}) died of famine");
                Animal = null;
                return;
            }
        }

        private void TickAnimal_Eat()
        {
            int foodNeeded = (int)(Animal.Stats.FoodPerTurn * Animal.Population);
            var foodSources = Animal.Stats.FoodSources;
            double shortage = foodNeeded / Animal.Population;
            if (foodSources.Sun && Math.Abs(Lat.Degrees) < 60)
            {
                return;
            }
            Animal.TotalHP -= (int)shortage;
            if (Animal.Population <= 0)
            {
                Animal = null;
                return;
            }
            shortage = 0;
            if (foodSources.Vegetation && Terrain.RemainingFood > 0)
            {
                int foodAvailable = Math.Min(foodNeeded, Terrain.RemainingFood);
                Terrain.RemainingFood -= foodAvailable;
                Animal.TotalHP += foodAvailable / Animal.Population;
                shortage = Math.Max(0, (foodNeeded - foodAvailable) / Animal.Population);
            }
        }

        public void TickTerrain()
        {
            TerrainKind old = Terrain.Kind;
            Terrain.Tick(CosLatitude);
            if (old != Terrain.Kind)
            {
                // The terrain has changed, update its display
                Display.UpdateTerrain();
            }
        }
    }
}