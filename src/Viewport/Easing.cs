// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

using System;

namespace Viewport2D
{
    public interface IEasing
    {
        float GetValue(float f);
    }

    public class LinearEasing : IEasing
    {
        public float GetValue(float f)
        {
            return f;
        }
    }
    public class SinEasing : IEasing
    {
        public float GetValue(float f)
        {
            var fraction = (float)Math.Sin(Math.PI / 2f * Math.Clamp(f, 0f, 1f));
            return fraction;
        }
    }
    public class CosEasing : IEasing
    {
        public float GetValue(float f)
        {
            return 1f - (float)Math.Cos(Math.PI / 2f * Math.Clamp(f, 0f, 1f));
        }
    }



}
