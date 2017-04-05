using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SD = System.Drawing;

namespace ConsoleApplication1
{
    public class MonoLit : Lit
    {
        private static int staticIndex = 0;

        public Clr Clr { get; private set; }
        public GECEStrand Strnd { get; set; }

        public int MarqueNdx { get; set; }

        public uint MarqueMask { get; set; }

        protected MonoLit()
        {
        }

        public MonoLit(string nm, Clr clr) : base(nm, clr)
        {
        }

        public override string Attributes() => base.Attributes() + ";" + Clr.Name;

        private int last = 255;
        private int lastPgmId;
        private int lastIntenPgmId;
        private int bitndx;
        private uint bitmask;

        public int BitNdx => bitndx;
        public uint BitMask => bitmask;

        //bitndx = value >> 5;
        //bitmask = 0x80000000 >> (value & 0x1f);


        public SD.Color Coerse(SD.Color clr)
        {
            if ((clr.R | clr.G | clr.B) > 0)
                return SD.Color.FromArgb(Clr.R, Clr.G, Clr.B);
            else
                return SD.Color.Black;
        }

        public override void Set(Clr val, Pgm pgm)
        {
            //if (!pgm.Runtime.PgmDict.ContainsKey(lastPgmId)) lastPgmId = 0;
            //if (pgm.PgmId >= lastPgmId)
            //{
            //    //last = (last & 0x0ff000000) | (val & 0x000ffffff);
            //    lastPgmId = pgm.PgmId;
            //    SetInten(val, pgm);
            //}
        }

        public void SetInten(UInt32 inten, Pgm pgm)
        {
            //if (!pgm.Runtime.PgmDict.ContainsKey(lastPgmId)) lastPgmId = 0;
            //if (pgm.PgmId >= lastIntenPgmId)
            //{
            //    //last = (last & 0x000ffffff) | (inten & 0x0ff000000);
            //    lastIntenPgmId = pgm.PgmId;
            //}
        }

        //public override Level Get()
        //{
        //    int inten = last;
        //    return Level.FromArgb(Clr.R * inten / 255, Clr.G * inten / 255, Clr.B * inten / 255);
        //}

        public override Clr Get()
        {
            throw new NotImplementedException();
            //Clr clr = LoopBack.Clr.FromArgb(Clr.R * last >> 8, Clr.G * last >> 8, Clr.B * last >> 8);
            //return clr;
        }

        public override void setIndex(int value)
        {
            bitndx = value >> 5;
            bitmask = 0x80000000 >> (value & 0x1f);
        }
    }
}
