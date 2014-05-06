﻿//
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
			DrawFocus (ctx, lastCursor);
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
			// Draw a 'zoom'-style Focus at specified point
			double r = 12, w = focusRadius - 1;
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
			ctx.SetColor (Colors.Black);
			ctx.SetLineWidth (1);
			ctx.Stroke ();
			// Double thickness outer arcs. Draw at (0,0) and rotate
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

