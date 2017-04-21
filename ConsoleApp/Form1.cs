//using dw = SharpDX.DirectWrite;
using System;
using System.Drawing;
using System.Windows.Forms;
using dx = SharpDX.DXGI;

namespace ConsoleApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            init();
        }

        ~Form1()
        {
            //if (bmp != null) bmp.Dispose();
            //if (rt != null) rt.Dispose();
            //if (factory != null) factory.Dispose();
            //if (factoryDwrite != null) factoryDwrite.Dispose();
        }

        private void init()
        { 
            InitializeComponent();

            //SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            Screen[] scrns = Screen.AllScreens;
            Rectangle bounds;
            bool fnd = false;
            foreach (Screen scrn in scrns)
            {
                if (!scrn.Primary)
                {
                    bounds = scrn.WorkingArea;
                    this.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    this.StartPosition = FormStartPosition.Manual;
                    fnd = true;
                    break;
                }
            }
            if (!fnd)
            {
                bounds = Screen.PrimaryScreen.WorkingArea;
                this.SetBounds(bounds.Width / 2, bounds.Y, bounds.Width / 2, bounds.Height);
                this.StartPosition = FormStartPosition.Manual;
            }
        }

       // private Factory factory = null;
        //private dw.Factory factoryDwrite = null;
        private int count = 0;
        private DateTime begin;
        //private WindowRenderTarget rt = null;
        //private BitmapRenderTarget bmp = null;
        //private Device device = null;
        private dx.Device dxdevice = null;

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            pictureBox1.Invalidate();
        }
        private void paint(Control sender, PaintEventArgs e)
        {
            //if (factory == null)
            //{
            //    factory = new Factory(FactoryType.SingleThreaded);
            //    factoryDwrite = new dw.Factory();

            //}

            //if (dxdevice == null)
            //{
            //    dxdevice = new dx.Device()
            //}

            //if (device == null)
            //{
            //    device = new Device(factory, )
            //}

            //if (rt == null)
            //{
            //    var rtp = new RenderTargetProperties(); // new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied));
                
            //    var hrtp = new HwndRenderTargetProperties();
            //    hrtp.Hwnd = sender.Handle;
            //    hrtp.PixelSize = new SharpDX.Size2(sender.ClientSize.Width, sender.ClientSize.Height);
                
            //    rt = new WindowRenderTarget(factory, rtp, hrtp);

            //    bmp = new BitmapRenderTarget(rt, CompatibleRenderTargetOptions.None);

            //    begin = DateTime.Now;
            //}

            //bmp.BeginDraw();
            //    bmp.Clear(new SharpDX.Mathematics.Interop.RawColor4(1.0f, 0.0f, 0.0f, 1.0f));

            //    var v2 = new SharpDX.Mathematics.Interop.RawVector2();
            //    v2.X = 100.0F;
            //    v2.Y = 200.0f;

            //using (var brush = new SharpDX.Direct2D1.SolidColorBrush(bmp, new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 1.0f, 1.0f)))
            //{


            //    var rnd = new Random();

            //    for (int i = 0; i < 1000; i++)
            //    {
            //        brush.Color = new SharpDX.Mathematics.Interop.RawColor4((rnd.Next() % 100) / 100.0f, (rnd.Next() % 100) / 100.0f, (rnd.Next() % 100) / 100.0f, 1.0f);
            //        v2.X = rnd.Next() % sender.Width;
            //        v2.Y = rnd.Next() % sender.Height;

            //        bmp.FillEllipse(new Ellipse(v2, 5, 5 /*rnd.Next() % 100, rnd.Next() % 100*/), brush);
            //    }
            //}

            //using (var txtfmt = new dw.TextFormat(factoryDwrite, "Calibri", 30) { TextAlignment = dw.TextAlignment.Leading, ParagraphAlignment = dw.ParagraphAlignment.Near })
            //using (var SceneColorBrush = new SharpDX.Direct2D1.SolidColorBrush(bmp, new SharpDX.Mathematics.Interop.RawColor4(0.0f, 0.0f, 0.0f, 1.0f)))
            //{
            //    var ClientRectangle = new SharpDX.Mathematics.Interop.RawRectangleF(0, 0, sender.ClientRectangle.Width / 2, sender.ClientRectangle.Height / 2);
            //    bmp.DrawText("Hello Marshall", txtfmt, ClientRectangle, SceneColorBrush);
            //}

            //bmp.EndDraw();


            //rt.BeginDraw();

            //rt.DrawBitmap(bmp.Bitmap, 1.0f, BitmapInterpolationMode.Linear);

            //rt.EndDraw();

            ////pictureBox1.Image = bmp.Bitmap;

            //count++;
            //if ((count % 100) == 0)
            //    Console.WriteLine(count.ToString() + ":" + ((DateTime.Now - begin).TotalSeconds / count));
            //this.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            resize();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            resize();

        }
        private void resize()
        {
            //if (bmp != null) bmp.Dispose();
            //bmp = null;
            //if (rt != null) rt.Dispose();
            //rt = null;
        }

       

        private void pictureBox1_Paint_1(object sender, PaintEventArgs e)
        {
            paint((Control)sender, e);
        }
    }
}
