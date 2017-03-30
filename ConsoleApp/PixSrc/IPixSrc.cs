using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SD = System.Drawing;

namespace ConsoleApplication1.PixSrc
{
    public interface IPixSrc
    {
        void Init(PSData psdata);
        SD.Color Get(PSData psdata);
    }
}
