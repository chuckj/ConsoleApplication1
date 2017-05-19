using System;
using System.Windows.Forms;

namespace WindowsFormsDMXTest
{
    public static class Extensions
    {
        public static void Invoke(this Control control, Action action) => control.Invoke((Delegate)action);

    }
}
