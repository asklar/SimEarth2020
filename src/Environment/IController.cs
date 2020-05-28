using Windows.Foundation;

namespace Environment
{
    public enum Speed
    {
        Paused,
        Slow,
        Medium,
        Fast
    }

    public enum TerrainUpDownMode
    {
        None,
        Up,
        Down
    }

    public interface IController : IDrawable
    {
        Speed Speed { get; set; }

        void Click(Point pt);
        ICellDisplay GetCellDisplay(Cell cell);
        World CurrentWorld { get; set; }
        double Scaling { get; set; }
        TerrainUpDownMode TerrainUpDownMode { get; set; }
        int CurrentToolCost { get; set; }
        bool MicroMoveEnabled { get; }
        bool UseBlitting { get; set; }
        Tool CurrentTool { get; set; }
        object ToolOption { get; set; }
        bool UseDiffing { get; set; }

        IViewport CreateViewport();
        void RaisePropertyChanged(string propName);
        void UpdateViewportSize(float width, float height);
        void SetCurrentTool(Tool tool, object value);
        void Scroll(DisplacementDirection d);
        World CreateWorld(int size);
        void SetStatus(string v);
    }
}