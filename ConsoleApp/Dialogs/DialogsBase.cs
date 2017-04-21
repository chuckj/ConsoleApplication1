using System;
using System.Collections.Generic;

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
