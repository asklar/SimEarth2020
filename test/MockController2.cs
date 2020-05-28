using Environment;
using SimEarth2020;
using System.Collections.Generic;
using Windows.Foundation;

namespace Tests
{
    internal class MockController2 : AppController
    {
        public MockController2(IApplicationUI ui) : base(ui) { }
        public override ICellDisplay GetCellDisplay(Cell cell)
        {
            return new MockCellDisplay(this);
        }

        internal List<Point> DrawnCells { get; set; } = new List<Point>();

        internal void NotifyDraw(float x, float y)
        {
            DrawnCells.Add(new Point(x, y));
        }
    }
}
