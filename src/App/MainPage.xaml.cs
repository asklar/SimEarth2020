using Environment;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using SimEarth2020;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace SimEarth2020App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainPage : Page, IApplicationUI
    {
        public IController Controller { get; set; }
        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public double Scaling { get => Controller.Scaling; set { Controller.Scaling = (float)value; } }
        public void RaisePropertyChanged(string propName)
        {
            if (propName == "Scaling")
            {
                ScaleControl.Value = Controller.Scaling;
                ScalePct.Text = $"{Controller.Scaling} %";
            }
            else if (propName == "TerrainUpDownString")
            {
                TerrainUpDown.Content = TerrainUpDownString;
            }
            else if (propName == "CostString")
            {
                CostTextBlock.Text = CostString;
            }
            else if (propName == "CurrentToolString")
            {
                CurrentlySelectedToolTextBlock.Text = Controller.CurrentToolString;
            }
            else if (propName == "Energy")
            {
                Budget.Text = (Controller.CurrentWorld != null ? Controller.CurrentWorld.Energy : 0).ToString();
            }
            else if (propName == "TitleString")
            {
                TitleTextBlock.Text = Controller.TitleString;
            }
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public MainPage()
        {
            Controller = new AppController(this);

            InitializeComponent();
            this.DataContext = this;
            Unloaded += (_, _1) => { };
            this.WorldCanvas.LayoutUpdated += (s, a) =>
            {
                Controller.UpdateViewportSize((float)WorldCanvas.RenderSize.Width, (float)WorldCanvas.RenderSize.Height);
            };

        }


        private void Add_Click(object sender, RoutedEventArgs e)
        {
            ClearToggleButtonsExcept(sender as ToggleButton);
            EnsurePopupPanelButtons<AnimalKind>(Animals, Tool.Add);
            EnsurePopupPanelButtons<TechTool>(TechTools, Tool.Add);
        }

        public string CostString
        {
            get
            {
                return "🕉 " + Controller.CurrentToolCost;
            }
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



        public string TerrainUpDownString
        {
            get
            {
                return Controller.TerrainUpDownMode ? "⬆" : "⬇";
            }
        }



        private void TerrainUpDown_Click(object sender, RoutedEventArgs e)
        {
            if (TerrainUpDown.IsChecked.HasValue)
            {
                Controller.TerrainUpDownMode = TerrainUpDown.IsChecked.Value;
                SetStatus("Terrain " + (Controller.TerrainUpDownMode ? "up" : "down"));
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
            Controller.SetCurrentTool(Tool.Inspect, null);
        }

        private void Terraform_Click(object sender, RoutedEventArgs e)
        {
            EnsurePopupPanelButtons<TerrainKind>(Terraform, Tool.Terraform);
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
                    /// TODO: Use an image for every animal/terrain/tech
                    panel.Children.Add(new Rectangle() { Width = 20, Height = 20, Fill = new SolidColorBrush(Colors.BlueViolet), HorizontalAlignment = HorizontalAlignment.Left });
                    TextBlock text = new TextBlock()
                    {
                        Text = Environment.Util.MakeEnumName(Enum.GetName(typeof(TEnum), value)),
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
                        Controller.SetCurrentTool(tool, value);
                        var p = SimEarth2020.Util.FindParent<Popup>(panel);
                        if (p != null)
                        {
                            p.IsOpen = false;
                        }
                    };
                    container.Children.Add(button);
                }
            }
        }


        private World GetNewWorld()
        {
            var watch = Stopwatch.StartNew();
            var world = Controller.CreateWorld(200);
            world.Name = "Random world";
            world.Age = 900e3;
            world.Radius = 6.3e6;
            world.Energy = 1000;
            watch.Stop();
            SetStatus($"Created world in {watch.ElapsedMilliseconds} ms");

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
            Controller.UpdateViewportSize((float)WorldCanvas.RenderSize.Width, (float)WorldCanvas.RenderSize.Height);
            Controller.Scaling = 33f;
            world.Terraform();
        }

        private void WorldCanvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            Controller.Draw(args.DrawingSession);
        }


        private void WorldCanvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Controller.CurrentWorld?.Tick();
                lfg?.Update();
            }).AsTask().Wait();
        }


        LifeFormBarGraph lfg;
        private void LifeFormBiomeGraph_Click(object sender, RoutedEventArgs e)
        {
            if (lfg == null)
            {
                lfg = new LifeFormBarGraph(Controller.CurrentWorld);
            }
            lfg.Closed += (s, a) => { lfg = null; };
            lfg.ShowAsync().AsTask().Wait();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }


        private void WorldCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (Controller.CurrentWorld == null) { return; }
            var pp = e.GetCurrentPoint(WorldCanvas);
            var pt = pp.Position;
            if (pt.X < 10)
            {
                Controller.Scroll(DisplacementDirection.Left);
            }
            else if (pt.X > Controller.CurrentWorld.Viewport.Width - 10)
            {
                Controller.Scroll(DisplacementDirection.Right);
            }
            else if (pt.Y < 10)
            {
                Controller.Scroll(DisplacementDirection.Up);
            }
            else if (pt.Y > Controller.CurrentWorld.Viewport.Height - 20)
            {
                Controller.Scroll(DisplacementDirection.Down);
            }
            else
            {
                Controller.Scroll(DisplacementDirection.None);
            }

            if (pp.Properties.IsLeftButtonPressed)
            {
                Controller.Click(pt);
            }
        }

        private void WorldCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(WorldCanvas).Position;
            Controller.Click(pt);
        }

        private void WorldCanvas_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Controller.Scroll(DisplacementDirection.None);
        }

        public void Inspect(double px, double py, Cell cell)
        {
            //Point o = WorldCanvas.TransformToVisual(this).TransformPoint(new Point());
            InspectPopup.HorizontalOffset = px;// cell.Display.X;
            InspectPopup.VerticalOffset = py;// WorldScrollViewer.ActualOffset.Y + cell.Display.Y;
            Inspector.Set(cell);
            InspectPopup.IsOpen = true;
        }


        public void SetStatus(string s)
        {
            Environment.Util.Debug(s);
            Dispatcher.RunIdleAsync((_) =>
            {
                Status.Text = Controller.CurrentWorld.CurrentTick + " " + s;
            });
        }

        public void DebugNotifyFPS(object args, float fps)
        {
            CanvasDrawingSession session = args as CanvasDrawingSession;
            session.DrawText($"Draw [{(Controller.Scaling/100f):N2}x] {fps:N1} fps", new Vector2(0, 10), Colors.Black, format);
        }

        public void DrawNewGameHint(object arg)
        {
            var session = arg as CanvasDrawingSession;
            session.Clear(Colors.Aqua);
            session.DrawText("Select New Game to start", new Vector2(200, 200), Colors.Black);
        }

        private CanvasTextFormat format = new CanvasTextFormat() { FontSize = 8 };

    }
}
