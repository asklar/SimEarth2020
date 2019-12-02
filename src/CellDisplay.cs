using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SimEarth2020
{
    public class CellDisplay : TextBlock, ICellDisplay
    {
        public void UpdateTerrain()
        {
            Background = GetBackground();
            Foreground = GetForeground();
        }
        private Brush GetForeground()
        {
            Color bg = (Background as SolidColorBrush).Color;
            if (((double)bg.R + (double)bg.G + (double)bg.B) / 3 > (double)255 / 3)
            {
                return Brushes.Black;
            }
            return Brushes.White;
        }
        private Brush GetBackground()
        {
            Color c;
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
            return new SolidColorBrush(c);
        }
        private Cell cell;
        public void Initialize(Cell cell)
        {
            this.cell = cell;
            Background = Brushes.Wheat;
            Foreground = Brushes.White;
            TextAlignment = System.Windows.TextAlignment.Center;
            VerticalAlignment = System.Windows.VerticalAlignment.Center;
            Text = " ";
            Margin = new System.Windows.Thickness(0.4);

            MouseDown += (sender, args) => { cell.World.Controller.Click(cell); };
            MouseEnter += (sender, args) =>
            {
                if (args.LeftButton == MouseButtonState.Pressed)
                    cell.World.Controller.Click(cell);
            };
            Grid.SetRow(this, cell.Y);
            Grid.SetColumn(this, cell.X);
        }

        public void UpdateAnimal()
        {
            Text = cell.Animal == null ? "" : cell.Animal.Kind.ToString()[0] + "";
        }
    }
}