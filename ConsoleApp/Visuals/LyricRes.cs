using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace ConsoleApplication1
{
    public class LyricRes : VizRes
    {
        public LyricRes() : base()
        {
        }

        public TextFormat Lyric_TextFormat;
        public Brush Lyric_FontBrush;
        public Brush Selected_PrimaryBrush;
        public Brush Selected_SecondaryBrush;
        public Brush HiliteBrush;

        public override void DevDepAcquire(RenderTarget target)
        {
            Selected_PrimaryBrush = SolidColorBrush(target, Global.Selected_PrimaryColor);
            Selected_SecondaryBrush = SolidColorBrush(target, Global.Selected_SecondaryColor);
            HiliteBrush = SolidColorBrush(target, Global.HiliteColor);
            Lyric_FontBrush = SolidColorBrush(target, Global.Lyric_FontColor); 
        }

        public override void DevDepRelease()
        {
            Selected_PrimaryBrush = null;
            Selected_SecondaryBrush = null;
            HiliteBrush = null;
            Lyric_FontBrush = null;
        }

        public override void DevIndepAcquire()
        {
            Lyric_TextFormat = TextFormat("Arial", 10);
        }

        public override void DevIndepRelease()
        {
            Lyric_TextFormat = null;
        }
    }
}
