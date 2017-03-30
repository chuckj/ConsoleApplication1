using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Threading.Tasks;
using SD = System.Drawing;

namespace ConsoleApplication1
{
    [Step("stepanim")]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class StepAnimated : StepBase, IRunTime
    {
        #region Statics
        static StepAnimated()
        {
        }
        #endregion

        #region Debugger
        public override string DebuggerDisplay => $"StepAnim: {base.DebuggerDisplayShort}";
        #endregion

        #region Ctors
        public override Viz Factory(XElement xml, Song song) => new StepMessage(xml, song);

        public StepAnimated(XElement xml, Song song) : base(xml, song)
        {
            var val = (string)xml.Attribute("steptime");
        }
        #endregion

        private RunTime context;

        public IEnumerable<int> Xeq(RunTime context)
        {
            //this.context = context;

            //var trgt = (DisplayUpdate)Eval(_trgt);
            //if (!(trgt is DisplayUpdate))
            //    throw new Exception("Invalid target:" + _trgt);

            //var steps = trans.Steps.Max();
            //var queue = new List<DisplayUpdate>();
            //var intr = steps == 0 ? 0 : ((_stopTime - _startTime) / (steps));
            //var prv = Global.Instance.curr;
            //var dta = Global.Instance.dta;

            //for (int stp = -_gap; stp < steps; stp++)
            //{
            //    var nxt = prv.Clone();

            //    for (int ndx = 0; ndx < 307; ndx++)
            //    {
            //        var stpno = trans.Steps[ndx];
            //        if (_reverse) stpno = (short)(steps - stpno);
            //        if (stpno == stp)
            //            nxt[dta[ndx]] = trgt[dta[ndx]];
            //        else if ((_gap > 0) && (stpno - stp == _gap))
            //            nxt[dta[ndx]] = false;
            //    }

            //    nxt.Time = tim;
            //    tim += intr;
            //    queue.Add(nxt);
            //    prv = nxt;
            //}
            //curr.Return();

            //trgt.Time = tim;
            //queue.Add(trgt);

            //return PlayUpdates(queue);
            //yield return context.CurTime + steptime;


            int[] bgnndx = Global.Instance.LitDict.Values.OfType<Lit>()
                .Where(v => v.Name.StartsWith(text))
                .OrderByDescending(v => v.Row).ThenBy(v => v.Column)
                .Select(v => v.GlobalIndex).ToArray();

            if (mode == 1)
            {
                int intr = (EndPoint.X - StartPoint.X) / 20;
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 11; col++)
                        context[bgnndx[row * 11 + col]] = array[row, col];

                    yield return StartPoint.X + intr * (row + 1);
                }
            }
            else
            {
                int thrds = (EndPoint.X - StartPoint.X) / 3;
                int intr = thrds / 10;
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 11; col++)
                        context[bgnndx[row * 11 + col]] = array[row, col];

                    yield return context.CurTime + intr;
                }

                yield return StartPoint.X + thrds;

                int pre = 0;
                int nxt = 11;
                int starttime = context.CurTime;
                int endtime = context.CurTime + thrds;
                while (context.CurTime < endtime)
                {
                    float fraction = 2 * (context.CurTime - starttime) / (float)thrds;
                    if (fraction > 1) break;
                    for (int row = 0; row < 9; row++)
                    {
                        for (int col = 0; col < 11; col++)
                            context[bgnndx[row * 11 + col]] = factor(array[row, col + pre], array[row, col + nxt], fraction);
                    }
                    yield return context.CurTime + 4;
                }

                yield return endtime;

                pre = 11;
                nxt = 22;
                starttime = context.CurTime;
                endtime = EndPoint.X;
                while (context.CurTime < endtime)
                {
                    float fraction = 2 * (context.CurTime - starttime) / (float)thrds;
                    if (fraction > 1) break;
                    for (int row = 0; row < 9; row++)
                    {
                        for (int col = 0; col < 11; col++)
                            context[bgnndx[row * 11 + col]] = factor(array[row, col + pre], array[row, col + nxt], fraction);
                    }
                    yield return context.CurTime + 4;
                }
            }

            yield return EndPoint.X;

            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 11; col++)
                    context[bgnndx[row * 11 + col]] = System.Drawing.Color.Black;
        }

        private SD.Color factor(SD.Color bgn, SD.Color end, float fraction)
        {
            fraction = Math.Max(Math.Min(fraction, 1), 0);
            int red = Math.Min((int)(((uint)bgn.R) * (1 - fraction) + ((uint)end.R) * fraction), 255);
            int green = Math.Min((int)(((uint)bgn.G) * (1 - fraction) + ((uint)end.G) * fraction), 255);
            int blue = Math.Min((int)(((uint)bgn.B) * (1 - fraction) + ((uint)end.B) * fraction), 25);
            return SD.Color.FromArgb(red, green, blue);
        }
    }
}
