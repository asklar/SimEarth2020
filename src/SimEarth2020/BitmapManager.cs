using Environment;
using Microsoft.Graphics.Canvas;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SimEarth2020
{
    public static class BitmapManager
    {
        public static CanvasBitmap[] Animals { get; private set; }

        public static async Task CreateResources(ICanvasResourceCreator canvas)
        {
            Animals = new CanvasBitmap[Enum.GetValues(typeof(AnimalKind)).Length];
            foreach (var v in Enum.GetNames(typeof(AnimalKind)))
            {
                int i = (int)Enum.Parse<AnimalKind>(v);
                string path = $"assets\\{v}.png";
                if (File.Exists(path))
                {
                    Animals[i] = await CanvasBitmap.LoadAsync(canvas, path);
                }
            }
            //Prokaryote = await CanvasBitmap.LoadAsync(canvas, @"assets\Prokaryote.png");
        }
    }

}
