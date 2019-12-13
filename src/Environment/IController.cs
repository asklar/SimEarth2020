using Windows.Foundation;

namespace Environment
{
    public interface IController : IDrawable
    {
        double Speed { get; }

        void Click(Point pt);
        ICellDisplay GetCellDisplay(Cell cell);
        World CurrentWorld { get; set; }
        float Scaling { get; set; }
        bool TerrainUpDownMode { get; set; }
        int CurrentToolCost { get; set; }
        string CurrentToolString { get; }

        IViewport CreateViewport();
        void RaisePropertyChanged(string propName);
        void UpdateViewportSize(float width, float height);
        void SetCurrentTool(Tool tool, object value);
        void Scroll(DisplacementDirection d);
        World CreateWorld(int size);
    }
}