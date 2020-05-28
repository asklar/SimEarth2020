// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Viewport2D
{
    public abstract class PlanarProjection : IProjection
    {
        protected int Size;
        public PlanarProjection(int s) { Size = s; }

        public abstract float LatitudeToY(float angle);
        public abstract float YToLatitude(int Y);
    }
}
