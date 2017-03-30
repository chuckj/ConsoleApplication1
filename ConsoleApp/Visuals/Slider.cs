using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;

namespace ConsoleApplication1
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Slider : Viz
    {
        private static SliderRes resources;

        static Slider()
        {
            resources = new SliderRes();
        }

        public override string DebuggerDisplay => $"Slider: {base.DebuggerDisplay}";

		public Slider()
		{
		}

        public override VizFeatures Features => VizFeatures.None;

        public override void ResetPoints(Song song)
		{
        }

        public override void Draw(DrawData dd)
		{

            //  save transform and install identity

            var savTrans = dd.target.Transform;
            dd.target.Transform = Matrix3x2.Identity; 
            var scale = (float)dd.Width / dd.Song.TrackPx;

            //  draw slider and thumb
            int ht = Global.Slider_Height;
            int top = dd.Height;

            //  body
            dd.target.DrawRectangle(new RectangleF(0, top, dd.Width, ht - 1), resources.Slider_SliderBrush);
            
            //  loop begin and end
            var pt = ((float)dd.Song.LoopBeginPx);
            dd.target.DrawLine(new Vector2(pt * scale, top), new Vector2(pt * scale, top + ht), resources.Slider_LoopBeginBrush);
            pt = ((float)dd.Song.LoopEndPx);
            dd.target.DrawLine(new Vector2(pt * scale, top), new Vector2(pt * scale, top + ht), resources.Slider_LoopEndBrush);

            ////  cursors
            pt = ((float)dd.Offset);
            dd.target.DrawLine(new Vector2(pt * scale, top), new Vector2(pt * scale, top + ht), resources.SliderCursor_CursorBrush);

            //  restore transform

            dd.target.Transform = savTrans;

            pt = dd.Offset;
            dd.target.DrawLine(new Vector2(pt, 1), new Vector2(pt, top - 1), resources.SliderCursor_CursorBrush);
        }
    }
}
