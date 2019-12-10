using System.ComponentModel;

namespace Environment
{
    public interface IController : INotifyPropertyChanged
    {
        double Speed { get; }

        void Click(Cell cell, int px, int py);
        void RaisePropertyChanged(string propName);
        ICellDisplay GetCellDisplay(Cell cell);
        void SetStatus(string s);
        IViewport CreateViewport(World world);
    }
}