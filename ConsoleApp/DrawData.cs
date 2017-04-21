using SharpDX.Direct2D1;

namespace ConsoleApplication1
{
    public class DrawData
	{
		public Song Song;
		public int LFT;
		public int RIT;

        public int Width;
        public int Height;
        //public Matrix Transform;
        public int Offset;

        public WindowRenderTarget target;
    }
}
