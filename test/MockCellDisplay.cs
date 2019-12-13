using Viewport2D;

namespace Tests
{
    internal class MockCellDisplay : ICellDisplay2D
    {
        private MockController2 mockController2;

        public MockCellDisplay(MockController2 mockController2 = null)
        {
            this.mockController2 = mockController2;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public void Draw(object s, float x, float y, float cellSize)
        {
            mockController2.NotifyDraw(x, y);
        }

        public void UpdateAnimal()
        { }

        public void UpdateTerrain()
        { }
    }
}