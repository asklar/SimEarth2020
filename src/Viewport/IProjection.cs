// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Viewport2D
{
    public interface IProjection
    {
        float LatitudeToY(float lat);
        float YToLatitude(int Y);
    }
}