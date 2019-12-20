using Environment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Windows.Foundation;

namespace Tests
{
    [TestClass]
    public class UITests
    {
        MockController2 appController;
        MockApplicationUI appui;
        [TestInitialize]
        public void Setup()
        {
            appui = new MockApplicationUI();
            appController = new MockController2(appui);
            var world = appController.CreateWorld(5);
            new Terraformer(world).Terraform();
            appController.CurrentWorld.Tick();
        }

        [TestMethod]
        public void ViewportDraw0x0()
        {
            appController.Draw(null);
            var cells = appController.DrawnCells;
            Assert.AreEqual(1, cells.Count);

            var ps = GenerateSquare(0, 0);
            Assert.AreEqual(1, ps.Length);
            Assert.IsTrue(Same(ps, cells));
        }

        [TestMethod]
        public void ViewportDraw40x40()
        {
            appController.UpdateViewportSize(40, 40);
            appController.Draw(null);
            var cells = appController.DrawnCells;
            Assert.AreEqual(9, cells.Count);
            Assert.IsTrue(Same(GenerateSquare(0, 2), cells));
        }

        [TestMethod]
        public void ViewportDrawWithScale()
        {
            appController.UpdateViewportSize(40, 40);
            appController.Scaling = 50f;
            appController.Draw(null);
            var cells = appController.DrawnCells;
            Assert.AreEqual(25, cells.Count, $"Expected 36 cells but got {cells.Count}"); // 0 to 4
            Assert.IsTrue(Same(GenerateSquare(0, 4), cells), "Cell elements aren't as expected");

            float fps = appui.FPS;
            const float minFPS = 150f;
            Assert.IsTrue(fps >= minFPS, $"fps = {fps} should be >{minFPS}"); // we're not doing any actual drawing, we should be fast
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
