using Environment;
using System;

namespace SimEarth2020
{
    public class DebugStats
    {
        public float FPS { get; set; }
        public float PctReused { get; set; }
    }
    public interface IApplicationUI
    {
        void Inspect(double px, double py, Cell cell);
        void SetStatus(string s);
        void DebugNotifyFPS(object session, DebugStats stats);
        void DrawNewGameHint(object session);
        void RunOnUIThread(Action action);
    }
}
