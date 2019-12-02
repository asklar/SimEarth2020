using SimEarth2020;
using System;
using System.ComponentModel;

namespace SimEarthTests
{
    public class MockController : IController
    {
        public double Speed => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddToGrid(ICellDisplay display)
        {
            throw new NotImplementedException();
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
        {
            throw new NotImplementedException();
        }

        public void SetStatus(string s)
        {
            throw new NotImplementedException();
        }
    }
}