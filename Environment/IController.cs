using System.ComponentModel;

namespace Environment
{
    public interface IController : INotifyPropertyChanged
    {
        double Speed { get; }

        void Click(Cell cell);
        void RaisePropertyChanged(string propName);
        ICellDisplay GetCellDisplay(Cell cell);
        void SetStatus(string s);
        void AddToGrid(ICellDisplay display);
    }
}