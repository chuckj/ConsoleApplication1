using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
