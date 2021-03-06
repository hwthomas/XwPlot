//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// PlotScale.cs
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
	/// PlotScale allows Plot to be rescaled in both X and Y
	/// </summary>
	public class PlotScale : Interaction
	{
		Point lastPoint = new Point (-1, -1);
		Point unset = new Point (-1, -1);
		private bool scaling = false;
		double focusX = 0.5, focusY = 0.5;
		Key key; 

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="horizontal">enable horizontal drag/param>
		/// <param name="vertical">enable vertical drag/param>
		public PlotScale (bool horizontal, bool vertical)
		{
			Vertical = vertical;
			Horizontal = horizontal;
			Sensitivity = 2.0;
		}

		/// <summary>
		/// Horizontal Scale enable/disable
		/// </summary>
		public bool Horizontal { get; set; }

		/// <summary>
		/// Vertical Scale enable/disable
		/// </summary>
		public bool Vertical { get; set; }

		/// <summary>
		/// Sensitivity factor for plot scaling
		/// </summary>
		public double Sensitivity { get; set; }

		public override bool OnButtonPressed (ButtonEventArgs args, PlotCanvas pc)
		{
			// Only start scaling if mouse is inside plot area (excluding axes)
			Rectangle area = pc.PlotAreaBoundingBoxCache;
			if (area.Contains (args.Position)) {
				scaling = true;
				lastPoint = new Point (args.X, args.Y);
				if (args.Button == PointerButton.Left) {
					if (Horizontal) {
						;//pc.plotCursor = CursorType.LeftRight;
					}
					if (Vertical) {
						;//pc.plotCursor = CursorType.UpDown;
					}
					if (Horizontal && Vertical) {
						;//pc.plotCursor = CursorType.Zoom;
					}
				}
				// evaluate focusPoint about which axis is expanded
				focusX = (double)(args.X - area.Left)/(double)area.Width;
				focusY = (double)(area.Bottom - args.Y)/(double)area.Height;
			}
			return false;
		}

		public override bool OnButtonReleased (ButtonEventArgs args, PlotCanvas pc)
		{
			if (scaling) {
				lastPoint = unset;
				scaling = false;
				//pc.plotCursor = CursorType.LeftPointer;
			}
			return false;
		}

		public override bool OnMouseMoved (MouseMovedEventArgs args, PlotCanvas pc)
		{
			Rectangle area = pc.PlotAreaBoundingBoxCache;

			if (scaling) {
				pc.CacheAxes();

				double dX = args.X - lastPoint.X;		// distance mouse has moved
				double dY = args.Y - lastPoint.Y;
				lastPoint = new Point (args.X, args.Y);

				// Alt key reduces sensitivity
				double factor = Sensitivity;
				if (key == Key.AltLeft || key == Key.AltRight) {
					factor *= 0.25;	   // arbitrary change 
				}

				double xProportion = +dX*factor/area.Width;
				double yProportion = -dY*factor/area.Height;
				if (Horizontal) {
					pc.ZoomXAxes (xProportion, focusX);
				}
				if (Vertical) {
					pc.ZoomYAxes (yProportion, focusY);
				}
				return true;
			}
			return false;
		}

		public override bool OnKeyPressed (KeyEventArgs args, PlotCanvas pc)
		{
			key = args.Key;
			return false;
		}

		public override bool OnKeyReleased (KeyEventArgs args, PlotCanvas pc)
		{
			key = args.Key;
			return false;
		}

	} // PlotScale
	
}