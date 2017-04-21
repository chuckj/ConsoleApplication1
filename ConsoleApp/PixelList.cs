using SD = System.Drawing;

namespace ConsoleApplication1
{
    public class PixelList : IPixelSource
    {
        public SD.Color[] Pixels { get; set; }
        public bool WillRepeat { get; set; }

        private int time;
        private int ndx;

        public void Init()
        {

        }
        public void Set(float pctg)
        {
            ndx = time++ / 30;
        }
        public SD.Color Get()
        {
            if (WillRepeat)
                return Pixels[ndx++ % Pixels.Length];
            else if (ndx < Pixels.Length)
                return Pixels[ndx++];
            return SD.Color.Black;
        }
    }
}
