using NUnit.Framework;
using SimEarth2020;
using System;
using System.Diagnostics;

namespace SimEarthTests
{
    public class ModelTests
    {
        World World { get; set; }
        MockController Controller { get; set; }
        const int HalfWidth = 35;

        Stopwatch watch;
        [SetUp]
        public void Setup()
        {
            Controller = new MockController();
            watch = Stopwatch.StartNew();
            World = new World(Controller) { Width = 2 * HalfWidth + 1, Radius = 1 };
            watch.Stop();
        }

        [Test]
        public void PerformanceWorld()
        {
            Assert.IsTrue(watch.ElapsedMilliseconds < 5);
            watch.Restart();
            World.Start();
            watch.Stop();
            Assert.IsTrue(watch.ElapsedMilliseconds < 10);
            watch.Restart();
            World.Terraform();
            watch.Stop();
            Assert.IsTrue(watch.ElapsedMilliseconds < 250);
        }

        [Test]
        public void TestCell()
        {
            Cell cell = new Cell(World, HalfWidth, HalfWidth);
            Assert.AreEqual(0, cell.Lat.Degrees, 1);
            Assert.AreEqual(0, cell.Long.Degrees, 1);
            Assert.AreEqual(HalfWidth, World.LatitudeToY(Angle.FromDegrees(0)));
            Assert.Throws<NullReferenceException>(() => { var t = cell.Temperature; });
            cell.Terrain = new Terrain(TerrainKind.Ocean);
            Assert.IsTrue(cell.Temperature.Celsius > 20);
            cell.Y = World.LatitudeToY(Angle.FromDegrees(90));
            Assert.IsTrue(cell.Temperature.Celsius <= 0);
        }

    }
}