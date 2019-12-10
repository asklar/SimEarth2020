using Environment;
using Windows.UI;

namespace SimEarth2020
{
    public class CellDisplay : ICellDisplay
    {
        public int X { get; set; }
        public int Y { get; set; }
        protected Color Background
        {
            get;
            set;
        }
        protected Color Foreground { get; set; }
        public void UpdateTerrain()
        {
            Background = GetBackground();
            Foreground = GetForeground();
        }
        private Color GetForeground()
        {
            Color bg = Background;
            if (((double)bg.R + (double)bg.G + (double)bg.B) / 3 > (double)255 / 3)
            {
                return Colors.Black;
            }
            return Colors.White;
        }
        private Color GetBackground()
        {
            Color c = Colors.Red;
            switch (cell.Terrain.Kind)
            {
                case TerrainKind.Tundra:
                    c = Colors.White; break;
                case TerrainKind.Taiga:
                    c = Colors.Azure; break;
                case TerrainKind.Desert:
                    c = Colors.Peru; break;
                case TerrainKind.Forest:
                    c = Colors.ForestGreen; break;
                case TerrainKind.Grass:
                    c = Colors.LawnGreen; break;
                case TerrainKind.Jungle:
                    c = Colors.LimeGreen; break;
                case TerrainKind.Rock:
                    c = Colors.DimGray; break;
                case TerrainKind.Swamp:
                    c = Colors.DarkOliveGreen; break;
                case TerrainKind.Ocean:
                    c = Colors.MidnightBlue; break;
            }
            return c;
        }

        protected Cell cell;

        protected char Text;

        public CellDisplay(Cell cell)
        {
            this.cell = cell;
            Text = ' ';
        }

        public void UpdateAnimal()
        {
            Text = cell.Animal == null ? (char)0 : cell.Animal.Kind.ToString()[0];
        }
    }
}