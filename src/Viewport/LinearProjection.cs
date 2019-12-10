using System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Viewport2D
{

    public class LinearProjection : PlanarProjection
    {
        public LinearProjection(int s) : base(s) { }

        public override float LatitudeToY(float lat)
        {
            return Size / 2.0f - lat / ((float)Math.PI / 2.0f) * Size / 2.0f;
        }

        public override float YToLatitude(int Y)
        {
            throw new NotImplementedException();
        }
    }
}