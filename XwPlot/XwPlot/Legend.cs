//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// Legend.cs
//
// Derived originally from NPlot (Copyright (C) 2003-2006 Matt Howlett and others)
// Updated and ported to Xwt 2012-2014 : Hywel Thomas <hywel.w.thomas@gmail.com>
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
// 3. Neither the name of NPlot nor the names of its contributors may
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
	/// Legend functionality specific to Legends associated with a PlotSurface.
	/// </summary>
	public class Legend : LegendBase
	{

		/// <summary>
		/// Enumeration of possible Legend placements.
		/// </summary>
		public enum Placement
		{
			/// <summary>
			/// Inside the plot area.
			/// </summary>
			Inside = 0,
			/// <summary>
			/// Outside the plot area.
			/// </summary>
			Outside = 1
		}

		private double xOffset_;
		private double yOffset_;
		private XAxisPosition xAttach_;
		private YAxisPosition yAttach_;
		private Placement horizontalEdgePlacement_;
		private Placement verticalEdgePlacement_;
		private bool neverShiftAxes_;

		/// <summary>
		/// Whether or not the positions of the Axes may be shifted to make
		/// room for the Legend. 
		/// </summary>
		public bool NeverShiftAxes
		{
			get { return neverShiftAxes_; }
			set { neverShiftAxes_ = value; }
		}


		/// <summary>
		/// Offset from the chosen Y-Axis. TODO: better description.
		/// </summary>
		public double XOffset
		{
			get { return xOffset_; }
			set { xOffset_ = value; }
		}


		/// <summary>
		/// Offset from the X-Axis. TODO: better description.
		/// </summary>
		public double YOffset
		{
			get { return yOffset_; }
			set { yOffset_ = value; }
		}


		/// <summary>
		/// Whether or not to attach the legend on the inside of the top
		/// or bottom axis (which, is specified using the AttachTo method) or the 
		/// outside. 
		/// </summary>
		public Legend.Placement VerticalEdgePlacement
		{
			get { return verticalEdgePlacement_; }
			set { verticalEdgePlacement_ = value; }
		}

		
		/// <summary>
		/// Whether or not to attach the legend on the inside of the
		/// left or right axis (which, is specified using the AttachTo method) 
		/// or the outside.
		/// </summary>
		public Legend.Placement HorizontalEdgePlacement
		{
			get { return horizontalEdgePlacement_; }
			set { horizontalEdgePlacement_ = value; }
		}


		/// <summary>
		/// Specify the Axes to attach the legend to. 
		/// </summary>
		/// <param name="xa">Specify which horizontal axis the legend should be attached to.</param>
		/// <param name="ya">Specify which vertical axis the legend should be attached to.</param>
		public void AttachTo (XAxisPosition xa, YAxisPosition ya)
		{
			xAttach_ = xa;
			yAttach_ = ya; 
		}


		/// <summary>
		/// Default constructor.
		/// </summary>
		public Legend()
		{
			xAttach_ = XAxisPosition.Top;
			yAttach_ = YAxisPosition.Right;
			xOffset_ = 10;
			yOffset_ = 1;
			verticalEdgePlacement_ = Placement.Outside;
			horizontalEdgePlacement_ = Placement.Inside;
			neverShiftAxes_ = false;
		}


		/// <summary>
		/// Updates the PlotSurface axes to compensate for the legend.
		/// </summary>
		/// <param name="pXAxis1">the bottom x axis</param>
		/// <param name="pYAxis1">the left y axis</param>
		/// <param name="pXAxis2">the top x axis</param>
		/// <param name="pYAxis2">the right y axis</param>
		/// <param name="plots">list of plots.</param>
		/// <param name="scale">scale parameter (for text and other)</param>
		/// <param name="padding">padding around plot within bounds.</param>
		/// <param name="bounds">graphics surface bounds</param>
		/// <param name="position">legend position</param>
		public void UpdateAxesPositions ( 
			PhysicalAxis pXAxis1,
			PhysicalAxis pYAxis1,
			PhysicalAxis pXAxis2,
			PhysicalAxis pYAxis2,
			ArrayList plots,
			double scale,
			double padding,
			Rectangle bounds,
			out Point position)
		{
		
			double leftIndent = 0;
			double rightIndent = 0;
			double bottomIndent = 0;
			double topIndent = 0;

			position = new Point (0,0);

			// now determine if legend should change any of these (legend should be fully 
			// visible at all times), and draw legend.

			Rectangle legendWidthHeight = GetBoundingBox (new Point(0,0), plots, scale);

			if (legendWidthHeight.Width > bounds.Width) {
				legendWidthHeight.Width = bounds.Width;
			}

			// (1) calculate legend position.

			// y
			position.Y = this.yOffset_;
			
			if (xAttach_ == XAxisPosition.Bottom) {
				position.Y += pYAxis1.PhysicalMin.Y;
				if (horizontalEdgePlacement_ == Legend.Placement.Inside) {
					position.Y -= legendWidthHeight.Height;
				}
			}
			else {
				position.Y += pYAxis1.PhysicalMax.Y;
				if (horizontalEdgePlacement_ == Legend.Placement.Outside) {
					position.Y -= legendWidthHeight.Height;
				}
			}
	
			// x
			position.X = xOffset_;
		
			if (yAttach_ == YAxisPosition.Left) {
				if (verticalEdgePlacement_ == Legend.Placement.Outside)  {
					position.X -= legendWidthHeight.Width;
				}
				position.X += pXAxis1.PhysicalMin.X;
			}
			else {
				if (verticalEdgePlacement_ == Legend.Placement.Inside) {
					position.X -= legendWidthHeight.Width;
				}
				position.X += pXAxis1.PhysicalMax.X;
			}


			// determine update amounts for axes

			if (!neverShiftAxes_) {
				if (position.X < padding) {
					double changeAmount = -position.X + padding;
					// only allow axes to move away from bounds.
					if (changeAmount > 0) {
						leftIndent = changeAmount;
					}
					position.X += changeAmount;
				}

				if (position.X + legendWidthHeight.Width > bounds.Right - padding) {
					double changeAmount = (position.X - bounds.Right + legendWidthHeight.Width + padding);
					// only allow axes to move away from bounds.
					if (changeAmount > 0) {
						rightIndent = changeAmount;
					}
					position.X -= changeAmount;
				}

				if (position.Y < padding) {
					double changeAmount = -position.Y + padding;
					// only allow axes to move away from bounds.
					if (changeAmount > 0) {
						topIndent = changeAmount;
					}
					position.Y += changeAmount;
				}

				if (position.Y + legendWidthHeight.Height > bounds.Bottom - padding)
				{
					double changeAmount = (position.Y - bounds.Bottom + legendWidthHeight.Height + padding);
					// only allow axes to move away from bounds.
					if (changeAmount > 0) {
						bottomIndent = changeAmount;
					}
					position.Y -= changeAmount;
				}

				// update axes.

				pXAxis1.PhysicalMin = new Point (pXAxis1.PhysicalMin.X + leftIndent, pXAxis1.PhysicalMin.Y - bottomIndent);
				pXAxis1.PhysicalMax = new Point (pXAxis1.PhysicalMax.X - rightIndent, pXAxis1.PhysicalMax.Y - bottomIndent);
				pYAxis1.PhysicalMin = new Point (pYAxis1.PhysicalMin.X + leftIndent, pYAxis1.PhysicalMin.Y - bottomIndent);
				pYAxis1.PhysicalMax = new Point (pYAxis1.PhysicalMax.X + leftIndent, pYAxis1.PhysicalMax.Y + topIndent);

				pXAxis2.PhysicalMin = new Point (pXAxis2.PhysicalMin.X + leftIndent, pXAxis2.PhysicalMin.Y + topIndent);
				pXAxis2.PhysicalMax = new Point (pXAxis2.PhysicalMax.X - rightIndent, pXAxis2.PhysicalMax.Y + topIndent);
				pYAxis2.PhysicalMin = new Point (pYAxis2.PhysicalMin.X - rightIndent, pYAxis2.PhysicalMin.Y - bottomIndent);
				pYAxis2.PhysicalMax = new Point (pYAxis2.PhysicalMax.X - rightIndent, pYAxis2.PhysicalMax.Y + topIndent);
			}
		}
	}
}
