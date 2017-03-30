using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    public class Player
    {
        //enum ColorEnum { Red, Yellow, Green, Blue };
        //static Color[] Colors = new[] { Color.Red, Color.Yellow, Color.ForestGreen, Color.Blue };


        public async void Runner()
        {
            //Display disp;
            //while (Global.Instance.readyList.TryTake(out disp))
            //{
            //    disp.Return();
            //}
            //if (Global.Instance.curr != null)
            //{
            //    Global.Instance.curr.Return();
            //    Global.Instance.curr = null;
            //}

            //Global.Instance.doc = XDocument.Load(@".\\..\\..\\XMLfile1.xml");

            ////  Displays

            //Global.Instance.dta = Global.Instance.doc.Descendants("lites").Descendants("lite").Select(n => new TreeData()
            //{
            //    row = (int)n.Attribute("row"),
            //    ctr = (int)n.Attribute("cir"),
            //    col = (int)n.Attribute("col"),
            //    ndx = (int)n.Attribute("ndx"),
            //    color = Colors[(int)(ColorEnum)Enum.Parse(typeof(ColorEnum), (string)n.Attribute("color"))],
            //    marqueNdx = string.IsNullOrEmpty((string)n.Attribute("mrqrow")) ? -1 : (int)n.Attribute("mrqrow"),
            //    marqueMsk = (uint)(string.IsNullOrEmpty((string)n.Attribute("mrqcol")) ? 0 : (1 << (int)n.Attribute("mrqcol"))),
            //}).OrderBy(t => t.ndx).ToArray();

            //Global.Instance.tdOrder = Global.Instance.doc.Descendants("lites").Descendants("lite").Select(n => (short)n.Attribute("ndx")).ToArray();
            
            //Global.Instance.dict = Global.Instance.dta.ToDictionary(d => Tuple.Create<int, int>(d.row, d.ctr), d => d);

            //// Transitions

            //Global.Instance.Transitions = new Dictionary<string, DisplayTransition>()
            //{ {"topdown", DisplayTransition.FromEnumerable(Global.Instance.dta.Select(x => (short)x.row)) } };

            //foreach (XElement trans in Global.Instance.doc.Descendants("transitions").Descendants("transition"))
            //{
            //    Global.Instance.Transitions.Add((string)trans.Attribute("name"), DisplayTransition.FromString((string)trans.Attribute("value")));
            //}

            ////  Font

            //foreach (XElement felm in Global.Instance.doc.Descendants("transitions").Descendants("transition"))
            //{
            //    Global.Instance.FontDict.Add(((string)felm.Attribute("char"))[0], FontUpdate.FromString((string)felm.Attribute("value")));
            //}


            //Global.Instance.curr = Global.Instance.currDisplay = DisplayUpdate.Get();
            //Global.Instance.ParseTime = 0;
            //Console.WriteLine("***: " + Global.Instance.ParseTime);
            Global.Instance.TimeReset();


            try
            {
                while (!Global.Instance.Tkn.IsCancellationRequested && (Global.Instance.readyList.Count > 0))
                {
                    var disp = Global.Instance.readyList[0];
                    Global.Instance.readyList.RemoveAt(0);
                    int wait = disp.Time - Global.Instance.RealTime;
                    if (wait > 0)
                    {
                        await Task.Delay(wait, Global.Instance.Tkn);
                    }

                    if (disp is DisplayUpdate)
                        Global.Instance.currDisplay = (DisplayUpdate)disp;
                    if (disp is MarqueUpdate)
                    {
                        var mrq = (MarqueUpdate)disp;
                        if ((mrq.Bits[0] | mrq.Bits[1] | mrq.Bits[2] | mrq.Bits[3] | mrq.Bits[4] | mrq.Bits[5] | mrq.Bits[6] | mrq.Bits[7] 
                            | mrq.Bits[8] | mrq.Bits[9] | mrq.Bits[10] | mrq.Bits[11] | mrq.Bits[12] | mrq.Bits[13] | mrq.Bits[14] | mrq.Bits[15]) == 0)
                            Global.Instance.currMarque = null;
                        else
                            Global.Instance.currMarque = mrq;

                    }
                    Global.Instance.Updated = true;
                }
            }
            catch (OperationCanceledException e)
            {
            }
        }
    }
}
