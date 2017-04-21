using System;

namespace ConsoleApplication1
{
    public class Update : IComparable<Update>
    {
        private int time;

        public int Time { get { return time; }  set { time = value; } }

        public int CompareTo(Update otherUpdate)
        {

            // If other is not a valid object reference, this instance is greater.
            if (otherUpdate == null) return 1;

            return this.time.CompareTo(otherUpdate.time);
        }
    }
}
