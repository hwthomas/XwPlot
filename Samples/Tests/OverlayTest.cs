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
	public class OverlayTest: OverlayCanvas
	{
		const double focusRadius = 32; 
		Size startSize = new Size (400, 400);
		Point lastCursor = Point.Zero;

		public OverlayTest () : base ()
		{
			WidthRequest = startSize.Width;
			HeightRequest = startSize.Height;
		}

		protected override void OnMouseMoved (MouseMovedEventArgs args)
		{
			// Clear previous overlay
			Rectangle focus = new Rectangle (lastCursor.X - 32, lastCursor.Y - 32, 65, 65);
			QueueDraw (focus);
			lastCursor.X = args.X;
			lastCursor.Y = args.Y;
			// Queue new overlay drawing
			focus = new Rectangle (lastCursor.X - 32, lastCursor.Y - 32, 64, 64);
			QueueDraw (focus);
			base.OnMouseMoved (args);
		}

		protected override void OnDrawCache (Context ctx, Rectangle dirtyArea)
		{
			DrawCache (ctx);
		}

		protected override void OnDrawOverlay (Context ctx, Rectangle dirtyArea)
		{
			// check if sufficiently inside Canvas
			// only draw once inside focusRadius
			if (lastCursor.X > focusRadius && lastCursor.X < Bounds.Right  - focusRadius &&
			    lastCursor.Y > focusRadius && lastCursor.Y < Bounds.Bottom - focusRadius) {
				DrawFocus (ctx, lastCursor);
			}
		}

		void DrawCache (Context ctx)
		{
			ctx.Save ();
			// Test 'background' is a vertical colour gradient
			ctx.Rectangle (0, 0, Bounds.Width, Bounds.Height);
			Gradient g = new Xwt.Drawing.LinearGradient (0, 0, 0, Bounds.Height);
			g.AddColorStop (0, new Color (0.5, 0.5, 1));
			g.AddColorStop (1, new Color (0.5, 1, 0.5));
			ctx.Pattern = g;
			ctx.Fill ();
			ctx.Restore ();
		}

		void DrawFocus (Context ctx, Point p)
		{
			// Draw a 'zoom'-style Focus at specified point.

			double r1 = 12, r2 = 22, w = 31;	// focusRadius = 32
			Point o = Point.Zero;	// Drawing origin
			// Align single-thickness lines on 0.5 pixel coords
			o.X += 0.5;
			o.Y += 0.5;

			ctx.Save ();
			ctx.SetColor (Colors.Black);
			ctx.Translate (p);	// Final translation to point p
			// draw as 4 quadrants, each rotated by +90 degrees
			for (double theta = 0; theta < 360; theta += 90) {
				ctx.Rotate (theta);
				// Hairline in X-direction, ending at x=r1
				ctx.MoveTo (o.X + w, o.Y);
				ctx.LineTo (o.X + r1, o.Y);
				// Inner single-thickness arc
				ctx.Arc (o.X, o.Y, r1, 0, 90);
				ctx.SetLineWidth (1);
				ctx.Stroke ();
				// Double thickness outer arc, 5 - 85 degrees. Draw at (0,0) and rotate
				ctx.Save ();
				ctx.Rotate (5);
				ctx.MoveTo (r2, 0);
				ctx.Arc (0, 0, r2, 0, 80);
				ctx.SetLineWidth (2);
				ctx.Stroke ();
				ctx.Restore ();
			}
			ctx.Restore ();
		}
	}

}

