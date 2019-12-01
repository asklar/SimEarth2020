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
using System.Windows.Shapes;

namespace SimEarth2020
{
    /// <summary>
    /// Interaction logic for LifeFormGraph.xaml
    /// </summary>
    public partial class LifeFormBarGraph : Window
    {
        private World world;
        public LifeFormBarGraph(World currentWorld)
        {
            InitializeComponent();
            world = currentWorld;
            this.Content = ShowBarGraph();

            Update();
        }
        private Panel ShowBarGraph()
        {
            modeIsGraph = false;
            string[] names = Enum.GetNames(typeof(AnimalKind));
            Census census = world.CurrentCensus;
            StackPanel p = new StackPanel() { Width = names.Length * (8 + 1), Height = 100, 
                Orientation = Orientation.Horizontal, Background = new SolidColorBrush(Colors.AliceBlue) };
            for (int i = 0; i < names.Length; i++)
            {
                Rectangle rectangle = new Rectangle() { Width = 8, Margin = new Thickness(.5), Height = census.TotalAnimals((AnimalKind)i), Fill = new SolidColorBrush(Colors.Red) };
                int kind = i; // Capture this so the callback doesn't use i which gets updated each iteration
                string name = names[i];
                rectangle.MouseEnter += (s, a) =>
                {
                    var toolTip = new ToolTip() { Content = $"{name} ({world.CurrentCensus.TotalAnimals((AnimalKind)kind)})", IsOpen = true };
                    rectangle.ToolTip = toolTip;
                };
                rectangle.MouseDown += (s, a) =>
                {
                    this.Content = ShowHistory(kind);
                };
                p.Children.Add(rectangle);
            }
            return p;
        }

        AnimalKind currentKind;
        private Panel ShowHistory(int kind)
        {
            modeIsGraph = true;
            Canvas canvas = new Canvas() { Width = ActualWidth, Height = ActualHeight - 50, Background = new SolidColorBrush(Colors.AliceBlue) };
            currentKind = (AnimalKind)kind;
            lastX = -1;
            canvas.MouseDown += (s, a) => { this.Content = ShowBarGraph(); };
            StackPanel p = new StackPanel();
            canvas.RenderTransform = new ScaleTransform();
            Slider s = new Slider() { Minimum = 0.01, Maximum = 10, Value = 1 };
            s.ValueChanged += (_, a) => {
                var st = canvas.RenderTransform as ScaleTransform;
                st.ScaleX = 1/s.Value;
                st.ScaleY = 1/s.Value;
                st.CenterX = lastX;
                st.CenterY = canvas.Height;

                canvas.Width = Width * s.Value;
                canvas.Height = Height * s.Value;
            };
            p.Children.Add(s);
            p.Children.Add(canvas);
            return p;
        }
        public void Update()
        {
            if (!modeIsGraph)
            {
                UpdateBarChart();
            }
            else
            {
                UpdateGraph();
            }
        }
        bool modeIsGraph = false;
        double lastX;
        double lastY;
        double originalHeight;
        private void UpdateGraph()
        {
            Canvas c = (Content as StackPanel).Children[1] as Canvas;
            if (originalHeight == 0)
            {
                originalHeight = c.Height;
            }
            double newY = originalHeight - world.CurrentCensus.TotalAnimals(currentKind) / 10;
            if (lastX != -1)
            {
                Line l = new Line() { X1 = lastX, Y1 = lastY, 
                    X2 = lastX + 1, Y2 = newY, 
                    Stroke = Brushes.Black };
                c.Children.Add(l);
            }
            lastX++;
            lastY = newY;
            return;
        }

        private void UpdateBarChart()
        { 
            StackPanel p = Content as StackPanel;
            for (int i = 0; i < p.Children.Count; i++)
            {
                var bar = p.Children[i] as Rectangle;
                if (bar.ToolTip != null)
                {
                    (bar.ToolTip as ToolTip).IsOpen = false;
                }
                bar.Height = world.CurrentCensus.TotalAnimals((AnimalKind)i);
            }
        }
    }
}
