using System;

namespace ConsoleApplication1
{
    [AttributeUsage(AttributeTargets.Class)]
	public class StepAttribute : System.Attribute
	{
		public readonly string XmlName;

		public StepAttribute(string xmlname)
		{
			this.XmlName = xmlname;
		}
	}
}
