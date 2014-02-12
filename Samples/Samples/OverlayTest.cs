//
// OverlayTest.cs
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
	public class OverlayTest: Canvas
	{
		bool testMode = false;

		Image vectorImage;
		Image bitmap;

		int testTime = 1000;
		double size = 500;
		double iterations = 20;

		ImageBuilder ib;
		Context ibx;

		public int DrawFPS { get; private set; }
		public int BitmapFPS { get; private set; }
		public int ImageFPS { get; private set; }

		public event EventHandler TestFinished;

		public OverlayTest ()
		{
			ib = new ImageBuilder (size, size);
			ibx = ib.Context;
			DrawScene (ibx);
			bitmap = ib.ToBitmap ();
			vectorImage = ib.ToVectorImage ();
			WidthRequest = size;
			HeightRequest = size;
		}

		public void StartTest ()
		{
			testMode = true;
			QueueDraw ();
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			DrawScene (ctx);	// Draw scene on Canvas Context

			DrawScene (ibx);	// Draw off-screen once inbitially
			bitmap = ib.ToBitmap ();
			vectorImage = ib.ToVectorImage ();

			if (!testMode)
				return;

			// Simplest drawing - direct to Canvas context
			DrawFPS = TimedDraw (delegate {
				DrawScene (ctx);
			});

			// Check timings for drawing the scene:-
			// a) to the ImageBuilder Context
			// b) converting to bitmap, etc
			// c) copying to Canvas.Context

			BitmapFPS = TimedDraw (delegate {
				//DrawScene (ibx);
				//bitmap = ib.ToBitmap ();
				ctx.DrawImage (bitmap, 0, 0);
			});

			ImageFPS = TimedDraw (delegate {
				//DrawScene (ibx);
				//vectorImage = ib.ToVectorImage ();
				ctx.DrawImage (vectorImage, 0, 0);
			});

			testMode = false;
			if (TestFinished != null)
				TestFinished (this, EventArgs.Empty);
		}

		int TimedDraw (Action draw)
		{
			var t = DateTime.Now;
			var n = 0;
			while ((DateTime.Now - t).TotalMilliseconds < testTime) {
				draw ();
				n++;
			}
			return n;
		}

		void DrawScene (Context ctx)
		{
			ctx.SetLineWidth (1);
			ctx.SetColor (Colors.Black);
			for (int n = 1; n < iterations; n += 3) {
				ctx.Rectangle (0, 0, (size / iterations) * n, (size / iterations) * n);
				ctx.Stroke ();
				ctx.Arc (size/2, size/2, ((size / iterations) * n) / 2, 0, 360);
				ctx.Stroke ();
			}
		}
	}
}

