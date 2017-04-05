using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Colors = System.Drawing.Color;

namespace ConsoleApplication1
{
    public class RGBLit : Lit
    {
        private static int staticIndex = 307;

        public string name;

        protected RGBLit()
        {
        }

        public RGBLit(string nm)
            : base(nm)
        {
        }

        public RGBLit(XElement xpn)
            : base((string)xpn.Attribute("nm"))
        {
        }


        public Clr[] Program { get; set; }
        private Clr last = Colors.Black;
        private int lastPgmId;
        private int lastIntenPgmId;

        public Clr Clr;

        public Strand Strnd { get; set; }

        public override string Attributes() => base.Attributes();

        public override Clr Parse(string txt) => Clr.RGBIParsex(txt);

        public override void Set(Clr val, Pgm pgm)
        {
            //if (!pgm.Runtime.PgmDict.ContainsKey(lastPgmId)) lastPgmId = 0;
            //if (pgm.PgmId >= lastPgmId)
            //{
            //    last = (last & 0x0ff000000) | (val & 0x000ffffff);
            //    lastPgmId = pgm.PgmId;
            //    SetInten(val, pgm);
            //}
        }

        public void SetInten(UInt32 inten, Pgm pgm)
        {
            //if (!pgm.Runtime.PgmDict.ContainsKey(lastPgmId)) lastPgmId = 0;
            //if (pgm.PgmId >= lastIntenPgmId)
            //{
            //    last = (last & 0x000ffffff) | (inten & 0x0ff000000);
            //    lastIntenPgmId = pgm.PgmId;
            //}
        }

        public override Clr Get() => last;
    }
}
