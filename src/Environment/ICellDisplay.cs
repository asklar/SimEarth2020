namespace Environment
{
    public interface ICellDisplay
    {
        int X { get; }
        int Y { get; }
        void UpdateAnimal();
        void UpdateTerrain();
    }
}