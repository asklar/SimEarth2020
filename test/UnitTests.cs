using Environment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestInitialize]
        public void Setup()
        {
        }

        const double epsilon = 0.015;

        [TestMethod]
        public void RandomNormal()
        {
            double mean = 0, stddev = 0;
            const int M = 10;
            Random random = new Random();
            for (int i = 0; i < M; i++)
            {
                GetNormalStatistics(random, out double mu, out double sigma);
                Debug.WriteLine($"Normal stats: mu={mu}, sigma={sigma}");
                mean += mu / M;
                stddev += sigma / M;
            }
            Assert.AreEqual(0, mean, epsilon);
            Assert.AreEqual(1, stddev, epsilon);
        }

        private static void GetNormalStatistics(Random random, out double mean, out double stddev)
        {
            const int N = 2000;
            double c = 0;
            double d = 0;
            for (int i = 0; i < N; i++)
            {
                var x = random.GetNormal();
                c += x;
                d += x * x;
            }
            mean = c / N;
            stddev = Math.Sqrt(d / N);
        }

        [TestMethod]
        public void AnimalStats()
        {
            var animal = new AnimalPack(AnimalKind.Prokaryote, 10);
            Assert.IsNotNull(animal.Stats);
            Assert.AreEqual(animal.Population, 10);
            Assert.AreEqual(animal.Kind, AnimalKind.Prokaryote);
            Assert.AreEqual(animal.Stats.Kind, AnimalKind.Prokaryote);
            Assert.IsTrue(animal.Stats.CanSwim);
            Assert.IsFalse(animal.Stats.CanWalk);
        }

        [TestMethod]
        public void TerrainStats()
        {
            Assert.IsTrue(Enum.GetValues(typeof(TerrainKind)).Length >= 9);
            for (int i = 0; i < Enum.GetValues(typeof(TerrainKind)).Length; i++)
            {
                var terrain = new Terrain((TerrainKind)i);
                Assert.IsNotNull(terrain.Stats);
                Assert.IsTrue(terrain.RemainingFood > 0, "Remaining food must be >0 for {0}", (TerrainKind)i);
                Assert.AreEqual(terrain.Kind, (TerrainKind)i);
                Assert.IsTrue(terrain.RemainingFood <= terrain.Stats.MaxFood);

            }
        }

        [TestMethod]
        public void TestTemperature()
        {
            Temperature zeroC = Temperature.FromKelvin(273.15);
            Assert.AreEqual(0, zeroC.Celsius, epsilon);
            Temperature eighteenC = Temperature.FromKelvin(zeroC.Kelvin + 18);
            Assert.AreEqual(18, eighteenC.Celsius, epsilon);
            Assert.AreEqual(64.4, eighteenC.Fahrenheit, epsilon);
        }

        [TestMethod]
        public void TestAngle()
        {
            var right = Angle.FromDegrees(90);
            Assert.AreEqual(1, Math.Sin(right.Radians), epsilon);
            Assert.AreEqual(0, Math.Cos(right.Radians), epsilon);
        }
    }
}