using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    public class StepTransition
    {
        public static StepTransition Factory(IEnumerable<XElement> transitions)
        {
            return new StepTransition(transitions);
        }

        public List<StepTransition_Base> Transitions { get; set; }

        public StepTransition(IEnumerable<XElement> transitions)
        {
            Transitions = new List<StepTransition_Base>();
            foreach (var child in transitions)
            {
                switch (child.Name.LocalName)
                {
                    case "position":
                        Transitions.Add(StepTransition_Position.Factory(child));
                        break;

                    case "fade":
                        Transitions.Add(StepTransition_Fade.Factory(child));
                        break;

                    case "linear":
                        Transitions.Add(StepTransition_Linear.Factory(child));
                        break;
                }
            }
        }
    }
}
