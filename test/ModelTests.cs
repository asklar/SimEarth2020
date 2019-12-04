using Environment;
using NUnit.Framework;
using SimEarth2020;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimEarthTests
{
    public class ModelTests
    {
        World World { get; set; }
        MockController Controller { get; set; }
        const int HalfWidth = 35;
        double benchmark;

        Stopwatch watch;
        [SetUp]
        public void Setup()
        {
            watch = new Stopwatch();
            SetUpBenchmark();

            Controller = new MockController();
            watch.Restart();
            World = new World(Controller) { Width = 2 * HalfWidth + 1, Radius = 1 };
            watch.Stop();
            Assert.IsTrue(watch.ElapsedMilliseconds < 5);

            Profile(() => World.Start(), 8e6);
        }

        private void SetUpBenchmark()
        {
            benchmark = 0;
            long M = 100000;
            while (benchmark < 100)
            {
                M *= 10;
                watch.Restart();
                for (long i = 0; i < M; i++)
                { }
                watch.Stop();
                benchmark = watch.ElapsedMilliseconds;
            }
            benchmark /= M;
            Assert.IsTrue(watch.ElapsedMilliseconds >= 100);
        }

        private void Profile(Action action, double expectedCycleCount)
        {
            if (benchmark == 0)
            {
                SetUpBenchmark();
            }
            watch.Restart();
            action();
            watch.Stop();
            double k = watch.ElapsedMilliseconds / benchmark;
            Assert.IsTrue(k < expectedCycleCount, "Operation took {0} cycles", k);
        }
 
        [Test]
        public void PerformanceWorld()
        {
            Profile(() => World.Terraform(), 1e8);
        }

        [Test]
        public void TestCell()
        {
            Cell cell = new Cell(World, HalfWidth, HalfWidth);
            Assert.AreEqual(0, cell.Lat.Degrees, 1);
            Assert.AreEqual(0, cell.Long.Degrees, 1);
            Assert.AreEqual(HalfWidth, World.LatitudeToY(Angle.FromDegrees(0)));
            Assert.AreEqual(0, World.LatitudeToY(Angle.FromDegrees(90)));
            Assert.AreEqual(HalfWidth * 2, World.LatitudeToY(Angle.FromDegrees(-90)));

        }

        [Test]
        public void RoundtripLatitude1()
        {
            const int Y = 15;
            Cell cell = new Cell(World, 0, Y);
            Assert.AreEqual(Y, World.LatitudeToY(cell.Lat));
        }

        [Test]
        public void RoundtripLatitude2()
        {
            Angle angle = Angle.FromDegrees(30);
            Cell cell2 = new Cell(World, 0, World.LatitudeToY(angle));
            Assert.AreEqual(angle.Degrees, cell2.Lat.Degrees, 1);
        }

        [Test]
        public void CellTemperature()
        {
            Cell cell = new Cell(World, HalfWidth, HalfWidth);
            Assert.Throws<NullReferenceException>(() => { var t = cell.Temperature; });
            cell.Terrain = new Terrain(TerrainKind.Ocean);
            Assert.IsTrue(cell.Temperature.Celsius > 20);
            cell.Y = World.LatitudeToY(Angle.FromDegrees(90));
            Assert.IsTrue(cell.Temperature.Celsius <= 0);
        }


        [Test]
        public void LatitudeTerraforming()
        {
            World.MakeLatitudeTerrain(Angle.FromDegrees(0), 0, 1, TerrainKind.Desert, false);
            var deserts = World.cells.Where(x => x.Terrain.Kind == TerrainKind.Desert);
            Assert.IsTrue(deserts.All(p => p.Lat.Degrees == 0));

            Angle angle = Angle.FromDegrees(30);
            World.MakeLatitudeTerrain(angle, 0, 1, TerrainKind.Forest, false);
            var forests = World.cells.Where(x => x.Terrain.Kind == TerrainKind.Forest);
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