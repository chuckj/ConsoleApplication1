using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    public class GECEStrand : Strand
    {

        private Clr[] lstLvls = new Clr[64];

        public string Name { get; set; }
        public int Port { get; set; }
        public int Index;
        public USBCntrl Ctl = null;

        public GECEStrand(XElement el, USBCntrl ctl)
        {
            Name = (string)el.Attribute("nm");
            Global.Instance.StrandDict.Add(Name, this);
            lites = new List<RGBLit>(64);

            Port = (int)el.Attribute("port");

            Global.Instance.Strands.Add(this);

            Ctl = ctl;

            Index = Global.Instance.Strands.Count - 1;

            //if (el.Attribute("lites") != null)
            //{
            //    lites.AddRange(from l in Enumerable.Range(0, (int)el.Attribute("lites")) select new RGBILit(Name + "_" + l.ToString()));
            //}
            //else
            //{
            //    lites.AddRange(from l in el.Elements("lite") select new RGBILit(l));
            //}
            //prv = new int[lites.Count];
        }


        public bool gather(USBMsg msg, int tim)
        {
            int ptr = 8;
            if ((tim != 0) && (lites.Count > 0))
            {
                UInt32 inten = lites[0].Get() & 0x0ff000000;
                bool doit = false;
                for (int ndx = 0; ndx < lites.Count; ndx++)
                {
                    Clr newlvl = lites[ndx].Get();
                    Clr oldlvl = lstLvls[ndx];
                    if ((newlvl & 0x0ff000000) != inten)
                    {
                        doit = false;
                        break;
                    }
                    if (((newlvl & 0x000ffffff) == (oldlvl & 0x000ffffff)) && ((newlvl & 0x0ff000000) != (oldlvl & 0x0ff000000)))
                        doit = true;
                }

                if (doit)
                {
                    msg.msg[ptr++] = (byte)0;
                    msg.msg[ptr++] = (byte)0;
                    UInt32 intn = inten >> 24;
                    msg.msg[ptr++] = (byte)((intn > 204) ? 204 : intn);
                    msg.msg[ptr++] = (byte)63;

                    for (int ndx = 0; ndx < lites.Count; ndx++)
                    {
                        lstLvls[ndx] = (lstLvls[ndx] & 0x000ffffff) | inten;
                    }
                }
            }


            for (int ndx = 0; ndx < lites.Count; ndx++)
            {
                Clr lvl = lites[ndx].Get();

                if ((tim == 0) || (lvl != lstLvls[ndx]))
                {
                    lstLvls[ndx] = lvl;

                    msg.msg[ptr++] = (byte)(((lvl.R >> 4) & 0x0f) + (lvl.G & 0xf0));
                    msg.msg[ptr++] = (byte)((lvl.B >> 4) & 0x0f);
                    byte i = lvl.I;
                    msg.msg[ptr++] = (i > 212) ? (byte)212 : i;
                    msg.msg[ptr++] = (byte)(ndx);
                }
            }
            msg.size = ptr;
            return (ptr > 8);
        }



        //public bool gather(Snapshot snap, USBMsg msg, int timndx)
        //{
        //    int ptr = 8;
        //    for (int ndx = 0; ndx < lites.Count; ndx++)
        //    {
        //        Lit lit = lites[ndx];

        //        Clr lvl = snap.bufr[lit.SnapshotRow, timndx];
        //        if ((timndx == 0) || (snap.bufr[lit.SnapshotRow, timndx - 1].Value != lvl.Value))
        //        {
        //            msg.msg[ptr++] = (byte)(((lvl.R >> 4) & 0x0f) + (lvl.G & 0xf0));
        //            msg.msg[ptr++] = (byte)((lvl.B >> 4) & 0x0f);
        //            byte i = lvl.I;
        //            msg.msg[ptr++] = (i > 212) ? (byte)212 : i;
        //            msg.msg[ptr++] = (byte)(ndx);
        //        }
        //    }
        //    msg.msg[2] = (byte)(ptr >> 2);
        //    return (ptr > 8);
        //}


        //public void gather()
        //{
        //    Console.WriteLine("Stand:" + Name);

        //    for (int ndx = 0; ndx < lites.Count; ndx++)
        //    {
        //        throw new Exception("not implemented");
        //        Lit lit = lites[ndx];
        //        int tmp = 0; //lites[ndx].
        //        //if (tmp != prv[ndx])
        //        //{
        //        //    Console.WriteLine("set:" + ndx.ToString() + ":" + lit.Name + "=" + tmp.ToString());
        //        //    prv[ndx] = tmp;
        //        //}
        //    }
        //    Console.WriteLine(".");
        //} 
    }
}
