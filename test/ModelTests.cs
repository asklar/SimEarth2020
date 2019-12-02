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
            Assert.IsTrue(watch.ElapsedMilliseconds < 5);

            Profile(() => World.Start(), 8e6);
            Profile(() => World.Terraform(), 1e8);
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