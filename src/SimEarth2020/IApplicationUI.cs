﻿using Environment;

namespace SimEarth2020
{
    public interface IApplicationUI
    {
        void RaisePropertyChanged(string propName);
        void Inspect(double px, double py, Cell cell);
        void SetStatus(string s);
        void DebugNotifyFPS(object session, float fps);
        void DrawNewGameHint(object session);
    }
}
