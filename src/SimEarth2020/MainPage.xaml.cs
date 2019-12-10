using Environment;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Viewport2D;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace SimEarth2020
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainPage : Page, IController
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

        public MainPage()
        {
            InitializeComponent();
            this.DataContext = this;
            Unloaded += (_, _1) => { };
            this.WorldCanvas.LayoutUpdated += (s, a) =>
            {
                UpdateViewportSize();
            };

            scrollTimer = new Timer(scrollProc, null, 0, 10);
        }

        private void UpdateViewportSize()
        {
            if (CurrentWorld != null)
            {
                CurrentWorld.Viewport.Width = (float)WorldCanvas.RenderSize.Width;
                CurrentWorld.Viewport.Height = (float)WorldCanvas.RenderSize.Height;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            ClearToggleButtonsExcept(sender as ToggleButton);
            EnsurePopupPanelButtons<AnimalKind>(Animals, Tool.Add);
            EnsurePopupPanelButtons<TechTool>(TechTools, Tool.Add);
        }

        private void ClearToggleButtonsExcept(ToggleButton toggleButton)
        {
            Panel p = toggleButton.Parent as Panel;
            foreach (UIElement u in p.Children)
            {
                if (u is ToggleButton)
                {
                    ToggleButton b = u as ToggleButton;
                    if (b != toggleButton)
                    {
                        b.IsChecked = false;
                    }
                }
            }

        }
        private void scrollProc(object state)
        {
            if (CurrentWorld != null)
            {
                CurrentWorld.Viewport.Scroll(displacementDirection);
            }
        }

        private int CurrentToolCost
        {
            get => currentToolCost;
            set { currentToolCost = value; RaisePropertyChanged("CostString"); }
        }
        private int GetToolCost(object tool)
        {
            if (tool == null)
            {
                return 0;
            }
            else if (tool is AnimalKind)
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

                    var panel = new Grid();
                    panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20) });
                    panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80) });
                    panel.Children.Add(new Rectangle() { Width = 20, Height = 20, Fill = new SolidColorBrush(Colors.BlueViolet), HorizontalAlignment = HorizontalAlignment.Left }); // TODO
                    TextBlock text = new TextBlock()
                    {
                        Text = Util.MakeEnumName(Enum.GetName(typeof(TEnum), value)),
                        TextAlignment = TextAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        FontSize = 10
                    };
                    Grid.SetColumn(text, 2);
                    panel.Children.Add(text);
                    Button button = new Button()
                    {
                        Content = panel,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Padding = new Thickness(0)
                    };
                    button.Click += (sender, args) =>
                    {
                        SetCurrentTool(tool, value);
                        var p = Util.FindParent<Popup>(panel);
                        if (p != null)
                        {
                            p.IsOpen = false;
                        }
                    };
                    container.Children.Add(button);
                }
            }
        }

        public double Scaling
        {
            get
            {
                if (CurrentWorld == null) return 100;
                else return CurrentWorld.Viewport.RenderScale * 100;
            }
            set
            {
                CurrentWorld.Viewport.RenderScale = (float)(value / 100);
                ScalePct.Text = $"{value} %";
                RaisePropertyChanged("Scaling");
                stats = new TimingStats();
            }
        }

        public void SetStatus(string s)
        {
            Debug.WriteLine(s);
            Dispatcher.RunIdleAsync((_) =>
            {
                Status.Text = s;
            });
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

        public int Energy { get => CurrentWorld?.Energy ?? 0; }

        public void Click(Cell cell, int px, int py)
        {
            SetStatus($"Clicked at ({cell.X}, {cell.Y}) {cell.Lat.Degrees}, {cell.Long.Degrees} {cell.Terrain.Kind.ToString()}");
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
                            //Point o = WorldCanvas.TransformToVisual(this).TransformPoint(new Point());
                            InspectPopup.HorizontalOffset = px;// cell.Display.X;
                            InspectPopup.VerticalOffset = py;// WorldScrollViewer.ActualOffset.Y + cell.Display.Y;
                            Inspector.Set(cell);
                            InspectPopup.IsOpen = true;
                        }
                        break;
                }
            }
            else
            {
                SetStatus("Insufficient 🕉");
                /// TODO
                // SystemSounds.Beep.Play();
            }
        }

        private World CurrentWorld { get; set; }
        public double Speed { get { return 1e4; } }

        private World GetNewWorld()
        {
            World world = new World(this, 200)
            {
                Name = "Random world",
                Age = 900e3,
                Radius = 6.3e6,
                Energy = 1000
            };

            // TODO: Configure world options like size, age, etc.
            return world;
        }

        public void StartNewGame()
        {
            var world = GetNewWorld();
            StartNewGame(world);
        }

        public void StartNewGame(World world)
        {
            CurrentWorld = world;
            UpdateViewportSize();
            Scaling = 33f;
            world.Terraform();
        }

        private TimingStats stats = new TimingStats();
        private CanvasTextFormat format = new CanvasTextFormat() { FontSize = 8 };

        private void WorldCanvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            if (CurrentWorld != null)
            {
                Stopwatch s = Stopwatch.StartNew();
                Viewport viewport = CurrentWorld.Viewport as Viewport;
                viewport.Draw(args);
                s.Stop();
                stats.AddValue(s.ElapsedMilliseconds);
                float t = stats.GetValue();
                float fps = 1000f / t;
                args.DrawingSession.DrawText($"Draw [{viewport.RenderScale:N2}x] {fps:N1} fps ", new Vector2(0, 10), Colors.Black, format);

            }
            else
            {
                args.DrawingSession.Clear(Colors.Aqua);
                args.DrawingSession.DrawText("Select New Game to start", new Vector2(200, 200), Colors.Black);
            }
        }

        private void WorldCanvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CurrentWorld?.Tick();
                lfg?.Update();
            }).AsTask().Wait();
        }


        LifeFormBarGraph lfg;
        private void LifeFormBiomeGraph_Click(object sender, RoutedEventArgs e)
        {
            if (lfg == null)
            {
                lfg = new LifeFormBarGraph(CurrentWorld);
            }
            lfg.Closed += (s, a) => { lfg = null; };
            lfg.ShowAsync().AsTask().Wait();
        }

        public ICellDisplay GetCellDisplay(Cell cell)
        {
            var cd = new CellDisplay2D(cell);
            return cd;
        }

        public IViewport CreateViewport(World world)
        {
            return new Viewport(world);
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private DisplacementDirection displacementDirection = DisplacementDirection.None;
        Timer scrollTimer;
        private void Scroll(DisplacementDirection d)
        {
            if (displacementDirection != d)
            {
                displacementDirection = d;
            }
        }

        private void WorldCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (CurrentWorld == null) { return; }
            var pp = e.GetCurrentPoint(WorldCanvas);
            var pt = pp.Position;
            if (pt.X < 10)
            {
                Scroll(DisplacementDirection.Left);
            }
            else if (pt.X > CurrentWorld.Viewport.Width - 10)
            {
                Scroll(DisplacementDirection.Right);
            }
            else if (pt.Y < 10)
            {
                Scroll(DisplacementDirection.Up);
            }
            else if (pt.Y > CurrentWorld.Viewport.Height - 20)
            {
                Scroll(DisplacementDirection.Down);
            }
            else
            {
                Scroll(DisplacementDirection.None);
            }

            if (pp.Properties.IsLeftButtonPressed)
            {
                Click(CurrentWorld.Viewport.GetCellAtPoint(pt), (int)pt.X, (int)pt.Y);
            }
        }

        private void WorldCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(WorldCanvas).Position;
            Cell clickedCell = CurrentWorld.Viewport.GetCellAtPoint(pt);
            Click(clickedCell, (int)pt.X, (int)pt.Y);
        }

        private void WorldCanvas_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Scroll(DisplacementDirection.None);
        }
    }
}
