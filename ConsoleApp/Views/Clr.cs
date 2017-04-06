using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SD = System.Drawing;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct Clr
    {
        #region statics

        public static Clr RGBIParsex(string name)
        {
            var color = System.Drawing.Color.FromName(name);
            if ((color != System.Drawing.Color.Black) || (name == "Black")) return color;
            return Clr.Parsex(name);
        }

        public static Clr FromName(string name)
        {
            if (name.StartsWith("#"))
            {
                int val = Convert.ToInt32(name.Substring(1), 16);
                return System.Drawing.Color.FromArgb((val >> 16) & 0xff, (val >> 8) & 0xff, val & 0xff);
            }
            //if (Global.Instance.CustomColors.TryGetValue(name, out clr))
            //    return clr;
            return System.Drawing.Color.FromName(name);
        }
        #endregion


        private UInt32 Value;

        #region Constructors
        public Clr(UInt32 val)
        {
            Value = val;
        }

        public Clr(long val)
        {
            Value = (UInt32)val;
        }
        #endregion

        #region Factories
        public static Clr Parsex(string txt)
        {
            UInt32 val;
            if (!UInt32.TryParse(txt, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out val))
                throw new ArgumentException("val: " + txt);
            return new Clr(val);
        }

        public static Clr FromIrgb(int i, int r, int g, int b)
        {
            if ((i < 0) || (i > 255))
                throw new FormatException("RGBI value is invalid.");
            if ((r < 0) || (r > 255))
                throw new FormatException("RGBI value is invalid.");
            if ((g < 0) || (g > 255))
                throw new FormatException("RGBI value is invalid.");
            if ((b < 0) || (b > 255))
                throw new FormatException("RGBI value is invalid.");

            return new Clr((((((((UInt32)i << 8) | (UInt32)r) << 8) | (UInt32)g) << 8) | (UInt32)b));
        }

        public static Clr FromArgb(int r, int g, int b) =>
            new Clr(((((((UInt32)r) << 8) | (UInt32)g) << 8) | (UInt32)b) | 0xff000000);
        #endregion

        #region Properties
        public Clr Parse(string txt) => Clr.Parsex(txt);

        public byte B => (byte)((Value & 0x000000ff) >> 0); 

        public byte G => (byte)((Value & 0x0000ff00) >> 8);

        public byte R => (byte)((Value & 0x00ff0000) >> 16); 

        public byte A => (byte)(0xff); 

        public byte I => (byte)((Value & 0xff000000) >> 24);

        public string Name => Value.ToString("X");
        #endregion

        #region Conversion
        public static implicit operator UInt32(Clr lvl) => lvl.Value;

        public static implicit operator Clr(long val) => new Clr(val);

        public static implicit operator SD.Color(Clr clr)
        {
            return SD.Color.FromArgb((byte)255, clr.R, clr.G, clr.B);
        }

        public static implicit operator Vector4(Clr clr) => new Vector4(clr.R / 255.0f, clr.G / 255.0f, clr.B / 255.0f, 1);

        public static implicit operator Clr(System.Drawing.Color color) => new Clr(color.ToArgb() | 0xff000000);
        #endregion

        public UInt32 UInt => Value;

        #region Overrides

        public override bool Equals(Object obj)
        {
            if (!(obj is Clr)) return false;
            return Value == ((Clr)obj).Value;
        }

        public override int GetHashCode() => (int)Value;
        #endregion

        public string DebuggerDisplay => string.Format($"#{Value:X8}");
    }
}
