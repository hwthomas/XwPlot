//
// RenderingTest.cs
//
// Author: Hywel Thomas <hywel.w.thomas@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Xwt;
using Xwt.Drawing;
using XwPlot;

namespace Samples
{
	public class RenderingTest: VBox
	{
		public RenderingTest ()
		{
			Button run = new Button ("Run Timing Test");
			PackStart (run);

			var st = new DrawingTest ();
			PackStart (st);

			run.Clicked += delegate {
				run.Sensitive = false;
				st.StartTest ();
			};

			st.TestFinished += delegate {
				run.Sensitive = true;
				string results = string.Format ("Draw: {0} FPS\nBitmap: {1} FPS\nVector image: {2}",
					 st.DrawFPS, st.BitmapFPS, st.ImageFPS);
				Console.WriteLine (results);
			};
		}
	}

	class DrawingTest: Canvas
	{
		bool testMode = false;

		ImageBuilder ib;
		Image vectorImage;
		Image bitmap;

		int testTime = 1000;
		// 'Focus' drawing size and centre
		double size = 64;
		double centre = 32;

		public int DrawFPS { get; private set; }
		public int BitmapFPS { get; private set; }
		public int ImageFPS { get; private set; }

		public event EventHandler TestFinished;

		public DrawingTest () : base ()
		{
			// Canvas size requests
			WidthRequest = 400;
			HeightRequest = 400;

			Point p = new Point (centre, centre);
			ib = new ImageBuilder (size, size);
			// Draw off-screen images and convert to bitmap/vector
			ib.Context.SetColor (Colors.Green);
			DrawFocus (ib.Context, p);
			bitmap = ib.ToBitmap ();
			ib.Context.SetColor (Colors.Blue);
			DrawFocus (ib.Context, p);
			vectorImage = ib.ToVectorImage ();
		}

		public void StartTest ()
		{
			testMode = true;
			// Invalidate canvas 
			QueueDraw ();
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			Point p = new Point (centre, centre);
			DrawFocus (ctx, p);
			if (!testMode)
				return;

			// Simplest drawing - direct to Canvas context
			ctx.SetColor (Colors.Red);
			DrawFPS = TimedAction (delegate {
				DrawFocus (ctx, p);
			});

			// Check timings for copying off-screen images
			// Destination on canvas may be changed
			Rectangle srcRect = new Rectangle (0, 0, size, size);
			Rectangle dstRect = new Rectangle (80, 0, size, size);

			BitmapFPS = TimedAction (delegate {
				ctx.DrawImage (bitmap, srcRect, dstRect);
			});

			dstRect.X += 80;
			ImageFPS = TimedAction (delegate {
				ctx.DrawImage (vectorImage, srcRect, dstRect);
			});

			testMode = false;
			if (TestFinished != null)
				TestFinished (this, EventArgs.Empty);
		}

		int TimedAction (Action draw)
		{
			var t = DateTime.Now;
			var n = 0;
			while ((DateTime.Now - t).TotalMilliseconds < testTime) {
				draw ();
				n++;
			}
			return n;
		}

		void DrawFocus (Context ctx, Point p)
		{
			// Draw a 'zoom'-style Focus at specified point
			double r = 12, w = 31;
			Point o = Point.Zero;	// Drawing origin
			// Align single-thickness lines on 0.5 pixel coords
			o.X += 0.5;
			o.Y += 0.5;
			ctx.Save ();
			ctx.Translate (p);	// Final translation
			// Hairlines in X-direction
			ctx.MoveTo (o.X + r, o.Y);
			ctx.LineTo (o.X + w, o.Y);
			ctx.MoveTo (o.X - r, o.Y);
			ctx.LineTo (o.X - w, o.Y);
			// Hairlines in Y-direction
			ctx.MoveTo (o.X, o.Y + r);
			ctx.LineTo (o.X, o.Y + w);
			ctx.MoveTo (o.X, o.Y - r);
			ctx.LineTo (o.X, o.Y - w);
			// Inner single-thickness circle
			ctx.MoveTo (o.X + r, o.Y);
			ctx.Arc (o.X, o.Y, r, 0, 360);
			ctx.SetLineWidth (1);
			ctx.Stroke ();
			// Double thickness outer arcs. Draw at (0,0) and transform
			o = Point.Zero;
			r = 22;
			ctx.Rotate (5);
			ctx.MoveTo (r, 0);
			ctx.Arc (o.X, o.Y, r, 0, 80);
			ctx.MoveTo (o.X, r);
			ctx.Arc (o.X, o.Y, r, 90, 170);
			ctx.MoveTo (-r, o.Y);
			ctx.Arc (o.X, o.Y, r, 180, 260);
			ctx.MoveTo (o.X, -r);
			ctx.Arc (o.X, o.Y, r, 270, 350);
			ctx.SetLineWidth (2);
			ctx.Stroke ();
			ctx.Restore ();
		}

	}
}

