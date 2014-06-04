//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// AxesConstraint.cs
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
	/// Classes derived from this abstract base class define and can apply 
	/// some form of constraint to the positioning and length of one or more
	/// of the four axes of a PlotSurface.
	/// </summary>
	public abstract class AxesConstraint
	{
		/// <summary>
		/// Applies the constraint to the axes. Must be overriden.
		/// </summary>
		/// <param name="pXAxis1">The bottom x-axis.</param>
		/// <param name="pYAxis1">The left y-axis.</param>
		/// <param name="pXAxis2">The top x-axis.</param>
		/// <param name="pYAxis2">The right y-axis.</param>
		public abstract void ApplyConstraint (PhysicalAxis pXAxis1, PhysicalAxis pYAxis1, PhysicalAxis pXAxis2, PhysicalAxis pYAxis2 );


		/// <summary>
		/// Defines an AxisConstraint that forces the world length corresponding
		/// to one pixel on the bottom x-axis to be a certain value. 
		/// 
		/// TODO: Allow the pixel world length to be set for the top axis.
		/// </summary>
		public class XPixelWorldLength : AxesConstraint
		{
			private double pWorldLength_ = 0.0;
			private object holdFixedY_ = null;

			/// <summary>
			/// Constructor, which defines the world pixel length only. Both 
			/// y-axes will be moved by equal amounts in order to force this
			/// constraint.
			/// </summary>
			/// <param name="p">The world pixel length</param>
			public XPixelWorldLength (double p)
			{
				pWorldLength_ = p;
			}
		
			/// <summary>
			/// Constructor, which defines the world pixel length together with
			/// the y-axis that should be held constant when forcing this 
			/// constraint [the other y-axis only will be moved].
			/// </summary>
			/// <param name="p">The world pixel length</param>
			/// <param name="holdFixedY">The position of this y-axis will be 
			/// held constant. The other y-axis will be moved in order to 
			/// force the constraint.</param>
			public XPixelWorldLength (double p, AxisPosition holdFixedY)
			{
				pWorldLength_ = p;
				holdFixedY_ = holdFixedY;
			}

			/// <summary>
			/// Applies the constraint to the axes.
			/// </summary>
			/// <param name="pXAxis1">The bottom x-axis.</param>
			/// <param name="pYAxis1">The left y-axis.</param>
			/// <param name="pXAxis2">The top x-axis.</param>
			/// <param name="pYAxis2">The right y-axis.</param>
			public override void ApplyConstraint( 
				PhysicalAxis pXAxis1, PhysicalAxis pYAxis1, 
				PhysicalAxis pXAxis2, PhysicalAxis pYAxis2 )
			{
				double desiredLength = pXAxis1.Axis.WorldLength/pWorldLength_;
				double currentLength = pXAxis1.PhysicalLength;
				double delta = currentLength - desiredLength;

				double changeLeft = delta / 2;
				double changeRight = delta / 2;
				if (holdFixedY_ != null) {
					if ((YAxisPosition)holdFixedY_ == YAxisPosition.Left) {
						changeLeft = 0;
						changeRight = delta;
					}
					else {
						changeLeft = delta;
						changeRight = 0;
					}
				}

				pXAxis1.PhysicalMin = new Point (pXAxis1.PhysicalMin.X+changeLeft, pXAxis1.PhysicalMin.Y);
				pXAxis1.PhysicalMax = new Point (pXAxis1.PhysicalMax.X-changeRight, pXAxis1.PhysicalMax.Y);
				pXAxis2.PhysicalMin = new Point (pXAxis2.PhysicalMin.X+changeLeft, pXAxis2.PhysicalMin.Y);
				pXAxis2.PhysicalMax = new Point (pXAxis2.PhysicalMax.X-changeRight, pXAxis2.PhysicalMax.Y);

				pYAxis1.PhysicalMin = new Point (pYAxis1.PhysicalMin.X+changeLeft, pYAxis1.PhysicalMin.Y);
				pYAxis1.PhysicalMax = new Point (pYAxis1.PhysicalMax.X+changeLeft, pYAxis1.PhysicalMax.Y);
				pYAxis2.PhysicalMin = new Point (pYAxis2.PhysicalMin.X-changeRight, pYAxis2.PhysicalMin.Y);
				pYAxis2.PhysicalMax = new Point (pYAxis2.PhysicalMax.X-changeRight, pYAxis2.PhysicalMax.Y);
		
			}

		} // class XPixelWorldLength

		/// <summary>
		/// Defines an AxisConstraint that forces the world length corresponding
		/// to one pixel on the left y-axis to be a certain value. 
		/// 
		/// TODO: Allow the pixel world length to be set for the right axis.
		/// </summary>
		public class YPixelWorldLength : AxesConstraint
		{
			private double pWorldLength_ = 0.0;
			private object holdFixedX_ = null;

			/// <summary>
			/// Constructor, which defines the world pixel length only. Both 
			/// x-axes will be moved by equal amounts in order to force this
			/// constraint.
			/// </summary>
			/// <param name="p">The world pixel length</param>
			public YPixelWorldLength (double p)
			{
				pWorldLength_ = p;
			}
		
			/// <summary>
			/// Constructor, which defines the world pixel length together with
			/// the x-axis that should be held constant when forcing this 
			/// constraint [the other x-axis only will be moved].
			/// </summary>
			/// <param name="p">The world pixel length</param>
			/// <param name="holdFixedX">The position of this x-axis will be held constant. The other x-axis will be moved in order to force the constraint.</param>
			public YPixelWorldLength (double p, AxisPosition holdFixedX)
			{
				pWorldLength_ = p;
				holdFixedX_ = holdFixedX;
			}

			/// <summary>
			/// Applies the constraint to the axes.
			/// </summary>
			/// <param name="pXAxis1">The bottom x-axis.</param>
			/// <param name="pYAxis1">The left y-axis.</param>
			/// <param name="pXAxis2">The top x-axis.</param>
			/// <param name="pYAxis2">The right y-axis.</param>
			public override void ApplyConstraint ( 
				PhysicalAxis pXAxis1, PhysicalAxis pYAxis1, 
				PhysicalAxis pXAxis2, PhysicalAxis pYAxis2 )
			{

				double desiredLength = pYAxis1.Axis.WorldLength/pWorldLength_;
				double currentLength = pYAxis1.PhysicalLength;
				double delta = currentLength - desiredLength;

				double changeBottom = -delta / 2;
				double changeTop = -delta / 2;
				if (holdFixedX_ != null) {
					if ((XAxisPosition)holdFixedX_ == XAxisPosition.Bottom) {
						changeBottom = 0;
						changeTop = -delta;
					}
					else {
						changeBottom = -delta;
						changeTop = 0;
					}
				}

				pYAxis1.PhysicalMin = new Point( pYAxis1.PhysicalMin.X, pYAxis1.PhysicalMin.Y+changeBottom );
				pYAxis1.PhysicalMax = new Point( pYAxis1.PhysicalMax.X, pYAxis1.PhysicalMax.Y-changeTop );
				pYAxis2.PhysicalMin = new Point( pYAxis2.PhysicalMin.X, pYAxis2.PhysicalMin.Y+changeBottom );
				pYAxis2.PhysicalMax = new Point( pYAxis2.PhysicalMax.X, pYAxis2.PhysicalMax.Y-changeTop );

				pXAxis1.PhysicalMin = new Point( pXAxis1.PhysicalMin.X, pXAxis1.PhysicalMin.Y+changeBottom );
				pXAxis1.PhysicalMax = new Point( pXAxis1.PhysicalMax.X, pXAxis1.PhysicalMax.Y+changeBottom );
				pXAxis2.PhysicalMin = new Point( pXAxis2.PhysicalMin.X, pXAxis2.PhysicalMin.Y-changeTop );
				pXAxis2.PhysicalMax = new Point( pXAxis2.PhysicalMax.X, pXAxis2.PhysicalMax.Y-changeTop );

			}

		} // class YPixelWorldLength
		

		/// <summary>
		/// Defines an AxisConstraint that forces the specified axis to be placed at a 
		/// specific physical position. The position of the axis opposite is held 
		/// constant.
		/// </summary>
		public class AxisPosition : AxesConstraint
		{
			private object xAxisPosition_;
			private object yAxisPosition_;
			private double position_;

			
			/// <summary>
			/// Constructor, which defines an horizontal axis and the physical
			/// y position it should be drawn at.
			/// </summary>
			/// <param name="axis">The x-axis for which the y position is to be specified.</param>
			/// <param name="yPosition">The [physical] y position of the axis.</param>
			public AxisPosition (XAxisPosition axis, double yPosition)
			{
				position_ = yPosition;
				xAxisPosition_ = axis;
			}

			/// <summary>
			/// Constructor, which defines a vertical axis and the physical
			/// x position it should be drawn at.
			/// </summary>
			/// <param name="axis">The y-axis for which the x position is to be specified.</param>
			/// <param name="xPosition">The [physical] x position of the axis.</param>
			public AxisPosition (YAxisPosition axis, double xPosition)
			{
				position_ = xPosition;
				yAxisPosition_ = axis;
			}

			/// <summary>
			/// Applies the constraint to the axes.
			/// </summary>
			/// <param name="pXAxis1">The bottom x-axis.</param>
			/// <param name="pYAxis1">The left y-axis.</param>
			/// <param name="pXAxis2">The top x-axis.</param>
			/// <param name="pYAxis2">The right y-axis.</param>
			public override void ApplyConstraint ( 
				PhysicalAxis pXAxis1, PhysicalAxis pYAxis1, 
				PhysicalAxis pXAxis2, PhysicalAxis pYAxis2 )
			{

				if (xAxisPosition_ != null) {

					if ((XAxisPosition)xAxisPosition_ == XAxisPosition.Bottom) {
						pXAxis1.PhysicalMin = new Point (pXAxis1.PhysicalMin.X, position_ );
						pXAxis1.PhysicalMax = new Point (pXAxis1.PhysicalMax.X, position_ );
			
						pYAxis1.PhysicalMin = new Point (pYAxis1.PhysicalMin.X, position_ );
						pYAxis2.PhysicalMin = new Point (pYAxis2.PhysicalMin.X, position_ );
					}
					else {
						pXAxis2.PhysicalMin = new Point (pXAxis2.PhysicalMin.X, position_ );
						pXAxis2.PhysicalMax = new Point (pXAxis2.PhysicalMax.X, position_ );

						pYAxis1.PhysicalMax = new Point (pYAxis1.PhysicalMax.X, position_ );
						pYAxis2.PhysicalMax = new Point (pYAxis2.PhysicalMax.X, position_ );
					}

				}
				else if (yAxisPosition_ != null) {

					if ((YAxisPosition)yAxisPosition_ == YAxisPosition.Left) {
						pYAxis1.PhysicalMin = new Point (position_, pYAxis1.PhysicalMin.Y );
						pYAxis1.PhysicalMax = new Point (position_, pYAxis1.PhysicalMax.Y );

						pXAxis1.PhysicalMin = new Point (position_, pXAxis1.PhysicalMin.Y );
						pXAxis2.PhysicalMin = new Point (position_, pXAxis2.PhysicalMin.Y );
					}
					else {
						pYAxis2.PhysicalMin = new Point (position_, pYAxis2.PhysicalMin.Y );
						pYAxis2.PhysicalMax = new Point (position_, pYAxis2.PhysicalMax.Y );
				
						pXAxis1.PhysicalMax = new Point (position_, pXAxis1.PhysicalMax.Y );
						pXAxis2.PhysicalMax = new Point (position_, pXAxis2.PhysicalMax.Y );
					}

				}
			}

		} // class AxisPosition

		/// <summary>
		/// Defines an axes constraint that forces the world width and height pixel lengths
		/// to be at the provided ratio. For example, an aspect ratio of 3:2 or
		/// 1.5 indicates that there should be 1.5 times as many pixels per fixed
		/// world length along the x direction than for the same world length along
		/// the y direction. In other words, the world length of one pixel along 
		/// the x direction is 2/3rds that of the world length of one pixel height
		/// in the y direction.
		/// </summary>
		/// <remarks>
		/// This class will never increase the size of the plot bounding box. It 
		/// will always be made smaller.
		/// </remarks>
		public class AspectRatio : AxesConstraint
		{
			private double a_;
			private object holdFixedX_ = null;
			private object holdFixedY_ = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="a">Aspect Ratio</param>
			public AspectRatio (double a)
			{
				a_ = a;
			}

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="a">Aspect Ratio</param>
			/// <param name="holdFixedX">
			/// When adjusting the position of axes, the specified axis will never
			/// be moved.
			/// </param>
			public AspectRatio (double a, XAxisPosition holdFixedX)
			{
				a_ = a;
				holdFixedX_ = holdFixedX;
			}

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="a">Aspect Ratio</param>
			/// <param name="holdFixedY">
			/// When adjusting the position of axes, the 
			/// specified axis will never be moved.
			/// </param>
			public AspectRatio (double a, YAxisPosition holdFixedY)
			{
				a_ = a;
				holdFixedY_ = holdFixedY;
			}

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="a">Aspect Ratio</param>
			/// <param name="holdFixedX">When adjusting the position of axes, the specified axis will never be moved.</param>
			/// <param name="holdFixedY">When adjusting the position of axes, the specified axis will never be moved.</param>
			public AspectRatio (double a, XAxisPosition holdFixedX, YAxisPosition holdFixedY)
			{
				a_ = a;
				holdFixedX_ = holdFixedX;
				holdFixedY_ = holdFixedY;
			}
							
			/// <summary>
			/// Applies the constraint to the axes.
			/// </summary>
			/// <param name="pXAxis1">The bottom x-axis.</param>
			/// <param name="pYAxis1">The left y-axis.</param>
			/// <param name="pXAxis2">The top x-axis.</param>
			/// <param name="pYAxis2">The right y-axis.</param>
			public override void ApplyConstraint ( 
				PhysicalAxis pXAxis1, PhysicalAxis pYAxis1, 
				PhysicalAxis pXAxis2, PhysicalAxis pYAxis2 )
			{
				double xWorldRange = Math.Abs (pXAxis1.Axis.WorldMax - pXAxis1.Axis.WorldMin);
				double xPhysicalRange = Math.Abs (pXAxis1.PhysicalMax.X - pXAxis1.PhysicalMin.X);
				double xDirPixelSize =	xWorldRange / xPhysicalRange;
			
				double yWorldRange = Math.Abs (pYAxis1.Axis.WorldMax - pYAxis1.Axis.WorldMin);
				double yPhysicalRange = Math.Abs (pYAxis1.PhysicalMax.Y - pYAxis1.PhysicalMin.Y);
				double yDirPixelSize =	yWorldRange / yPhysicalRange;

				double currentAspectRatio = yDirPixelSize / xDirPixelSize;

				// we want to change the current aspect ratio to be the desired.
				// to do this, we may only add the world pixel lengths.

				if (a_ > currentAspectRatio) {
					// want to increase aspect ratio. Therefore, want to add some amount
					// to yDirPixelSize (numerator).

					double toAdd = (a_ - currentAspectRatio) * xDirPixelSize;
					double newHeight =
						Math.Abs (pYAxis1.Axis.WorldMax - pYAxis1.Axis.WorldMin) / (yDirPixelSize + toAdd);
					double changeInHeight = yPhysicalRange - newHeight;

					double changeBottom = changeInHeight/2;
					double changeTop = changeInHeight/2;
					if (holdFixedX_ != null) {
						if ((XAxisPosition)holdFixedX_ == XAxisPosition.Bottom) {
							changeBottom = 0;
							changeTop = changeInHeight;
						}
						else { 
							changeBottom = changeInHeight;
							changeTop = 0;
						}
					}

					pYAxis1.PhysicalMin = new Point (pYAxis1.PhysicalMin.X, pYAxis1.PhysicalMin.Y-changeBottom );
					pYAxis1.PhysicalMax = new Point (pYAxis1.PhysicalMax.X, pYAxis1.PhysicalMax.Y+changeTop );
					pYAxis2.PhysicalMin = new Point (pYAxis2.PhysicalMin.X, pYAxis2.PhysicalMin.Y-changeBottom );
					pYAxis2.PhysicalMax = new Point (pYAxis2.PhysicalMax.X, pYAxis2.PhysicalMax.Y+changeTop );

					pXAxis1.PhysicalMin = new Point (pXAxis1.PhysicalMin.X, pXAxis1.PhysicalMin.Y-changeBottom );
					pXAxis1.PhysicalMax = new Point (pXAxis1.PhysicalMax.X, pXAxis1.PhysicalMax.Y-changeBottom );
					pXAxis2.PhysicalMin = new Point (pXAxis2.PhysicalMin.X, pXAxis2.PhysicalMin.Y+changeTop );
					pXAxis2.PhysicalMax = new Point (pXAxis2.PhysicalMax.X, pXAxis2.PhysicalMax.Y+changeTop );

				}
				else {

					// want to decrease aspect ratio. Therefore, want to add some amount
					// to xDirPixelSize (denominator).

					double toAdd = yDirPixelSize / this.a_ - xDirPixelSize;
					double newWidth = 
						Math.Abs(pXAxis1.Axis.WorldMax - pXAxis1.Axis.WorldMin) / (xDirPixelSize + toAdd);
					double changeInWidth = xPhysicalRange - newWidth;

					double changeLeft = changeInWidth / 2;
					double changeRight = changeInWidth / 2;
					if (holdFixedY_ != null) {
						if ((YAxisPosition)holdFixedY_ == YAxisPosition.Left) {
							changeLeft = 0;
							changeRight = changeInWidth;
						} else {
							changeLeft = changeInWidth;
							changeRight = 0;
						}
					}

					pXAxis1.PhysicalMin = new Point (pXAxis1.PhysicalMin.X+changeLeft, pXAxis1.PhysicalMin.Y );
					pXAxis1.PhysicalMax = new Point (pXAxis1.PhysicalMax.X-changeRight, pXAxis1.PhysicalMax.Y );
					pXAxis2.PhysicalMin = new Point (pXAxis2.PhysicalMin.X+changeLeft, pXAxis2.PhysicalMin.Y );
					pXAxis2.PhysicalMax = new Point (pXAxis2.PhysicalMax.X-changeRight, pXAxis2.PhysicalMax.Y );

					pYAxis1.PhysicalMin = new Point (pYAxis1.PhysicalMin.X+changeLeft, pYAxis1.PhysicalMin.Y );
					pYAxis1.PhysicalMax = new Point (pYAxis1.PhysicalMax.X+changeLeft, pYAxis1.PhysicalMax.Y );
					pYAxis2.PhysicalMin = new Point (pYAxis2.PhysicalMin.X-changeRight, pYAxis2.PhysicalMin.Y );
					pYAxis2.PhysicalMax = new Point (pYAxis2.PhysicalMax.X-changeRight, pYAxis2.PhysicalMax.Y );

				}

			}

		} // class AspectRatio
		

	} // AxesConstraint

}
