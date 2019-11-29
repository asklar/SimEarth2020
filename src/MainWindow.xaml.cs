﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Windows;
using System.Windows.Controls;
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
                EnsurePopupPanelButtons<AnimalKind>(Animals, Tool.Add);
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
            if (tool is AnimalKind)
            {
                return 50 + 10 * (int)((AnimalKind)tool);
            }
            else if (tool is TechTool)
            {
                return 70 + 10 * (int)((TechTool)tool);
            }
            else if (tool is TerrainKind && CurrentTool == Tool.Terraform)
            {
                // TODO: Figure out costs
                return 10;
            }
            else if (tool == null)
            {
                switch (CurrentTool)
                {
                    case Tool.Inspect:
                        return 5;
                    case Tool.Move:
                        return 25;
                    case Tool.TerrainUpDown:
                        return 200;
                }
            }
            throw new ArgumentException();
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
            SetCurrentTool(Tool.Inspect, null);
        }

        private void Terraform_Click(object sender, RoutedEventArgs e)
        {
            if (!isTerraformPopupOpen)
            {
                IsTerraformPopupOpen = true;
                EnsurePopupPanelButtons<TerrainKind>(Terraform, Tool.Terraform);
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
                    if ((int)value < 0) continue;
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

        public double Scale
        {
            get => scale;
            set
            {
                scale = value;
                ScalePct.Text = (int)(scale * 100) + "%";
                ScaleTransform.ScaleX = scale;
                ScaleTransform.ScaleY = scale;
            }
        }
        public void SetStatus(string s)
        {
            Debug.WriteLine(s);
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
            CurrentWorld = world;
            WorldGrid.Children.Clear();
            WorldGrid.RenderTransform = ScaleTransform;
            world.Start();
        }

        public int Energy { get => CurrentWorld?.Energy ?? 0; }
        private bool isInspectPopupOpen;
        private double scale = 1;

        public void Click(Cell cell)
        {
            SetStatus($"Clicked at ({cell.X}, {cell.Y}) {cell.Lat.Degrees}, {cell.Long.Degrees}");
            if (CurrentToolCost <= CurrentWorld.Energy)
            {
                CurrentWorld.Energy -= CurrentToolCost;
                switch (CurrentTool)
                {
                    case Tool.None:
                        break;
                    case Tool.Add:
                        {
                            if (toolOption is AnimalKind)
                            {
                                var kind = (AnimalKind)toolOption;
                                if (cell.Animal == null || cell.Animal.Kind != kind)
                                {
                                    cell.Animal = new AnimalPack(Util.GetStats(kind));
                                }
                                else
                                {
                                    // refund
                                    CurrentWorld.Energy += CurrentToolCost;
                                }
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
                            var kind = (TerrainKind)toolOption;
                            if (cell.Terrain.Kind != kind)
                            {
                                cell.Terrain = new Terrain() { Kind = kind, RemainingFood = 1000 };
                            }
                            else
                            {
                                CurrentWorld.Energy += CurrentToolCost;
                            }
                        }
                        break;
                    case Tool.Inspect:
                        {
                            IsInspectPopupOpen = true;
                        }
                        break;
                }
            }
            else
            {
                SetStatus("Insufficient 🕉");
                SystemSounds.Beep.Play();
            }
        }

        public bool IsInspectPopupOpen { get => isInspectPopupOpen; set { isInspectPopupOpen = value; RaisePropertyChanged("IsInspectPopupOpen"); } }
        private World CurrentWorld { get; set; }
        public double Speed { get { return 1e4; } }

        private World GetNewWorld()
        {
            // TODO: Configure world options like size, age, etc.
            return new World(this)
            {
                Name = "Random world",
                Width = 60,
                Age = 900e3,
                Radius = 6.3e6,
                Energy = 1000
            };
        }
    }
}
