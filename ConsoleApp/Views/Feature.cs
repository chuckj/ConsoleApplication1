using System;
using System.Xml.Linq;

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
                Clr clr = Clr.FromName((string)color.Attribute("color"));
                new FeatureLit(nm, clr);
            }
        }
    }
}
