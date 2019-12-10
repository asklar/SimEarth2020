// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Viewport2D
{
    public class TimingStats
    {
        private long N = 0;
        private float lastValue = 0;
        public void AddValue(float f)
        {
            lastValue = (lastValue * N + f) / (N + 1);
            N++;
        }
        public float GetValue()
        {
            return lastValue;
        }
    }
}
