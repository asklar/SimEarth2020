using System.ComponentModel;

namespace SimEarth2020
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