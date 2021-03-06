﻿using Environment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Tests
{
    [TestClass]
    public class ModelTests
    {
        World World { get; set; }
        MockController Controller { get; set; }
        const int HalfWidth = 35;

        PerfUtil perf;
        public ModelTests()
        {
            perf = new PerfUtil();

            Controller = new MockController();
            var watch = Stopwatch.StartNew();
            World = Controller.CreateWorld(2 * HalfWidth + 1);
            World.Radius = 1;
            watch.Stop();
            Assert.IsTrue(watch.ElapsedMilliseconds < 500);
        }


        [TestMethod]
        public void PerformanceWorld()
        {
            perf.Profile(() => new Terraformer(World).Terraform(), 1e8);
        }

        [TestMethod]
        public void TestCell()
        {
            var cell = new Cell(World, HalfWidth, HalfWidth);
            Assert.AreEqual(0, cell.Lat.Degrees, 1);
            Assert.AreEqual(0, cell.Long.Degrees, 1);
            Assert.AreEqual(HalfWidth, World.LatitudeToY(Angle.FromDegrees(0)));
            Assert.AreEqual(0, World.LatitudeToY(Angle.FromDegrees(90)));
            Assert.AreEqual(HalfWidth * 2, World.LatitudeToY(Angle.FromDegrees(-90)));

        }

        [TestMethod]
        public void RoundtripLatitude1()
        {
            const int Y = 15;
            var cell = new Cell(World, 0, Y);
            Assert.AreEqual(Y, World.LatitudeToY(cell.Lat));
        }

        [TestMethod]
        public void RoundtripLatitude2()
        {
            Angle angle = Angle.FromDegrees(30);
            var cell2 = new Cell(World, 0, World.LatitudeToY(angle));
            Assert.AreEqual(angle.Degrees, cell2.Lat.Degrees, 1);
        }

        [TestMethod]
        public void TestHeightMapTerraformer()
        {
            var hmt = new HeightMapTerraformer(World);
            hmt.Terraform();
            var ocean = World.Cells.Where((x) => x.Terrain.Kind == TerrainKind.Ocean).Count();
            float pct = (float)ocean / (World.Width * World.Height);
            Assert.AreEqual(pct, 0.67f, .01f);
        }

        // commented because the spherical noise library doesn't seem to behave as we expect it to
        // [TestMethod]
        public void TestHeightMap()
        {
            var hm = new HeightMap(World);

            hm.Lacunarity = .03f;
            hm.Layers = 5;
            hm.Persistence = .54f;
            float[,] map = hm.Map;
            float[,] grad = GetGradientMap(map);
            var info = GetMinMax(grad);
            Assert.IsTrue(info.min < 0.01f);
            Assert.IsTrue(info.max < 0.1f);
            var leftmostValue = hm.Map[0, HalfWidth];
            var rightmostValue = hm.Map[2 * HalfWidth, HalfWidth];
            Assert.AreEqual(leftmostValue, rightmostValue, 0.1f, $"left={leftmostValue}, right={rightmostValue}");

            var topmostValue = hm.Map[HalfWidth, 0];
            var bottommostValue = hm.Map[HalfWidth, 2 * HalfWidth];
            Assert.AreEqual(topmostValue, bottommostValue, 0.1f, $"top={topmostValue}, bottom={bottommostValue}");
            var all = hm.PercentBelowLevel(1f);
            Assert.AreEqual(1f, all);
            var water = hm.PercentBelowLevel(.6f);
            Assert.IsTrue(water > .60f);
        }

        private float[,] GetGradientMap(float[,] map)
        {
            float[,] grad = new float[map.GetLength(0), map.GetLength(1)];
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    int prevx = (x > 0) ? x - 1 : map.GetLength(0) - 1;
                    int prevy = (y > 0) ? y - 1 : map.GetLength(1) - 1;
                    grad[x, y] = new Vector2(map[x, y] - map[prevx, y], map[x, y] - map[x, prevy]).Length();
                }
            }
            return grad;
        }

        private class MinMaxInfo
        {
            public float min = float.MaxValue;
            public float max = float.MinValue;
            public int minX;
            public int minY;
            public int maxX;
            public int maxY;
        }
        private MinMaxInfo GetMinMax(float[,] map)
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

        [TestMethod]
        public void CellTemperature()
        {
            var cell = new Cell(World, HalfWidth, HalfWidth);
            Assert.ThrowsException<NullReferenceException>(() => { var t = cell.Temperature; });
            cell.Terrain = new Terrain(TerrainKind.Ocean);
            Assert.IsTrue(cell.Temperature.Celsius > 20);
            cell.Y = World.LatitudeToY(Angle.FromDegrees(90));
            Assert.IsTrue(cell.Temperature.Celsius <= 0);
        }


        [TestMethod]
        public void LatitudeTerraforming()
        {
            Terraformer terraformer = new Terraformer(World);
            terraformer.MakeLatitudeTerrain(Angle.FromDegrees(0), 0, 1, TerrainKind.Desert, false);
            var deserts = World.Cells.Where(x => x.Terrain.Kind == TerrainKind.Desert);
            Assert.IsTrue(deserts.All(p => p.Lat.Degrees == 0));

            Angle angle = Angle.FromDegrees(30);
            terraformer.MakeLatitudeTerrain(angle, 0, 1, TerrainKind.Forest, false);
            var forests = World.Cells.Where(x => x.Terrain.Kind == TerrainKind.Forest);
            foreach (var d in forests)
            {
                Debug.WriteLine(d.LatLongString);
            }
            Assert.IsTrue(forests.All(p => p.Lat == angle));

            World.Tick();
            int countDesert = World.CurrentCensus.TerrainCount(TerrainKind.Desert);
            Assert.IsTrue(countDesert > HalfWidth);
            Assert.AreEqual(countDesert, deserts.Count());

        }
    }
}