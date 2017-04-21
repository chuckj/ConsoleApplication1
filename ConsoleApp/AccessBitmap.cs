using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace ConsoleApplication1
{
    public class AccessBitmap
    {
        public static BitMapImg ToArray(Bitmap processedBitmap)
        {
            BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadOnly, processedBitmap.PixelFormat);
            int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInPixels = bitmapData.Width;

            BitMapImg bmi = new BitMapImg();
            List<Color> colors = new List<Color>();
            bmi.Indices = new byte[heightInPixels, widthInPixels];
            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;
                int ndx;
                for (int y = 0; y < heightInPixels; y++)
                {
                    int i =  y * bitmapData.Stride;
                    for (int x = 0; x < widthInPixels; x++)
                    {
                        Color clr = Color.FromArgb((int)(0xff000000 | ((uint)ptr[i++]) | ((uint)ptr[i++] << 8) | ((uint)ptr[i++] << 16)));
                        if ((ndx = colors.IndexOf(clr)) == -1)
                        {
                            ndx = colors.Count();
                            colors.Add(clr);
                        }
                        bmi.Indices[y, x] = (byte)ndx;
                    }
                }
                processedBitmap.UnlockBits(bitmapData);
            }
            bmi.Pixels = new PixelList();
            bmi.Pixels.Pixels = colors.ToArray();

            return bmi;
        }
    }
}
