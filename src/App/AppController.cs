using Environment;
using Microsoft.Graphics.Canvas;
using SimEarth2020;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Viewport2D;
using Windows.Foundation;
using Windows.UI;

namespace SimEarth2020App
{
    public class AppController : IController
    {
        public AppController(IApplicationUI npc)
        {
            UI = npc;
            scrollTimer = new Timer(scrollProc, null, 0, 10);
        }
        public void RaisePropertyChanged(string propName) { UI.RaisePropertyChanged(propName); }
        public double Speed { get { return 1e4; } }
        private int currentToolCost;
        public int CurrentToolCost
        {
            get => currentToolCost;
            set { currentToolCost = value; UI.RaisePropertyChanged("CostString"); }
        }

        public bool TerrainUpDownMode
        {
            get { return terrainUpDownModeIsUp; }
            set { terrainUpDownModeIsUp = value; UI.RaisePropertyChanged("TerrainUpDownString"); }
        }
        bool terrainUpDownModeIsUp = true;

        private Tool currentTool = Tool.None;


        public string CurrentToolString
        {
            get
            {
                return CurrentTool.ToString() + " " + toolOption?.ToString();
            }
        }
        private object toolOption;

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

        private DisplacementDirection displacementDirection = DisplacementDirection.None;
        public void Scroll(DisplacementDirection d)
        {
            if (displacementDirection != d)
            {
                displacementDirection = d;
            }
        }

        private Timer scrollTimer;

        private void scrollProc(object state)
        {
            if (CurrentWorld != null)
            {
                CurrentWorld.Viewport?.Scroll(displacementDirection);
            }
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

        public World CurrentWorld { get; set; }


        public float Scaling
        {
            get
            {
                if (CurrentWorld == null) return 100;
                else return CurrentWorld.Viewport.RenderScale * 100;
            }
            set
            {
                CurrentWorld.Viewport.RenderScale = value / 100f;
                RaisePropertyChanged("Scaling");
                stats = new TimingStats();
            }
        }

        private TimingStats stats = new TimingStats();


        private Tool CurrentTool
        {
            get => currentTool;
            set
            {
                currentTool = value;
                UI.RaisePropertyChanged("CurrentToolString");
            }
        }

        public IApplicationUI UI { get; set; }

        public void UpdateViewportSize(float width, float height)
        {
            if (CurrentWorld != null)
            {
                CurrentWorld.Viewport.Width = width;
                CurrentWorld.Viewport.Height = height;
            }
        }

        public virtual ICellDisplay GetCellDisplay(Cell cell)
        {
            var cd = new CellDisplay2D(cell);
            return cd;
        }

        public virtual IViewport CreateViewport()
        {
            return new Viewport(CurrentWorld);
        }

        public void SetCurrentTool(Tool tool, object value)
        {
            var oldTool = CurrentTool;
            toolOption = value;
            CurrentTool = tool;
            CurrentToolCost = GetToolCost(value);
            if (tool == Tool.None)
            {
                UI.SetStatus($"Unselected tool {oldTool}");
            }
            else
            {
                UI.SetStatus($"Set tool to {tool} {value} ({CurrentToolCost})");
            }
        }


        public void Click(Point pt)
        {
            var cell = CurrentWorld.Viewport.GetCellAtPoint(pt);
            var px = pt.X;
            var py = pt.Y;
            UI.SetStatus($"Clicked at ({cell.X}, {cell.Y}) {cell.Lat.Degrees}, {cell.Long.Degrees} {cell.Terrain.Kind.ToString()}");
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
                            UI.Inspect(px, py, cell);
                        }
                        break;
                }
            }
            else
            {
                UI.SetStatus("Insufficient 🕉");
                /// TODO

                // SystemSounds.Beep.Play();
            }
        }

        public void DrawWorld(object arg)
        {
            if (CurrentWorld != null)
            {
                Stopwatch s = Stopwatch.StartNew();
                var viewport = CurrentWorld.Viewport as IViewport;
                viewport.Draw(arg);
                s.Stop();
                stats.AddValue(s.ElapsedMilliseconds);
                float t = stats.GetValue();
                float fps = 1000f / t;
                UI.DrawDebugText(arg, $"Draw [{viewport.RenderScale:N2}x] {fps:N1} fps ");

            }
            else
            {
                var session = arg as CanvasDrawingSession;
                session.Clear(Colors.Aqua);
                session.DrawText("Select New Game to start", new Vector2(200, 200), Colors.Black);
            }
        }

        public World CreateWorld(int size)
        {
            return new World(this, size);
        }
    }
}
