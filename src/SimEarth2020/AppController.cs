using Environment;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Viewport2D;
using Windows.Foundation;

namespace SimEarth2020
{
    public class AppController : IController, INotifyPropertyChanged
    {
        public AppController(IApplicationUI npc)
        {
            UI = npc;
            scrollTimer = new Timer(scrollProc, null, 0, 17);
        }

        public void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            UI.RunOnUIThread(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName)));
        }

        public Speed Speed { get; set; } = Speed.Fast;
        private int currentToolCost;
        public int CurrentToolCost
        {
            get => currentToolCost;
            set { currentToolCost = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentToolCost")); }
        }

        private TerrainUpDownMode terrainUpDownMode = TerrainUpDownMode.None;
        public TerrainUpDownMode TerrainUpDownMode
        {
            get { return terrainUpDownMode; }
            set { terrainUpDownMode = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TerrainUpDownMode")); }
        }

        public int Energy { get => CurrentWorld?.Energy ?? 0; }

        private DisplacementDirection displacementDirection = DisplacementDirection.None;
        public void Scroll(DisplacementDirection d)
        {
            if (displacementDirection != d)
            {
                ToggleSpeed_Scroll();

                displacementDirection = d;
            }
        }

        private Speed scrollSpeed = Speed.Paused;
        private void ToggleSpeed_Scroll()
        {
            if (Speed != Speed.Paused ^ scrollSpeed != Speed.Paused)
            {
                Speed newSpeed = scrollSpeed;
                scrollSpeed = Speed;
                Speed = newSpeed;
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
            else if (tool is TerrainUpDownMode)
            {
                return 200;
            }
            else if (tool == null)
            {
                switch (CurrentTool)
                {
                    case Tool.Inspect:
                        return 5;
                    case Tool.Move:
                        return 25;
                }
            }
            throw new ArgumentException();
        }

        public World CurrentWorld
        {
            get => currentWorld;
            set { currentWorld = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentWorld")); }
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
                CurrentWorld.Viewport.RenderScale = Math.Clamp((float)value, 1f, 150f) / 100f;
                stats = new TimingStats();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Scaling"));
            }
        }

        private TimingStats stats = new TimingStats();
        private World currentWorld;

        public event PropertyChangedEventHandler PropertyChanged;

        private Tool currentTool = Tool.None;
        public Tool CurrentTool
        {
            get => currentTool;
            set
            {
                currentTool = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentTool"));
            }
        }

        public IApplicationUI UI { get; set; }

        public bool MicroMoveEnabled
        {
            get;
            set;
        } = true;
        public bool Blitting
        {
            get
            {
                return CurrentWorld?.Viewport.UseBlitting ?? false;
            }
            set
            {
                if (CurrentWorld != null)
                {
                    CurrentWorld.Viewport.UseBlitting = value;
                    stats = new TimingStats();
                }
            }
        }

        private object toolOption;
        public object ToolOption
        {
            get => toolOption;
            set
            {
                toolOption = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ToolOption"));
            }
        }

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
            ToolOption = value;
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
            if (CurrentWorld == null)
            {
                return;
            }
            var cell = CurrentWorld.Viewport.GetCellAtPoint(pt);
            var px = pt.X;
            var py = pt.Y;
            UI.SetStatus($"Clicked at pt={pt} -> cell ({cell.X}, {cell.Y}) {cell.Lat.Degrees}, {cell.Long.Degrees} {cell.Terrain.Kind.ToString()}");
            if (CurrentToolCost <= CurrentWorld.Energy)
            {
                CurrentWorld.Energy -= CurrentToolCost;
                switch (CurrentTool)
                {
                    case Tool.None:
                        break;
                    case Tool.Add:
                        {
                            if (ToolOption is AnimalKind)
                            {
                                var kind = (AnimalKind)ToolOption;
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
                            else if (ToolOption is TechTool)
                            {
                                cell.TechTool = (TechTool)ToolOption;
                            }
                            else
                            {
                                throw new ArgumentException();
                            }
                        }
                        break;
                    case Tool.TerrainUpDown:
                        {
                            switch (TerrainUpDownMode)
                            {
                                case TerrainUpDownMode.Up:
                                    cell.Elevation += 600; break;
                                case TerrainUpDownMode.Down:
                                    cell.Elevation -= 600; break;
                                default:
                                    throw new InvalidOperationException();
                            }
                        }
                        break;
                    case Tool.Terraform:
                        {
                            var kind = (TerrainKind)ToolOption;
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

        public void Draw(object arg)
        {
            if (CurrentWorld != null && CurrentWorld.IsInited)
            {
                Stopwatch s = Stopwatch.StartNew();
                var viewport = CurrentWorld.Viewport as IViewport;
                viewport.Draw(arg);
                s.Stop();
                stats.AddValue(s.ElapsedMilliseconds);
                float t = stats.GetValue();
                float fps = 1000f / t;
                UI.DebugNotifyFPS(arg, fps);
            }
            else
            {
                UI.DrawNewGameHint(arg);
            }
        }

        public World CreateWorld(int size)
        {
            return new World(this, size);
        }

        public void SetStatus(string v)
        {
            UI.SetStatus(v);
        }
    }
}
