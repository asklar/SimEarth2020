using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Windows.Controls.Primitives;

namespace SimEarth2020
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        internal void RaisePropertyChanged(string propName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private bool isAddToolPopupOpen = false;
        private Tool currentTool = Tool.None;
        private object toolOption;
        private int currentToolCost;
        private bool isTerraformPopupOpen;

        public bool IsAddToolPopupOpen
        {
            get => isAddToolPopupOpen;
            set
            {
                isAddToolPopupOpen = value;
                RaisePropertyChanged("IsAddToolPopupOpen");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (!isAddToolPopupOpen)
            {
                IsAddToolPopupOpen = true;
                EnsurePopupPanelButtons<Animal>(Animals, Tool.Add);
                EnsurePopupPanelButtons<TechTool>(TechTools, Tool.Add);
            }
        }

        private int CurrentToolCost
        {
            get => currentToolCost;
            set { currentToolCost = value; RaisePropertyChanged("CostString"); }
        }
        private int GetToolCost(object tool)
        {
            if (tool is Animal)
            {
                return 50 + 10 * (int)((Animal)tool);
            }
            else if (tool is TechTool)
            {
                return 70 + 10 * (int)((TechTool)tool);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public string CurrentToolString
        {
            get
            {
                return CurrentTool.ToString() + " " + toolOption?.ToString();
            }
        }

        public string CostString
        {
            get
            {
                return "🕉 " + CurrentToolCost;
            }
        }

        private Tool CurrentTool
        {
            get => currentTool;
            set { currentTool = value; RaisePropertyChanged("CurrentToolString"); }
        }

        public bool IsTerraformPopupOpen
        {
            get => isTerraformPopupOpen;
            set
            {
                isTerraformPopupOpen = value; RaisePropertyChanged("IsTerraformPopupOpen");
            }
        }

        bool terrainUpDownModeIsUp = true;

        public string TerrainUpDownString
        {
            get
            {
                return terrainUpDownModeIsUp ? "⬆" : "⬇";
            }
        }

        private bool TerrainUpDownMode
        {
            get { return terrainUpDownModeIsUp; }
            set { terrainUpDownModeIsUp = value; RaisePropertyChanged("TerrainUpDownString"); }
        }

        public void SetCurrentTool(Tool tool, object value)
        {
            var oldTool = CurrentTool;
            toolOption = value;
            CurrentTool = tool;
            CurrentToolCost = GetToolCost(value);
            if (tool == Tool.None)
            {
                SetStatus($"Unselected tool {oldTool}");
            }
            else
            {
                SetStatus($"Set tool to {tool} {value} ({CurrentToolCost})");
            }
        }

        private void TerrainUpDown_Click(object sender, RoutedEventArgs e)
        {
            if (TerrainUpDown.IsChecked.HasValue)
            {
                TerrainUpDownMode = TerrainUpDown.IsChecked.Value;
                SetStatus("Terrain " + (TerrainUpDownMode ? "up" : "down"));
            }
        }

        private void Disaster_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Move_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Inspect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Terraform_Click(object sender, RoutedEventArgs e)
        {
            if (!isTerraformPopupOpen)
            {
                IsTerraformPopupOpen = true;
                EnsurePopupPanelButtons<Terrain>(Terraform, Tool.Terraform);
            }
        }

        private void EnsurePopupPanelButtons<TEnum>(Panel container, Tool tool)
where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            if (!typeof(TEnum).IsEnum) throw new ArgumentException();

            if (container.Children.Count == 0)
            {
                // first time opening the popup
                var values = Enum.GetValues(typeof(TEnum));
                foreach (var value in values)
                {
                    var panel = new DockPanel();
                    panel.Children.Add(new Image() { Width = 20, Height = 20 }); // TODO
                    TextBlock text = new TextBlock() { Text = Util.MakeEnumName(Enum.GetName(typeof(TEnum), value)), HorizontalAlignment = HorizontalAlignment.Right };
                    DockPanel.SetDock(text, Dock.Right);
                    panel.Children.Add(text);
                    Button button = new Button() { HorizontalContentAlignment = HorizontalAlignment.Left };
                    button.Content = panel;
                    button.Click += (sender, args) =>
                    {
                        SetCurrentTool(tool, value);
                        var p = Util.FindParent<Popup>(button);
                        if (p != null)
                        {
                            p.IsOpen = false;
                        }
                    };
                    container.Children.Add(button);
                }
            }
        }



        public void SetStatus(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
            Status.Content = s;
        }

        public string TitleString
        {
            get
            {
                if (CurrentWorld == null)
                {
                    return "";
                }
                else
                {
                    return CurrentWorld.ToString();
                }
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            var world = GetNewWorld();
            StartWorld(world);
        }

        public int Energy { get => CurrentWorld?.Energy ?? 0; }
        private void StartWorld(World world)
        {
            CurrentWorld = world;
            for (int i = 0; i < world.Width; i++)
            {
                WorldGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(16) }) ;
            }
            for (int i = 0; i < world.Height; i++)
            {
                WorldGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(16) });
            }
            for (int x = 0; x < world.Width; x++)
            {
                for (int y = 0; y < world.Height; y++)
                {
                    var cell = new Cell(world, x, y) { Content = "X" };
                    cell.Click += (sender, args) => { (sender as Cell).DoClick(); } ;
                    Grid.SetRow(cell, y);
                    Grid.SetColumn(cell, x);
                    WorldGrid.Children.Add(cell);
                }
            }
            timer = new Timer(1000);
            timer.Elapsed += (sender, args) => { world.Tick(); };
            timer.Start();
        }

        Timer timer;

        private void Click(Cell cell)
        {
            SetStatus($"Clicked at {cell.Lat.Degrees}, {cell.Long.Degrees}");
            if (CurrentToolCost <= CurrentWorld.Energy)
            {
                CurrentWorld.Energy -= CurrentToolCost;
                switch (CurrentTool)
                {
                    case Tool.None:
                        break;
                    case Tool.Add:
                        {
                            if (toolOption is Animal)
                            {
                                cell.Animal = (Animal)toolOption;
                            }
                            else if (toolOption is TechTool)
                            {
                                cell.TechTool = (TechTool)toolOption;
                            }
                            else
                            {
                                throw new ArgumentException();
                            }
                        }
                        break;
                    case Tool.TerrainUpDown:
                        {
                            cell.Elevation += terrainUpDownModeIsUp ? 600 : -600;
                        }
                        break;
                    case Tool.Terraform:
                        {
                            cell.Terrain = (Terrain)toolOption;
                        }
                        break;
                }
            }
            else
            {
                SetStatus("Insufficient 🕉");
            }
        }

        private World CurrentWorld { get; set; }
        public double Speed { get { return 1e4; } }

        private World GetNewWorld()
        {
            // TODO: Configure world options like size, age, etc.
            return new World(this)
            {
                Name = "Random world",
                Width = 40,
                Height = 40,
                Age = 900e3,
                Radius = 6.3e6,
                Energy = 1000
            };
        }
    }
}
