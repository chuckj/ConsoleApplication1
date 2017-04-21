using System.Diagnostics;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Vari
    {
        public string VarName { get; protected set;}

        public Vari(string varname)
        {
	  VarName = varname;
        }

        private string DebuggerDisplay => string.Format("{0}:", VarName);

        public virtual object BoxedValue => null;
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Vari<T> : Vari
    {
        public Vari(string varname) : base(varname)
        {

        }

        public T Value { get; set; }

        private string DebuggerDisplay => string.Format("{0}:{1}", VarName, Value.ToString());

        public override string ToString() => Value.ToString();

        public override object BoxedValue => (object)Value;
    }
}
