using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class FontUpdate : Update
    {
        public uint[] Bits = new uint[16];

        public static FontUpdate FromString(string txt)
        {
            var font = new FontUpdate();
            if (!txt.Contains(","))
            {
                for (int ndx = 0; ndx < 16; ndx++)
                {
                    font.Bits[ndx] = uint.Parse(txt.Substring(ndx * 2, 2), NumberStyles.HexNumber);
                }
            }
            else
            {
                string[] vals = txt.Split(',');
                for (int ndx = 0; ndx < 16; ndx++)
                {
                    font.Bits[ndx] = uint.Parse(vals[ndx]);
                }
            }
            return font;
        }
    }
}
