using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    [Step("anime")]
    public class AnimatedStep : Step
    {
        private string _trgt;
        private string _transition;
        private string _reverseStr;
        private string _gapStr;
        private string _start;
        private string _stop;

        private bool _reverse;
        private int _gap;
        private int _startTime;
        private int _stopTime = 0;

        private DisplayTransition trans;

        public override Step Factory(XElement nod) => new AnimatedStep(nod, (string)nod.Attribute("display"), (string)nod.Attribute("transition"), (string)nod.Attribute("reverse"), (string)nod.Attribute("gap"), (string)nod.Attribute("start"), (string)nod.Attribute("stop"));

        public AnimatedStep(XElement nod, string trgt, string transition, string reverse, string gap, string start, string stop)
            : base(nod)
        {
            _trgt = trgt;
            _transition = transition;
            _reverseStr = reverse;
            _gapStr = gap;
            _start = start;
            _stop = stop;

            _reverse = (!string.IsNullOrEmpty(_reverseStr)) && ("true|reverse|1|t".Contains(_reverseStr));
            _gap = 0;
            if (!string.IsNullOrEmpty(_gapStr))
                if (!int.TryParse(_gapStr, out _gap))
                    throw new ArgumentException("gap is invalid:" + _gapStr);

            _startTime = editTime(_start, Global.Instance.ParseTime);
            _stopTime = editTime(_stop, _startTime);
            if (_startTime >= _stopTime)
                throw new ArgumentException("start Time must be < stop time:" + _startTime + ":" + _stopTime);

            //var elm = Global.Instance.doc.Descendants("transitions").Descendants("transition")
            //    .First(d => d.Attribute("name").Value == _transition);
            //if (elm == null)
            //    throw new ArgumentException("start Time must be < stop time:" + _startTime + ":" + _stopTime);

            if (!Global.Instance.Transitions.TryGetValue(_transition, out trans))
                throw new ArgumentException("transition is unknown:" + trans);
            if (_reverse || (_gap > 0))
            {

            }
        }


        public override int Run()
        {
            var tim = _startTime;
            var curr = Global.Instance.curr.Clone();
            var trgt = (DisplayUpdate)Eval(_trgt);
            if (!(trgt is DisplayUpdate))
                throw new Exception("Invalid target:" + _trgt);

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
