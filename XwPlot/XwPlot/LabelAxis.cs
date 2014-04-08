//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// LabelAxis.cs
//
// Derived originally from NPlot (Copyright (C) 2003-2006 Matt Howlett and others)
// Updated and ported to Xwt 2012-2014 : Hywel Thomas <hywel.w.thomas@gmail.com>
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
	/// Allows the creation of axes with any number of user defined labels at
	/// user defined world values along the axis. 
	/// </summary>
	public class LabelAxis : Axis
	{
		/// <summary>
		/// Deep copy of LabelAxis.
		/// </summary>
		/// <returns>A copy of the LinearAxis Class.</returns>
		public override object Clone()
		{
			LabelAxis a = new LabelAxis ();
			// ensure that this isn't being called on a derived type. If it is, then oh no!
			if (this.GetType () != a.GetType ()) {
				throw new XwPlotException ("Error. Clone method is not defined in derived type.");
			}
			DoClone( this, a );
			return a;
		}

		/// <summary>
		/// Helper method for Clone.
		/// </summary>
		/// <param name="a">The original object to clone.</param>
		/// <param name="b">The cloned object.</param>
		protected static void DoClone (LabelAxis b, LabelAxis a)
		{
			Axis.DoClone (b, a);

			a.labels = (ArrayList)b.labels.Clone();
			a.numbers = (ArrayList)b.numbers.Clone();

			a.ticksBetweenText = b.ticksBetweenText;
			a.sortDataIfNecessary = b.sortDataIfNecessary;
		}


		/// <summary>
		/// Initialise LabelAxis to default state.
		/// </summary>
		private void Init()
		{
			labels = new ArrayList();
			numbers = new ArrayList();
		}


		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="a">The Axis to clone.</param>
		/// <remarks>TODO: [review notes] I don't think this will work as desired.</remarks>
		public LabelAxis (Axis a) : base( a )
		{
			Init ();
		}


		/// <summary>
		/// Default constructor
		/// </summary>
		public LabelAxis () : base()
		{
			Init ();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="worldMin">Minimum world value</param>
		/// <param name="worldMax">Maximum world value</param>
		public LabelAxis (double worldMin, double worldMax) : base (worldMin, worldMax)
		{
			Init ();
		}


		/// <summary>
		/// Adds a label to the axis
		/// </summary>
		/// <param name="name">The label</param>
		/// <param name="val">The world value at which to place the label</param>
		public void AddLabel (string name, double val)
		{
			labels.Add (name);
			numbers.Add (val);
		}
		

		/// <summary>
		/// Draw Ticks using the Drawing Context and physical extents supplied
		/// associated labels.
		/// </summary>
		/// <param name="ctx">The Drawing Context with which to draw</param>
		/// <param name="physicalMin">The physical location of the world min point</param>
		/// <param name="physicalMax">The physical location of the world max point</param>
		/// <param name="boundingBox">out: smallest box that completely encompasses all of the ticks and tick labels.</param>
		/// <param name="labelOffset">out: a suitable offset from the axis to draw the axis label.</param>
		protected override void DrawTicks (Context ctx, Point physicalMin,  Point physicalMax, out object labelOffset, out object boundingBox )
		{
			Point tLabelOffset;
			Rectangle tBoundingBox;

			labelOffset = getDefaultLabelOffset (physicalMin, physicalMax);
			boundingBox = null;

			// draw the tick labels (but not the ticks).
			Point lastPos = WorldToPhysical ((double)numbers[0], physicalMin, physicalMax, true);
			for (int i=0; i<labels.Count; ++i) {
				if ((double)numbers[i] > WorldMin && (double)numbers[i] < WorldMax) {
					// check to make sure labels are far enough appart.
					Point thisPos = WorldToPhysical ((double)numbers[i], physicalMin, physicalMax, true);
					double dist = Utils.Distance (thisPos, lastPos);

					if (i==0 || dist > PhysicalSpacingMin) {
						lastPos = thisPos;

						DrawTick (ctx, (double)numbers[i], 0, 
							(string)labels[i],
							new Point(0,0), 
							physicalMin, physicalMax,
							out tLabelOffset, out tBoundingBox );
					
						Axis.UpdateOffsetAndBounds ( 
							ref labelOffset, ref boundingBox, 
							tLabelOffset, tBoundingBox );
					}
				}
			}

			// now draw the ticks (which might not be aligned with the tick text).
			ArrayList largeTickPositions;
			ArrayList smallTickPositions;
			WorldTickPositions_FirstPass( physicalMin, physicalMax, out largeTickPositions, out smallTickPositions );
			lastPos = WorldToPhysical ((double)largeTickPositions[0], physicalMin, physicalMax, true);
			for (int i=0; i<largeTickPositions.Count; ++i) {
				double tickPos = (double)largeTickPositions[i];

				// check to see that labels are far enough appart. 
				Point thisPos = WorldToPhysical (tickPos, physicalMin, physicalMax, true);
				double dist = Utils.Distance (thisPos, lastPos);
				if (i==0 || dist>PhysicalSpacingMin) {
					lastPos = thisPos;
					DrawTick (ctx, tickPos, LargeTickSize, "", Point.Zero, physicalMin, physicalMax,
						out tLabelOffset, out tBoundingBox );
					
					Axis.UpdateOffsetAndBounds( 
						ref labelOffset, ref boundingBox, 
						tLabelOffset, tBoundingBox );
				}
			}
		}


		/// <summary>
		/// Determines the positions, in world coordinates, of the large ticks. 
		/// 
		/// Label axes do not have small ticks.
		/// </summary>
		/// <param name="physicalMin">The physical position corresponding to the world minimum of the axis.</param>
		/// <param name="physicalMax">The physical position corresponding to the world maximum of the axis.</param>
		/// <param name="largeTickPositions">ArrayList containing the positions of the large ticks.</param>
		/// <param name="smallTickPositions">null</param>
		internal override void WorldTickPositions_FirstPass(
			Point physicalMin, 
			Point physicalMax,
			out ArrayList largeTickPositions,
			out ArrayList smallTickPositions
			)
		{
			smallTickPositions = null;
			largeTickPositions = new ArrayList();

			// if ticks correspond to position of text
			if (!ticksBetweenText) {
				for (int i=0; i<labels.Count; ++i) {
					if ((double)numbers[i] > WorldMin && (double)numbers[i] < WorldMax) {
						largeTickPositions.Add( numbers[i]);
					}
				}
			}

			// if ticks correspond to gaps between text
			else {
				ArrayList numbers_copy;
				if (sortDataIfNecessary) {
					numbers_copy = (ArrayList)numbers.Clone(); // shallow copy.
					numbers_copy.Sort();
				}
				else {
					numbers_copy = numbers;
				}

				for (int i=1; i<labels.Count; ++i) {
					double worldPosition = ((double)numbers_copy[i] + (double)numbers_copy[i-1])/2.0;
					if (worldPosition > WorldMin && worldPosition < WorldMax) {
						largeTickPositions.Add (worldPosition);
					}
				}
			}
		}

		/// <summary>
		/// If true, large ticks are drawn between the labels, rather
		/// than at the position of the labels.
		/// </summary>
		public bool TicksBetweenText
		{
			get {
				return ticksBetweenText;
			}
			set {
				ticksBetweenText = value;
			}
		}
		private bool ticksBetweenText = false;

		/// <summary>
		/// If your data may be be specified out of order (that is 
		/// abscissa values with a higher index may be less than
		/// abscissa values of a lower index), then data sorting 
		/// may be necessary to implement some of the functionality
		/// of this object. If you know your data is already 
		/// ordered with abscissa values lowest -> highest, then
		/// you may set this to false. It's default is true.
		/// </summary>
		public bool SortDataIfNecessary
		{
			get {
				return sortDataIfNecessary;
			}
			set {
				sortDataIfNecessary = value;
			}
		}
		private bool sortDataIfNecessary = true;


		/// <summary>
		/// If consecutive labels are less than this number of pixels appart, 
		/// some of the labels will not be drawn.
		/// </summary>
		public double PhysicalSpacingMin
		{
			get {
				return physicalSpacingMin;
			}
			set {
				physicalSpacingMin = value;
			}
		}
		private double physicalSpacingMin = 0;


		private ArrayList labels;
		private ArrayList numbers;
	}
}
