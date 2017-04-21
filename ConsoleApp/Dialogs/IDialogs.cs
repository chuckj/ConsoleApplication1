using System;
using System.Collections.Generic;

namespace ConsoleApplication1.Dialogs
{
    interface IDialogs
    {
        IEnumerable<Type> Parms { get; }
    }
}
