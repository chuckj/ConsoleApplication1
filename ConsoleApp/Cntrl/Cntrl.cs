using System;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    public class Cntrl
    {
        public static void Load(XElement root)
        {
            XElement el = root.Element("Controllers");
            foreach (XElement ctl in el.Elements())
            {
                switch (ctl.Name.LocalName.ToLower())
                {
                    case "usbcontroller":
                        Global.Instance.Cntrlrs.Add(new USBCntrl(ctl));
                        break;

                    case "treecontroller":
                        Global.Instance.Cntrlrs.Add(new TreeCntrl(ctl));
                        break;

                    default:
                        throw new Exception("Unknown controller type:" + ctl.Name.LocalName);
                }
            }
        }

        public static void SendAll(int time)
        {
            Global.Instance.Cntrlrs.ForEach(c => c.Send(time));
        }

        public virtual void Send(int time)
        {
            throw new NotImplementedException();
        }

        public static void gatherAll(int time)
        {
            Global.Instance.Cntrlrs.ForEach(c => c.gather(time));
        }

        public string Name { get; set; }

        public virtual void gather(int time)
        {
        }

        public virtual void Close()
        {
        }
    }
}
