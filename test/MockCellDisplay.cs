using Environment;

namespace Tests
{
    public class MockCellDisplay : ICellDisplay
    {
        public int X { get; set; }
        public int Y { get; set; }
        public void UpdateAnimal()
        { }

        public void UpdateTerrain()
        { }
    }
}