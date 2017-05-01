using System;

namespace ConsoleApplication1
{
    public class MathEx
    {
        public static float Atan(float x, float y)
        {
            float angle = (x == 0) ? 90f : (float)(180 / Math.PI * Math.Atan(y / x));
            if (x < 0)
                angle += 180;
            if (angle < 0)
                angle += 360;
            return angle;
        }

        public static int Length(float x, float y)
        {
            return Convert.ToInt32(Math.Sqrt(y * y + x * x));
        }
    }
}
