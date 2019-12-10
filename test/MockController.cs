using Environment;
using System;
using System.ComponentModel;
using Viewport2D;

namespace Tests
{
    public class MockController : IController
    {
        public double Speed => 1;

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddToGrid(ICellDisplay display)
        {
        }

        public void Click(Cell cell, int px, int py)
        {
            throw new NotImplementedException();
        }

        public IViewport CreateViewport(World world)
        {
            return new Viewport(world);
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