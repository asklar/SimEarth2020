using Environment;
using SimEarth2020;

namespace Tests
{
    class MockApplicationUI : IApplicationUI
    {
        public string DebugText { get; set; }
        public void DrawDebugText(object session, string v)
        {
            DebugText = v;
        }

        public void DrawNewGameHint(object session)
        { }

        public void Inspect(double px, double py, Cell cell)
        { }

        public void RaisePropertyChanged(string propName)
        { }

        public void SetStatus(string s)
        { }
    }
}
