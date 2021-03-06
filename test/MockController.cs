﻿using Environment;
using System;
using System.ComponentModel;
using Viewport2D;
using Windows.Foundation;

namespace Tests
{
    public class MockController : IController
    {
        public Speed Speed { get; set; } = Speed.Fast;

        public World CurrentWorld { get; set; }
        public double Scaling { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int CurrentToolCost { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool MicroMoveEnabled => false;

        public bool UseBlitting { get; set; }
        public TerrainUpDownMode TerrainUpDownMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Tool CurrentTool { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public object ToolOption { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool UseDiffing { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddToGrid(ICellDisplay display)
        {
        }

        public World CreateWorld(int size)
        {
            return new World(this, size);
        }

        public void Click(Cell cell, int px, int py)
        {
            throw new NotImplementedException();
        }

        public void Click(Point pt)
        {
            throw new NotImplementedException();
        }


        public IViewport CreateViewport()
        {
            return new Viewport(CurrentWorld);
        }

        public void Draw(object session)
        { }

        public ICellDisplay GetCellDisplay(Cell cell)
        {
            return new MockCellDisplay();
        }

        public void RaisePropertyChanged(string propName)
        { }

        public void Scroll(DisplacementDirection d)
        {
            throw new NotImplementedException();
        }

        public void SetCurrentTool(Tool tool, object value)
        {
            throw new NotImplementedException();
        }

        public void SetStatus(string s)
        { }

        public void UpdateViewportSize(float width, float height)
        {
            throw new NotImplementedException();
        }
    }
}