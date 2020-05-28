using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace Environment
{
    public class Terraformer : ITerraformer
    {
        private Random rand = new Random();
        public World World { get; set; }
        public Terraformer(World w)
        {
            World = w;
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
            int Area = World.Width * World.Height;
            int budget = (int)(Area * Coverage);
            int ymax = World.LatitudeToY(maxLatitude);
            int ymin = World.LatitudeToY(-maxLatitude);
            while (budget > 0)
            {
                int x = rand.Next(0, World.Width);
                int y = rand.Next(0, World.Height);
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
            int cover = (int)(MathF.PI * radius * radius * CoverPct);
            int ret = cover;
            while (cover-- > 0)
            {
                int candX = (int)(x + rand.GetNormal() * radius / 6);
                int candY = (int)(y + rand.GetNormal() * radius / 6);
                candX = (candX + World.Width) % World.Width;
                candY = (candY + World.Height) % World.Height;
                if (World.Cells[candX, candY].Terrain == null || World.Cells[candX, candY].Terrain.Kind != TerrainKind.Ocean)
                {
                    World.Cells[candX, candY].Terrain = new Terrain(kind);
                }
            }
            return ret;
        }

        public void Terraform()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            // Oceans
            CoverWithTerrain(World.Width * 15 / 40, 2 / 3.0, .8, TerrainKind.Ocean, Angle.FromDegrees(90));
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
            int budget = (int)(World.Width * Coverage);
            int y0 = World.LatitudeToY(latitude);
            while (budget-- > 0)
            {
                int x = rand.Next(0, World.Width);
                int y = (int)(rand.GetNormal() * Sigma + y0);
                y = (y + 16 * World.Height) % World.Height;
                if (onWater || World.Cells[x, y].Terrain.Kind != TerrainKind.Ocean)
                {
                    World.Cells[x, y].Terrain = new Terrain(kind);
                    // Util.Debug($"Set {kind} at latitude {cells[x, y].Lat.Degrees}°");
                }
            }
        }
    }
}
