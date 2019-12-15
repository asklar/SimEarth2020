using Environment;
using System;

namespace SimEarth2020
{
    public interface IApplicationUI
    {
        void Inspect(double px, double py, Cell cell);
        void SetStatus(string s);
        void DebugNotifyFPS(object session, float fps);
        void DrawNewGameHint(object session);
        void RunOnUIThread(Action action);
    }
}
