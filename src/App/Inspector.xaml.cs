using Environment;
using Windows.UI.Xaml.Controls;

namespace SimEarth2020App
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

        public void Set(Cell cell)
        {
            Location.Text = cell.LatLongString;
            TerrainType.Text = cell.Terrain.Kind.ToString();
            Food.Text = cell.Terrain.RemainingFood.ToString();
            Temperature.Text = $"{cell.Temperature.Celsius:N1}°";
            if (cell.Animal != null)
            {
                AnimalKind.Text = cell.Animal.Kind.ToString();
                Population.Text = $"Pop: {cell.Animal.Population}\nL {cell.Animal.Location}";
                FoodSources.Text = cell.Animal.Stats.FoodSources.ToString();
            }
            else
            {
                AnimalKind.Text = "";
                Population.Text = "";
                FoodSources.Text = "";
            }
            this.InvalidateArrange();
        }
    }
}
