//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// PlotSelection.cs
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
	/// Uses a Rubberband rectangle to select an area of the plot for the new Plot Range
	/// </summary>
	public class PlotSelection : Interaction
	{
		bool selectionActive = false;
		Point startPoint = Point.Zero;
		Point endPoint = Point.Zero;
		Rectangle selection = Rectangle.Zero;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public PlotSelection ()
		{
			LineColor = Colors.White;
		}

		/// <summary>
		/// Constructor with specific color
		/// </summary>
		public PlotSelection (Color color)
		{
			LineColor = color;
		}

		/// <summary>
		/// PlotSelection LineColor
		/// </summary>
		public Color LineColor { get; set; }

		public override bool OnButtonPressed (ButtonEventArgs args, PlotCanvas pc)
		{
			// Only start selection if mouse is inside plot area (excluding axes)
			Rectangle area = pc.PlotAreaBoundingBoxCache;
			if (args.Button == PointerButton.Left && area.Contains (args.Position)) {
				selectionActive = true;
				startPoint.X = args.X;
				startPoint.Y = args.Y;
				endPoint = startPoint;
			}
			return false;
		}

		public override bool OnButtonReleased (ButtonEventArgs args, PlotCanvas pc)
		{
			bool modified = false;

			// delete previous overlay rectangle
			pc.Canvas.QueueDraw (selection);

			if (selectionActive) {
				selectionActive = false;
				Rectangle bounds = pc.PlotAreaBoundingBoxCache;
				if (!bounds.Contains(endPoint)) {
					// MouseUp outside plotArea - cancel selection
					modified = false;
				}
				else {
					pc.CacheAxes();
					// Redefine range based on selection. The proportions for
					// Min and Max do not require Min < Max, since they will
					// be re-ordered by Axis.DefineRange if necessary
					double xMin = startPoint.X - bounds.Left;
					double yMin = bounds.Bottom - startPoint.Y;
				
					double xMax = endPoint.X - bounds.Left;
					double yMax = bounds.Bottom - endPoint.Y;
				
					double xMinProp = xMin/bounds.Width;
					double xMaxProp = xMax/bounds.Width;
					double yMinProp = yMin/bounds.Height;
					double yMaxProp = yMax/bounds.Height;
				
					pc.DefineXAxes (xMinProp, xMaxProp);
					pc.DefineYAxes (yMinProp, yMaxProp);
					modified = true;
				}
			}
			return modified;
		}

		public override bool OnMouseMoved (MouseMovedEventArgs args, PlotCanvas pc)
		{
			double X = args.X;
			double Y = args.Y;

			if (selectionActive) {
				// note last selection rectangle
				Rectangle lastSelection = selection;
				Rectangle bounds = pc.PlotAreaBoundingBoxCache;
				// clip selection rectangle to PlotArea
				X = Math.Max(X, bounds.Left);
				X = Math.Min(X, bounds.Right);
				Y = Math.Max(Y, bounds.Top);
				Y = Math.Min(Y, bounds.Bottom);

				endPoint.X = X;
				endPoint.Y = Y;
				selection = FromPoints (startPoint, endPoint);

				pc.Canvas.QueueDraw (lastSelection);
				//Console.WriteLine ("Erase: {0} {1} {2} {3} ", lastSelection.X, lastSelection.Y, lastSelection.Width, lastSelection.Height);
				pc.Canvas.QueueDraw (selection);
			}
			return false;
		}

		public override bool OnMouseExited (EventArgs args, PlotCanvas pc)
		{
			if (selectionActive) {
				pc.Canvas.QueueDraw (selection);
				selectionActive = false;
			}
			return false;
		}

		public override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			if (selectionActive && selection != Rectangle.Zero) {
				ctx.Save ();
				ctx.SetColor (LineColor);
				ctx.Rectangle (selection);
				ctx.Stroke ();
				//Console.WriteLine ("Draw: {0} {1} {2} {3} ", selection.X, selection.Y, selection.Width, selection.Height);
				ctx.Restore ();
			}
		}

		/// <summary>
		/// Return normalised Rectangle from two diagonal points, reordering if necessary
		/// </summary>
		Rectangle FromPoints (Point start, Point end)
		{
			Point tl = start;
			Point br = end;
			if (start.X > end.X) {
				tl.X = end.X;
				br.X = start.X;
			}
			if (start.Y > end.Y) {
				tl.Y = end.Y;
				br.Y = start.Y;
			}
			double w = br.X - tl.X + 1;
			double h = br.Y - tl.Y + 1;
			return new Rectangle (tl.X, tl.Y, w, h);
		}

	} // Plot Selection
		
}
