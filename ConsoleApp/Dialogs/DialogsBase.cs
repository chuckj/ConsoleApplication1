using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.Dialogs
{
    class DialogsBase : IDialogs
    {
        public IEnumerable<Type> Parms
        {
            get
            {
                yield return typeof(int);
                yield return typeof(string);
                yield break;
            }
        }
    }
}
