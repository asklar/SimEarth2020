using Environment;
using SimEarth2020;
using System;

namespace Tests
{
    class MockApplicationUI : IApplicationUI
    {
        public float FPS { get; set; }
        public void DebugNotifyFPS(object session, DebugStats stats)
        {
            FPS = stats.FPS;
        }

        public void DrawNewGameHint(object session)
        { }

        public void Inspect(double px, double py, Cell cell)
        { }

        public void RunOnUIThread(Action action)
        {
            action();
        }

        public void SetStatus(string s)
        { }
    }
}
