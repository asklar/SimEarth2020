using Environment;
using SimEarth2020;
using System;
using System.ComponentModel;

namespace SimEarthTests
{
    public class MockController : IController
    {
        public double Speed => 1;

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddToGrid(ICellDisplay display)
        {
        }

        public void Click(Cell cell)
        {
            throw new NotImplementedException();
        }

        public ICellDisplay GetCellDisplay(Cell cell)
        {
            return new MockCellDisplay();
        }

        public void RaisePropertyChanged(string propName)
        { }

        public void SetStatus(string s)
        { }
    }
}