using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Drawing;

namespace ConsoleApplication1
{
    public class Feature
    {
        public static void Load(XElement root)
        {
            foreach (XElement color in root.Element("Features").Elements("Feature"))
            {
                var nm = (string)color.Attribute("nm");
                if ((nm == null) || Global.Instance.FeatureLitDict.ContainsKey(nm))
                    throw new ArgumentException("nm");
                if ((string)color.Attribute("color") == null)
                    throw new ArgumentException("color");
                FeatureLit.FromName((string)color.Attribute("color"));
            }
        }
    }
}
