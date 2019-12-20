using System;
using System.Numerics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace Environment
{
    public class HeightMap
    {
        public float[,] Map
        {
            get
            {
                if (map == null)
                {
                    CalculateHeightMap();
                }
                return map;
            }
            private set => map = value;
        }
        public Vector2[,] Winds { get; private set; }
        public float Persistence { get; set; } = 0.651f;
        public float Lacunarity { get; set; } = 0.56f;

        private int[] histogram;
        public HeightMap(World world, int layers = 5)
        {
            Layers = layers;
            this.world = world;
        }
        private World world;
        private int NumBuckets = 100;
        Random r = new Random();
        private float[,] map;

        public int Layers { get; set; } = 5;
        private void CalculateHeightMap()
        {
            int N = world.Size;
            map = new float[N, N];
            var pressure = new float[N, N];
            var winds = new Vector2[N, N];
            histogram = new int[NumBuckets];
            float p = 1f;
            for (int i = 0; i < Layers; i++)
            {
                var offsetA = (float)r.NextDouble() * 2f * MathF.PI;
                var offsetP = (float)r.NextDouble() * 2f * MathF.PI;
                p *= Persistence;

                float polar, azimut, onebyty, onebytx, halfx, halfy, sinpolar, cospolar;
                onebytx = 1.0f / N;   // onebytx/y is an optimization to prevent recurring divisions
                onebyty = 1.0f / N;
                halfx = onebytx * 0.5f;             // halfx/y is the width/height of a half pixel to calculate the midpoint of a pixel
                halfy = onebyty * 0.5f;

                for (int y = 0; y < N; y++)
                {
                    polar = (y * onebyty * 2 + halfy) * MathF.PI;     // calculate the polar angle (between positive y in unity (northpole) and the line to the point in space), required in radians 0 to pi
                    sinpolar = MathF.Sin(polar + offsetP); // cache these values as they are the same for each column in the next line
                    cospolar = MathF.Cos(polar + offsetP);
                    for (int x = 0; x < N; x++)
                    {
                        azimut = (((x * onebytx + halfx) * 2.0f) - 1.0f) * MathF.PI;      // calculate the azimut angle (between positive x axis and the line to the point in space), required in radians -pi to +pi,
                        float X = sinpolar * MathF.Cos(azimut + offsetA);
                        float Z = sinpolar * MathF.Sin(azimut + offsetA);// this is y in the wikipedia formula but because unitys axis are differerent (y and z swapped) its changed here
                        float Y = cospolar;

                        var val = SimplexNoise.Noise.CalcPixel3D(
                            (int)(X * N),
                            (int)(Y * N),
                            (int)(Z * N),
                            Lacunarity * (i + 1) / N);
                        map[x, y] += val;
                    }
                }
            }
            var info = GetMinMax(map);

            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    var prevX = x > 0 ? x - 1 : N - 1;
                    var prevY = y > 0 ? y - 1 : N - 1;
                    var df_dx = Pressure(x, y) - Pressure(prevX, y);
                    var df_dy = Pressure(x, y) - Pressure(x, prevY);
                    winds[x, y] = new Vector2(df_dx, df_dy);
                    map[x, y] = (map[x, y] - info.min) / (info.max - info.min);
                    var bucket = (int)(map[x, y] * (NumBuckets - 1));
                    histogram[bucket]++;
                }
            }
        }

        public float PercentBelowLevel(float level, float startLevel = 0f)
        {
            int levelBucket = (int)(level * (NumBuckets - 1));
            int startLevelBucket = (int)(startLevel * (NumBuckets - 1));
            var ret = 0f;
            for (int i = startLevelBucket; i <= levelBucket; i++)
            {
                ret += histogram[i];
            }
            return ret / (world.Size * world.Size);
        }

        public float GetValueForCumulativePercentage(float pct)
        {
            if (histogram == null) { CalculateHeightMap(); }
            int targetAmount = (int)(pct * world.Size * world.Size + .5f);
            int cumulative = 0;
            int currentBucket = 0;
            do
            {
                cumulative += histogram[currentBucket];
                currentBucket++;
            } while (currentBucket < histogram.Length && cumulative <= targetAmount);


            return currentBucket / (float)NumBuckets;
        }

        public class MinMaxInfo
        {
            public float min = float.MaxValue;
            public float max = float.MinValue;
            public int minX;
            public int minY;
            public int maxX;
            public int maxY;
        }
        public static MinMaxInfo GetMinMax(float[,] map)
        {
            MinMaxInfo info = new MinMaxInfo();
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x, y] < info.min)
                    {
                        info.min = map[x, y];
                        info.minX = x;
                        info.minY = y;
                    }
                    if (map[x, y] > info.max)
                    {
                        info.max = map[x, y];
                        info.maxX = x;
                        info.maxY = y;
                    }
                }
            }
            return info;
        }


        private float Pressure(int x, int y)
        {
            const float g = 9.8f;
            const float M = 0.02896968f;
            const float T0 = 288.16f;
            const float R0 = 8.314462618f;
            const float p0 = 101325f / 1000f;
            return p0 * MathF.Exp(-g * map[x, y] * M / (T0 * R0));
        }

    }
    public class HeightMapTerraformer : ITerraformer
    {
        public float WaterPct { get; set; } = 0.67f;

        public World World { get; private set; }
        public HeightMapTerraformer(World w) { World = w; }
        public void Terraform()
        {
            HeightMap heightMap = new HeightMap(World);
            var water = heightMap.GetValueForCumulativePercentage(WaterPct);
            var forest = (water + 1f) / 2f;
            for (int y = 0; y < World.Height; y++)
            {
                for (int x = 0; x < World.Width; x++)
                {
                    var val = heightMap.Map[x, y];
                    Terrain terrain = null;
                    if (val < water)
                    {
                        terrain = new Terrain(TerrainKind.Ocean);
                    }
                    else
                    {
                        terrain = new Terrain(TerrainKind.Forest);
                    }
                    terrain.Elevation = heightMap.Map[x, y] - water;
                    World.Cells[x, y].Terrain = terrain;
                }
            }
        }

    }
}
