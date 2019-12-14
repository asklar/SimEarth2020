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
    public interface IController : IDrawable
    {
        Speed Speed { get; set; }

        void Click(Point pt);
        ICellDisplay GetCellDisplay(Cell cell);
        World CurrentWorld { get; set; }
        float Scaling { get; set; }
        bool TerrainUpDownMode { get; set; }
        int CurrentToolCost { get; set; }
        string CurrentToolString { get; }
        string TitleString { get; }
        bool MicroMoveEnabled { get; }

        IViewport CreateViewport();
        void RaisePropertyChanged(string propName);
        void UpdateViewportSize(float width, float height);
        void SetCurrentTool(Tool tool, object value);
        void Scroll(DisplacementDirection d);
        World CreateWorld(int size);
        void SetStatus(string v);
    }
}