using Environment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class ModelTests
    {
        World World { get; set; }
        MockController Controller { get; set; }
        const int HalfWidth = 35;

        PerfUtil perf;
        [TestInitialize]
        public void Setup()
        {
            perf = new PerfUtil();

            Controller = new MockController();
            var watch = Stopwatch.StartNew();
            World = new World(Controller, 2 * HalfWidth + 1) { Radius = 1 };
            watch.Stop();
            Assert.IsTrue(watch.ElapsedMilliseconds < 500);

            // perf.Profile(() => World.Start(), 8e6);
        }


        [TestMethod]
        public void PerformanceWorld()
        {
            perf.Profile(() => World.Terraform(), 1e8);
        }

        [TestMethod]
        public void TestCell()
        {
            Cell cell = new Cell(World, HalfWidth, HalfWidth);
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
            Cell cell = new Cell(World, 0, Y);
            Assert.AreEqual(Y, World.LatitudeToY(cell.Lat));
        }

        [TestMethod]
        public void RoundtripLatitude2()
        {
            Angle angle = Angle.FromDegrees(30);
            Cell cell2 = new Cell(World, 0, World.LatitudeToY(angle));
            Assert.AreEqual(angle.Degrees, cell2.Lat.Degrees, 1);
        }

        [TestMethod]
        public void CellTemperature()
        {
            Cell cell = new Cell(World, HalfWidth, HalfWidth);
            Assert.ThrowsException<NullReferenceException>(() => { var t = cell.Temperature; });
            cell.Terrain = new Terrain(TerrainKind.Ocean);
            Assert.IsTrue(cell.Temperature.Celsius > 20);
            cell.Y = World.LatitudeToY(Angle.FromDegrees(90));
            Assert.IsTrue(cell.Temperature.Celsius <= 0);
        }


        [TestMethod]
        public void LatitudeTerraforming()
        {
            World.MakeLatitudeTerrain(Angle.FromDegrees(0), 0, 1, TerrainKind.Desert, false);
            var deserts = World.Cells.Where(x => x.Terrain.Kind == TerrainKind.Desert);
            Assert.IsTrue(deserts.All(p => p.Lat.Degrees == 0));

            Angle angle = Angle.FromDegrees(30);
            World.MakeLatitudeTerrain(angle, 0, 1, TerrainKind.Forest, false);
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