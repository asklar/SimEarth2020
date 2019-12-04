using Environment;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SimEarth2020
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IController
    {
        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void RaisePropertyChanged(string propName)
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
            Closed += (_, _1) => { timer?.Stop(); };
            cellDisplayBatch = new CellDisplayBatch((cellDisplays) =>
            {
                Dispatcher.Invoke(() =>
                {
                    progress.Value += 10;
                }, System.Windows.Threading.DispatcherPriority.Render);
                Dispatcher.BeginInvoke( new Action(() =>
                {
                    foreach (var d in cellDisplays)
                    {
                        if (d != null)
                        {
                            WorldGrid.Children.Add(d as UIElement);
                        }
                        else
                        {
                            return; // we are in the final flush and found the last element
                        }
                    }
                }));
            });
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

        class CellDisplayBatch
        {
            private Action<IEnumerable<ICellDisplay>> flusher;
            public CellDisplayBatch(Action<IEnumerable<ICellDisplay>> flusher)
            {
                this.flusher = flusher;
            }
            private const int CellDisplayBatchSize = 100;
            private ICellDisplay[] cellDisplays = new ICellDisplay[CellDisplayBatchSize];
            private int nextIndexToUse = 0;
            public void Add(ICellDisplay c)
            {
                cellDisplays[nextIndexToUse++] = c;
                if (nextIndexToUse < CellDisplayBatchSize)
                {
                    // do nothing, we'll process these during a flush
                }
                else
                {
                    Flush();
                }
            }

            public void Flush()
            {
                flusher(cellDisplays);
                for (int i = 0; i < cellDisplays.Length; i++)
                {
                    cellDisplays[i] = null;
                }
                nextIndexToUse = 0;
            }


        }

        private CellDisplayBatch cellDisplayBatch;
        public void AddToGrid(ICellDisplay display)
        {
            if (display != null)
            {
                cellDisplayBatch.Add(display);
            }
            else
            {
                cellDisplayBatch.Flush();
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
                RaisePropertyChanged("Scale");
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
            NewGame(sender, null);
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
                                    cell.Animal = new AnimalPack(kind, 10);
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
                                cell.Terrain = new Terrain(kind);
                            }
                            else
                            {
                                CurrentWorld.Energy += CurrentToolCost;
                            }
                        }
                        break;
                    case Tool.Inspect:
                        {
                            if (IsInspectPopupOpen)
                            {
                                IsInspectPopupOpen = false;
                            }
                            Inspector.Set(cell);
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
                Width = 100,
                Age = 900e3,
                Radius = 6.3e6,
                Energy = 1000
            };
        }

        private void NewGame(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            NewGame();
        }

        public void NewGame()
        {
            var world = GetNewWorld();
            NewGame(world);
        }

        ProgressBar progress;
        public void NewGame(World world)
        {
            CurrentWorld = world;
            progress = new ProgressBar() { Minimum = 0, Maximum = 100, Value = 50, VerticalAlignment = VerticalAlignment.Center, Width = 200 };
            var status = new TextBlock() { Text = $"Creating world {world.Name}", TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Stretch };
            StackPanel panel = new StackPanel() { MinWidth = 200, MinHeight = 80, Background = Brushes.AntiqueWhite, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            panel.Children.Add(progress);
            panel.Children.Add(status);
            Popup popup = new Popup()
            {
                PlacementTarget = WorldGrid,
                PopupAnimation = PopupAnimation.Slide,
                Placement = PlacementMode.Center,
                Child = panel,
                IsOpen = true
            };

            new Thread(() =>
            {
                Thread.Sleep(100);
                Dispatcher.BeginInvoke(new Action(
                    () =>
                    {
                        WorldGrid.Children.Clear();
                        WorldGrid.RenderTransform = ScaleTransform;
                        for (int i = 0; i < Width; i++)
                        {
                            WorldGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(16) });
                        }
                        progress.Value += 5;
                        for (int i = 0; i < Height; i++)
                        {
                            WorldGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(16) });
                        }
                        progress.Value += 5;
                        Scale = .33;

                        world.Start();
                        world.Terraform();
                        if (timer != null)
                        {
                            timer.Stop();
                        }
                        timer = new System.Timers.Timer(1000);
                        timer.Elapsed += (sender, args) =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                world.Tick();
                                lfg?.Update();
                            });
                        };
                        timer.Start();
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() => { popup.IsOpen = false; }));
                    }

                ));
            }).Start();
        }

        System.Timers.Timer timer;

        LifeFormBarGraph lfg;
        private void LifeFormBiomeGraph_Click(object sender, RoutedEventArgs e)
        {
            if (lfg == null)
            {
                lfg = new LifeFormBarGraph(CurrentWorld);
            }
            lfg.Show();
            lfg.Closed += (s, a) => { lfg = null; };
        }

        public ICellDisplay GetCellDisplay(Cell cell)
        {
            var cd = new CellDisplay(cell);
            return cd;
        }
    }
}
