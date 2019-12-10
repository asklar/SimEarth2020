using Environment;
using Windows.UI.Xaml.Controls;

namespace SimEarth2020
{
    /// <summary>
    /// Interaction logic for Inspector.xaml
    /// </summary>
    public partial class Inspector : UserControl
    {
        public Inspector()
        {
            InitializeComponent();
        }

        internal void Set(Cell cell)
        {
            Location.Text = cell.LatLongString;
            TerrainType.Text = cell.Terrain.Kind.ToString();
            Food.Text = cell.Terrain.RemainingFood.ToString();
            Temperature.Text = $"{cell.Temperature.Celsius:N1}°";
            if (cell.Animal != null)
            {
                AnimalKind.Text = cell.Animal.Kind.ToString();
                Population.Text = $"Pop: {cell.Animal.Population} HP {cell.Animal.TotalHP}";
                FoodSources.Text = cell.Animal.Stats.FoodSources.ToString();
            }
        }
    }
}
