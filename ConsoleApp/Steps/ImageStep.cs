using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    [Step("image")]
    public class ImageStep : Step
    {
        private string _trgtStr;
        private string _transitionStr;
        private string _gapStr;
        private string _startStr;
        private string _stopStr;

        private bool _reverse;
        private int _gap;
        private int _startTime;
        private int _stopTime = 0;

        private DisplayTransition trans;

        public override Step Factory(XElement nod) => new AnimatedStep(nod, (string)nod.Attribute("display"), (string)nod.Attribute("transition"), (string)nod.Attribute("reverse"), (string)nod.Attribute("gap"), (string)nod.Attribute("start"), (string)nod.Attribute("stop"));

        public ImageStep(XElement nod, string trgt, string transition, string reverse, string gap, string start, string stop)
            : base(nod)
        {
            _trgtStr = trgt;
            _transitionStr = transition;
            _gapStr = gap;
            _startStr = start;
            _stopStr = stop;

            _reverse = (!string.IsNullOrEmpty(reverse)) && ("true|reverse|1|t".Contains(reverse));
            _gap = 0;
            if (!string.IsNullOrEmpty(_gapStr))
                if (!int.TryParse(_gapStr, out _gap))
                    throw new ArgumentException("gap is invalid:" + _gapStr);

            _startTime = editTime(_startStr, Global.Instance.ParseTime);
            _stopTime = editTime(_stopStr, _startTime);
            if (_startTime >= _stopTime)
                throw new ArgumentException("start Time must be < stop time:" + _startTime + ":" + _stopTime);

            //var elm = Global.Instance.doc.Descendants("transitions").Descendants("transition")
            //    .First(d => d.Attribute("name").Value == _transition);
            //if (elm == null)
            //    throw new ArgumentException("start Time must be < stop time:" + _startTime + ":" + _stopTime);

            if (!Global.Instance.Transitions.TryGetValue(_transitionStr, out trans))
                throw new ArgumentException("transition is unknown:" + trans);
            if (_reverse || (_gap > 0))
            {

            }
        }


        public override int Run()
        {
            var tim = _startTime;
            var curr = Global.Instance.curr.Clone();
            var trgt = (DisplayUpdate)Eval(_trgtStr);
            if (!(trgt is DisplayUpdate))
                throw new Exception("Invalid target:" + _trgtStr);

            var steps = trans.Steps.Max();
            var queue = new List<DisplayUpdate>();
            var intr = steps == 0 ? 0 : ((_stopTime - _startTime) / (steps));
            var prv = Global.Instance.curr;
            var dta = Global.Instance.dta;

            for (int stp = -_gap; stp < steps; stp++)
            {
                var nxt = prv.Clone();

                for (int ndx = 0; ndx < 307; ndx++)
                {
                    var stpno = trans.Steps[ndx];
                    if (_reverse) stpno = (short)(steps - stpno);
                    if (stpno == stp)
                        nxt[dta[ndx]] = trgt[dta[ndx]];
                    else if ((_gap > 0) && (stpno - stp == _gap))
                        nxt[dta[ndx]] = false;
                }

                nxt.Time = tim;
                tim += intr;
                queue.Add(nxt);
                prv = nxt;
            }
            curr.Return();

            trgt.Time = tim;
            queue.Add(trgt);

            return PlayUpdates(queue);
        }
    }
}
