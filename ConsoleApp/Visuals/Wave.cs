using Microsoft.ConcurrencyVisualizer.Instrumentation;
using SharpDX;
using SharpDX.Direct2D1;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Wave : Viz
	{
        public override string DebuggerDisplay => $"Wave: {base.DebuggerDisplay}";

        private static WaveRes resources;

        static Wave()
        {
            resources = new WaveRes();
        }

        public override void ResetPoints(Song song)
		{
		}

        public override void Draw(DrawData dd)
        {
            //	draw wave data
            byte[] wavedata = dd.Song.WaveformData;

            if (wavedata != null)
            {
#if (MARKERS)
                Markers.WriteFlag("Wave");
#endif

                int fnd = -1;
                for (int ndx = dd.LFT / Global.pxpersec; ndx < (dd.RIT + Global.pxpersec - 1) / Global.pxpersec; ndx++)
                {
                    if (resources.Wave_Bmps[ndx] == null)
                    {
                        fnd = ndx;
                        break;
                    }
                }

                if (fnd == -1)
                {
                    for (int ndx = 0; ndx < resources.Wave_Bmps.Length; ndx++)
                    {
                        if (resources.Wave_Bmps[ndx] == null)
                        {
                            fnd = ndx;
                            break;
                        }
                    }
                }

                if (fnd != -1)
                {

                    var ht = Global.Wave_Height;
                    var half = ht >> 1;

                    using (var renderT = new SharpDX.Direct2D1.BitmapRenderTarget(dd.target, CompatibleRenderTargetOptions.None, new Size2F(Global.pxpersec, ht), 
                        null, new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)))
                    {
                        renderT.BeginDraw();
                        renderT.Clear(SharpDX.Color.Black);

                        using (var path = new PathGeometry(Global.Instance.factory2D1))
                        {
                            var sink = path.Open();
                            sink.BeginFigure(new Vector2(Global.pxpersec, half + 1), FigureBegin.Hollow);
                            sink.AddLine(new Vector2(0, half + 1));
                            for (int ndx = 0, src = fnd * 2 * Global.pxpersec; ndx < Global.pxpersec && src < wavedata.Length; ndx++, src += 2)
                            {
                                sink.AddLine(new Vector2(ndx, half + 1 + wavedata[src]));
                            }
                            sink.EndFigure(FigureEnd.Open);
                            sink.Close();

                            renderT.DrawGeometry(path, resources.Wave_RedBrush);
                        }

                        using (var path = new PathGeometry(Global.Instance.factory2D1))
                        {
                            var sink = path.Open();
                            sink.BeginFigure(new Vector2(Global.pxpersec, half - 1), FigureBegin.Hollow);
                            sink.AddLine(new Vector2(0, half - 1));
                            for (int ndx = 0, src = fnd * 2 * Global.pxpersec + 1; ndx < Global.pxpersec && src < wavedata.Length; ndx++, src += 2)
                            {
                                sink.AddLine(new Vector2(ndx, half - 1 - wavedata[src]));
                            }
                            sink.EndFigure(FigureEnd.Open);
                            sink.Close();

                            renderT.DrawGeometry(path, resources.Wave_GreenBrush);
                        }

                        renderT.EndDraw();

                        resources.Wave_Bmps[fnd] = renderT.Bitmap;
                    }
                }


                for (int ndx = dd.LFT / Global.pxpersec, bgn = ndx * Global.pxpersec; ndx < resources.Wave_Bmps.Length && bgn <= dd.RIT; ndx++, bgn += Global.pxpersec)
                {
                    var bmp = resources.Wave_Bmps[ndx];
                    if (bmp == null) break;

                    dd.target.DrawBitmap(bmp, new RectangleF(bgn, Global.Wave_Channels, Global.pxpersec, Global.Wave_Height), 1.0f, BitmapInterpolationMode.NearestNeighbor,
                        new RectangleF(0, 0, Global.pxpersec, Global.Wave_Height));
                }

#if (MARKERS)
                Markers.WriteFlag("WaveEnd");
#endif
            }
        }
	}
}
