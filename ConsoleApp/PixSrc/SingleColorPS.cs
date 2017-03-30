using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SD = System.Drawing;


namespace ConsoleApplication1.PixSrc
{
    public class SingleColorPS : IPixSrc
    {
        public SD.Color Color { get; set; }

        public void Init(PSData psdata)
        {

        }
        public SD.Color Get(PSData PSD)
        {
            return Color;
        }
    }
}
