using NUnit.Framework;
using SimEarth2020;
using System;

namespace SimEarthTests
{
    public class ModelTests
    {
        World World { get; set; }
        MockController Controller { get; set; }
        [SetUp]
        public void Setup()
        {
            Controller = new MockController();
            World  = new World(Controller) { Width = 11, Radius = 1 };
        }

        [Test]
        public void TestCell()
        {
            Cell cell = new Cell(World, 5, 5);
            Assert.AreEqual(0, cell.Lat.Degrees, 1);
            Assert.AreEqual(0, cell.Long.Degrees, 1);
            Assert.AreEqual(5, World.LatitudeToY(Angle.FromDegrees(0)));
            Assert.Throws<NullReferenceException>(() => { var t = cell.Temperature; });
            cell.Terrain = new Terrain(TerrainKind.Ocean);
            Assert.IsTrue(cell.Temperature.Celsius > 20);
            cell.Y = World.LatitudeToY(Angle.FromDegrees(90));
            Assert.IsTrue(cell.Temperature.Celsius <= 0);
        }

    }
}