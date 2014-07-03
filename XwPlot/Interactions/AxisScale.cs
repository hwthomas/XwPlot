//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// AxisScale.cs
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
using System.Collections;

using Xwt;
using Xwt.Drawing;

namespace XwPlot
{

	/// <summary>
	/// Dragging within an axis increases or decreases its scaling factors
	/// with the expansion being about the point where the drag is started
	/// Only the axis in which the mouse is clicked will be modified
	/// </summary>
	public class AxisScale : Interaction
	{
		Axis axis = null;
		bool dragging = false;
		PhysicalAxis physicalAxis = null;
		Point lastPoint;
		Point startPoint;
		double focusRatio = 0.5;

		/// <summary>
		/// Default constructor
		/// </summary>
		public AxisScale ()
		{
			Sensitivity = 1.0;
		}

		/// <summary>
		/// Sensitivity factor for axis scaling
		/// </summary>
		public double Sensitivity { get; set; }

		/// <summary>
		/// OnButtonPressed method for AxisScale interaction
		/// </summary>
		public override bool OnButtonPressed (ButtonEventArgs args, PlotCanvas pc)
		{
			// if the mouse is inside the plot area (the tick marks may be here,
			// and are counted as part of the axis), then *don't* invoke scaling 
			if (pc.PlotAreaBoundingBoxCache.Contains(args.X, args.Y)) {
				return false;
			}
				
			if (args.Button == PointerButton.Left) {
				// see if hit with axis. NB Only one axis object will be returned
				ArrayList objects = pc.HitTest (new Point(args.X, args.Y));

				foreach (object o in objects) {
					if (o is Axis) {
						dragging = true;
						axis = (Axis)o;
						if (pc.PhysicalXAxis1Cache.Axis == axis) {
							physicalAxis = pc.PhysicalXAxis1Cache;
							//pc.plotCursor = CursorType.LeftRight;
						}
						else if (pc.PhysicalXAxis2Cache.Axis == axis) {
							physicalAxis = pc.PhysicalXAxis2Cache;
							//ps.plotCursor = CursorType.LeftRight;
						}
						else if (pc.PhysicalYAxis1Cache.Axis == axis) {
							physicalAxis = pc.PhysicalYAxis1Cache;
							//pc.plotCursor = CursorType.UpDown;
						}
						else if (pc.PhysicalYAxis2Cache.Axis == axis) {
							physicalAxis = pc.PhysicalYAxis2Cache;
							//pc.plotCursor = CursorType.UpDown;
						}

						startPoint = new Point (args.X, args.Y);
						lastPoint = startPoint;

						// evaluate focusRatio about which axis is expanded
						double  x = startPoint.X - physicalAxis.PhysicalMin.X;
						double  y = startPoint.Y - physicalAxis.PhysicalMin.Y;
						double r = Math.Sqrt(x*x + y*y);
						focusRatio = r/physicalAxis.PhysicalLength;
						return false;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// OnButtonReleased method for AxisScale interaction
		/// </summary>
		public override bool OnButtonReleased (ButtonEventArgs args, PlotCanvas pc)
		{
			if (dragging) {
				dragging = false;
				axis = null;
				physicalAxis = null;
				lastPoint = new Point();
				//pc.plotCursor = CursorType.LeftPointer;
			}
			return false;
		}

		/// <summary>
		/// OnMouseMoved method for AxisScale interaction
		/// </summary>
		public override bool OnMouseMoved (MouseMovedEventArgs args, PlotCanvas pc)
		{
			if (dragging && physicalAxis != null) {
				pc.CacheAxes();

				double dX = (args.X - lastPoint.X);
				double dY = (args.Y - lastPoint.Y);
				lastPoint = new Point (args.X, args.Y);

				// In case the physical axis is not horizontal/vertical, combine dX and dY
				// in a way which preserves their sign and intuitive axis zoom sense, ie
				// because the physical origin is top-left, expand with +ve dX, but -ve dY 
				double distance = dX - dY;
				double proportion = distance*Sensitivity /physicalAxis.PhysicalLength;
				axis.IncreaseRange (proportion, focusRatio);
				return true;
			}
			return false;
		}

	} // AxisScale

}
