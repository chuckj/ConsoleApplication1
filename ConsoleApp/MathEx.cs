using System;

namespace ConsoleApplication1
{
    public class MathEx
    {
        public static float Atan(float deltax, float deltay)
        {
            float angle = (deltax == 0) ? 90f : (float)(180 / Math.PI * Math.Atan(deltay / deltax));
            if (deltax < 0)
                angle += 180;
            if (angle < 0)
                angle += 360;
            return angle;
        }

        public static float Length(float deltay, float deltax)
        {
            return (float)Math.Sqrt(deltay * deltay + deltax * deltax);
        }
    }
}
