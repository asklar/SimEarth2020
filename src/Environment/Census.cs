using System;

namespace Environment
{
    public class Census
    {
        int[] animalPackCount;
        int[] totalAnimals;
        int[] terrainCount;
        int[] totalTerrainFoodAvailable;
        public Census()
        {
            animalPackCount = new int[Enum.GetValues(typeof(AnimalKind)).Length];
            totalAnimals = new int[Enum.GetValues(typeof(AnimalKind)).Length];
            terrainCount = new int[Enum.GetValues(typeof(TerrainKind)).Length];
            totalTerrainFoodAvailable = new int[Enum.GetValues(typeof(TerrainKind)).Length];
        }
        public void Add(AnimalPack animal)
        {
            animalPackCount[(int)animal.Kind]++;
            totalAnimals[(int)animal.Kind] += animal.Population;
        }
        public void Add(Terrain terrain)
        {
            terrainCount[(int)terrain.Kind]++;
            totalTerrainFoodAvailable[(int)terrain.Kind] += terrain.RemainingFood;
        }

        public int AnimalPackCount(AnimalKind kind) => animalPackCount[(int)kind];
        public int TotalAnimals(AnimalKind kind) => totalAnimals[(int)kind];
        public int TerrainCount(TerrainKind kind) => terrainCount[(int)kind];
        public int TotalTerrainFoodAvailable(TerrainKind kind) => totalTerrainFoodAvailable[(int)kind];
    }
}
