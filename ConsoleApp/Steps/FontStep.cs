using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
	[Step("font")]
	public class FontStep : Step
	{
        public override Step Factory(XElement nod) => new FontStep(nod);

        public FontStep(XElement nod)
			: base(nod)
		{
		}

        //foreach (string cmd in Global.Instance.doc.Descendants("fonts").Descendants("font")
        //	.Select(n => string.Format("f{0},{1}\r", (string)n.Attribute("char"), (string)n.Attribute("value"))))
        //{
        //	Send(cmd);
        //	ReceiveOk();
        //}

        public override int Run() => 0;
    }
}
