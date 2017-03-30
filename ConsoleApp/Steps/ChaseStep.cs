using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    [Step("chase")]
    public class ChaseStep : Step
    {
        private string _start;
        private string _stop;

        private int _startTime;
        private int _stopTime = 0;
        //private LvlSeq _lvlseq;

        public override Step Factory(XElement nod) => new ChaseStep(nod, (string)nod.Attribute("lvlseq"), (string)nod.Attribute("grpseq"), (string)nod.Attribute("finallvl"), (string)nod.Attribute("step"), (string)nod.Attribute("stop"), (string)nod.Attribute("runin"), (string)nod.Attribute("runout"));

        public ChaseStep(XElement nod, string lvlseq, string grpseq, string finallvl, string step, string stop, string runin, string runout) : base(nod)
        {

            _startTime = editTime(_start, Global.Instance.ParseTime);
            _stopTime = editTime(_stop, _startTime);
            if (_startTime >= _stopTime)
                throw new ArgumentException("StartTime must be < StopTime:" + _startTime + ":" + _stopTime);
        }
    }
}
