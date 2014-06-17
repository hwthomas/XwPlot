//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// PlotZoom.cs
// 
// Copyright (C) 2013 Hywel Thomas <hywel.w.thomas@gmail.com>
//
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this
//	  list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright notice,
//	  this list of conditions and the following disclaimer in the documentation
//	  and/or other materials provided with the distribution.
// 3. Neither the name of XwPlot nor the names of its contributors may
//	  be used to endorse or promote products derived from this software without
//	  specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
// OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
// OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System;
using Xwt;
using Xwt.Drawing;

namespace XwPlot
{
	/// <summary>
	/// Mouse Scroll (wheel) increases or decreases both axes scaling factors
	/// Zoom direction is Up/+ve/ZoomIn or Down/-ve/ZoomOut.  If the mouse
	/// pointer is inside the plot area, its position is used as the focus point
	/// of the zoom, otherwise the centre of the plot is used as the default
	/// </summary>
	public class PlotZoom : Interaction
	{
		private Rectangle focusRect = Rectangle.Zero;
		private Point pF = Point.Zero;
  
		public PlotZoom ()
		{
			Sensitivity = 1.0;
		}
		 
		/// <summary>
		/// Sensitivity factor for plot zoom
		/// </summary>
		public double Sensitivity { get; set; }

		/// <summary>
		/// Mouse Scroll (wheel) method for PlotZoom interaction
		/// </summary>
		public override bool OnMouseScrolled (MouseScrolledEventArgs args, PlotCanvas pc)
		{
			double proportion = 0.1*Sensitivity;	// use initial zoom of 10%
			double focusX = 0.5, focusY = 0.5;		// default focus point

			double direction = 1;
			if (args.Direction == ScrollDirection.Down) {
				direction = -1;
			}
				
			// Zoom direction is +1 for Up/ZoomIn, or -1 for Down/ZoomOut
			proportion *= -direction;

			// delete previous focusPoint drawing - this is all a bit 'tentative'
			//pc.QueueDraw (focusRect);

			Rectangle area = pc.PlotAreaBoundingBoxCache;
			if (area.Contains(args.X, args.Y)) {
				pF.X = args.X;
				pF.Y = args.Y;
				focusX = (double)(args.X - area.Left)/(double)area.Width;
				focusY = (double)(area.Bottom - args.Y)/(double)area.Height;
			}

			// Zoom in/out for all defined axes
			pc.CacheAxes();
			pc.ZoomXAxes (proportion,focusX);
			pc.ZoomYAxes (proportion,focusY);

			double x = pF.X-10;
			double y = pF.Y-10;

			focusRect = new Rectangle (x, y, 21, 21);
			// draw new focusRect
			//pc.QueueDraw (focusRect);
				
			return (true);
		}

		/// <summary>
		/// MouseMove method for PlotScroll
		/// </summary>
		public override bool OnMouseMoved (MouseMovedEventArgs args, PlotCanvas pc)
		{
			// delete previous focusPoint drawing
			//pc.QueueDraw (focusRect);
			return false;
		}

		public override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			//DrawFocus (ctx);
		}

		void DrawFocus (Context ctx, Point p)
		{
			// Draw a 'zoom'-style Focus at specified point
			double focusRadius = 32; 
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

	} // Plot Zoom
	
}
