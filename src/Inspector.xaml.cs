using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            Location.Text = cell.LatLongString();
            TerrainType.Text = cell.Terrain.Kind.ToString();
            Food.Text = cell.Terrain.RemainingFood.ToString();
            Temperature.Text = $"{cell.Temperature.Celsius:N1}°";

            AnimalKind.Text = cell.Animal != null ? cell.Animal.Kind.ToString() : "None";
            Population.Text = $"Pop: {cell.Animal?.Population} HP {cell.Animal?.TotalHP}";
            FoodSources.Text = cell.Animal?.Stats.FoodSources.ToString();
        }
    }
}
