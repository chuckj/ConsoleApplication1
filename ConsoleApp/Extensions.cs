using SharpDX;
using System;
using System.Collections.Generic;
//using System.Drawing;
using SD = System.Drawing;

namespace ConsoleApplication1
{
    public static class Extensions
    {
        //public static void DrawRectangle(this Graphics dc, Pen pen, RectangleF rectf)
        //{
        //    dc.DrawRectangle(pen, rectf.Left, rectf.Top, rectf.Width, rectf.Height);
        //}

        public static int FindFirstIndexGreaterThanOrEqualTo<T, U>(this SortedList<T, U> sortedList, T key) => BinarySearch(sortedList.Keys, key);

        private static int BinarySearch<T>(IList<T> list, T value)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            var comp = Comparer<T>.Default;
            int lo = 0, hi = list.Count - 1;
            while (lo < hi)
            {
                int m = lo + (hi - lo) / 2;  // this might overflow; be careful.
                if (comp.Compare(list[m], value) < 0) lo = m + 1;
                else hi = m - 1;
            }
            if (comp.Compare(list[lo], value) < 0) lo++;
            return lo;
        }

        public static Point Add(this Point pt, Size2 sz) => new Point(pt.X + sz.Width, pt.Y + sz.Height);

        public static SD.Color Fade(this SD.Color left, SD.Color right, float factor)
        {
            var _factor = 1 - factor;
            return SD.Color.FromArgb((int)(left.A * _factor + right.A * factor), 
                (int)(left.R * _factor + right.R * factor), 
                (int)(left.G * _factor + right.G * factor), 
                (int)(left.B * _factor + right.B * factor));
        }

        public static SD.Color Cross(this SD.Color left, SD.Color right) =>
            SD.Color.FromArgb(left.A * right.A / 255, left.R * right.R / 255, left.G * right.G / 255, left.B * right.B / 255);
    }
}
