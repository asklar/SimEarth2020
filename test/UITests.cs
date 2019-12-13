using Environment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimEarth2020App;
using System.Collections.Generic;
using Windows.Foundation;

namespace Tests
{
    class MockApplicationUI : IApplicationUI
    {
        public void DrawDebugText(object session, string v)
        { }

        public void Inspect(double px, double py, Cell cell)
        { }

        public void RaisePropertyChanged(string propName)
        { }

        public void SetStatus(string s)
        { }
    }
    [TestClass]
    public class UITests
    {
        MockController2 appController;

        [TestInitialize]
        public void Setup()
        {
            appController = new MockController2(new MockApplicationUI());
            var world = appController.CreateWorld(5);
            world.Terraform();
            appController.CurrentWorld.Tick();
        }

        [TestMethod]
        public void ViewportDraw0x0()
        {
            appController.DrawWorld(null);
            var cells = appController.DrawnCells;
            Assert.AreEqual(4, cells.Count);

            var ps = GenerateSquare(-1, 0);
            Assert.AreEqual(4, ps.Length);
            Assert.IsTrue(Same(ps, cells));
        }

        [TestMethod]
        public void ViewportDraw40x40()
        {
            appController.UpdateViewportSize(40, 40);
            appController.DrawWorld(null);
            var cells = appController.DrawnCells;
            Assert.AreEqual(16, cells.Count);
            Assert.IsTrue(Same(GenerateSquare(-1, 2), cells));
        }

        [TestMethod]
        public void ViewportDrawWithScale()
        {
            appController.UpdateViewportSize(40, 40);
            appController.Scaling = 50f;
            appController.DrawWorld(null);
            var cells = appController.DrawnCells;
            Assert.AreEqual(36, cells.Count); // -1 to 4
            Assert.IsTrue(Same(GenerateSquare(-1, 4), cells));
        }
        private Point[] GenerateSquare(int v1, int v2)
        {
            Point[] ps = new Point[(v2 - v1 + 1) * (v2 - v1 + 1)];
            for (int y = v1; y <= v2; y++)
            {
                for (int x = v1; x <= v2; x++)
                {
                    ps[(y - v1) * (v2 - v1 + 1) + (x - v1)] = new Point(x, y);
                }
            }
            return ps;
        }

        private static bool Same<T>(IList<T> a, IList<T> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (!a[i].Equals(b[i])) return false;
            }
            return true;
        }
    }
}
