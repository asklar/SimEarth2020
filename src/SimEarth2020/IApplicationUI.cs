using Environment;

namespace SimEarth2020App
{
    public interface IApplicationUI
    {
        void RaisePropertyChanged(string propName);
        void Inspect(double px, double py, Cell cell);
        void SetStatus(string s);
        void DrawDebugText(object session, string v);
    }
}
