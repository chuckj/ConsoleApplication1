using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USB_DMX_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //USBController nm = "USB1" serial = "A6FOL1K7";
            //USBController nm = "USB2" serial = "A6FOL1FD";
            //USBController nm = "USB3" serial = "A6F0L1FD";

            var cntrl = new USBCntrl("USB1", "A6FOL1K7");

        }
    }
}
