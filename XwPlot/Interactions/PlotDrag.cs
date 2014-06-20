//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// PlotDrag.cs
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
	/// PlotDrag allows Plot to be dragged without rescaling in both X and Y
	/// </summary>
	public class PlotDrag : Interaction
	{
		Point lastPoint;
		Point unset = Point.Zero;
		bool dragging = false;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="horizontal">enable horizontal drag/param>
		/// <param name="vertical">enable vertical drag/param>
		public PlotDrag (bool horizontal, bool vertical)
		{
			Vertical = vertical;
			Horizontal = horizontal;
		}

		/// <summary>
		/// Horizontal Drag enable/disable
		/// </summary>
		public bool Horizontal { get; set; }

		/// <summary>
		/// Vertical Drag enable/disable
		/// </summary>
		public bool Vertical { get; set; }

		public override bool OnButtonPressed (ButtonEventArgs args, PlotCanvas pc)
		{
			// Only start drag if mouse is inside plot area (excluding axes)
			Rectangle area = pc.PlotAreaBoundingBoxCache;
			if (area.Contains (args.Position)) {
				dragging = true;
				lastPoint = new Point (args.X, args.Y);
				if (args.Button == PointerButton.Left) {
					if (Horizontal || Vertical) {
						//pc.plotCursor = CursorType.Hand;
					}
				}
			}
			return false;
		}

		public override bool OnButtonReleased (ButtonEventArgs args, PlotCanvas pc)
		{
			if (dragging) {
				lastPoint = unset;
				dragging = false;
				//pc.plotCursor = CursorType.LeftPointer;
			}
			return false;
		}

		public override bool OnMouseMoved (MouseMovedEventArgs args, PlotCanvas pc)
		{
			Rectangle area = pc.PlotAreaBoundingBoxCache;

			if (dragging) {
				pc.CacheAxes();

				double dX = args.X - lastPoint.X;		// distance mouse has moved
				double dY = args.Y - lastPoint.Y;
				lastPoint = new Point (args.X, args.Y);

				// Axis translation required
				double xShift = -dX / area.Width;
				double yShift = +dY / area.Height;

				if (Horizontal) {
					pc.TranslateXAxes (xShift);
				}
				if (Vertical) {
					pc.TranslateYAxes (yShift);
				}
				return true;
			}
			return false;
		}

	} // PlotDrag
	
}