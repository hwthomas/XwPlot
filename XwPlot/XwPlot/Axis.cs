//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// Axis.cs
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
	/// Encapsulates functionality common to all axis classes. All specific axis classes
	/// derive from Axis. Axis can be used as a concrete class itself - it is an Axis
	/// without any embellishments [tick marks or tick mark labels]
	/// </summary>
	/// <remarks>
	/// This class encapsulates no physical information about where the axes are drawn. 
	/// </remarks>
	public class Axis : System.ICloneable
	{
		private double worldMax;
		private double worldMin;

        private Font tickTextFont;
        private Font labelFont;

		private double fontScale;
		private Font tickTextFontScaled;
		private Font labelFontScaled;

		/// <summary>
		/// If set to true, the axis is hidden. That is, the axis line, ticks, tick 
		/// labels and axis label will not be drawn. 
		/// </summary>
		public bool Hidden { get; set; }

		/// <summary>
		/// If set true, the axis will behave as though the WorldMin and WorldMax values
		/// have been swapped.
		/// </summary>
		/// <remarks>
		/// This may well change with a rewrite of the drawing routine - and this property disappear
		/// </remarks>
		public bool Reversed { get; set; }

		/// <summary>
		/// The maximum world extent of the axis. Note that it makes sense for WorldMax to
		/// be less than WorldMin - the axis would just be descending rather than ascending.
		/// Currently, however, Axes won't display properly if you do this - use the
		/// Axis.Reversed property instead to achieve the same result.
		/// </summary>
		/// <remarks>
		/// Setting this raises the WorldMinChanged event and the WorldExtentsChanged event.
		/// </remarks>
		public virtual double WorldMax
		{
			get { return worldMax; }
			set {
				worldMax = value;
				//
				// if (WorldExtentsChanged != null)
				// WorldExtentsChanged (this, new WorldValueChangedArgs(worldMax, WorldValueChangedArgs.MinMaxType.Max));
				// if (WorldMaxChanged != null)
				//		WorldMaxChanged(this, new WorldValueChangedArgs(worldMax, WorldValueChangedArgs.MinMaxType.Max));
				//
			}
		}
		
		/// <summary>
		/// The minumum world extent of the axis. Note that it woiuld be sensible if 
		/// WorldMax is less than WorldMin - the axis would just be descending rather
		/// than not ascending. Currently, however, Axes won't display properly if you
		/// do this - use Axis.Reversed property instead to achieve the same result.
		/// </summary>
		/// <remarks> 
		/// Setting this should (sometime!) raise the WorldMinChanged and WorldExtentsChanged events
		/// </remarks>
		public virtual double WorldMin
		{
			get { return worldMin; }
			set {
				worldMin = value;
				// if (WorldExtentsChanged != null)
				// 	WorldExtentsChanged( this, new WorldValueChangedArgs( worldMin, WorldValueChangedArgs.MinMaxType.Min) );
				// if (WorldMinChanged != null)
				// WorldMinChanged( this, new WorldValueChangedArgs(worldMin, WorldValueChangedArgs.MinMaxType.Min) );
				//
			}
		}

		/// <summary>
		/// Length (in pixels) of a large tick. Not the distance 
		/// between large ticks, but the length of the tick itself
		/// </summary>
		public double LargeTickSize { get; set; }

		/// <summary>
		/// Length (in pixels) of the small ticks.
		/// </summary>
		public double SmallTickSize { get; set; }

		/// <summary>
		/// If true, text associated with tick marks will be drawn on the other side of the
		/// axis line [next to the axis]. If false, tick mark text will be drawn at the end
		/// of the tick mark [on the same of the axis line as the tick].
		/// </summary>
		public bool TickTextNextToAxis { get; set; }

		/// <summary>
		/// If true, tick marks will cross the axis, with their centre on the axis line.
		/// If false, tick marks will be drawn as a line with origin starting on the axis line.
		/// </summary>
		public bool TicksCrossAxis { get; set; }

		/// <summary>
		/// If true, no text will be drawn next to any axis tick marks.
		/// </summary>
		public bool HideTickText { get; set; }

		/// <summary>
		/// This font is used for the drawing of text next to the axis tick marks.
		/// </summary>
		public Font TickTextFont
		{
			get { return tickTextFont; }
			set { 
				tickTextFont = value;
				UpdateScale();
			}
		}

		/// <summary>
		/// Specifies the format used for drawing tick text. See 
		/// StringBuilder.AppendFormat for a description of this 
		/// string.
		/// </summary>
		public string NumberFormat { get; set; }

		/// <summary>
		/// The Axis Label
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// This font is used to draw the axis label.
		/// </summary>
		public Font LabelFont
		{
			get { return labelFont; }
			set {
				labelFont = value;
				UpdateScale();
			}
		}

		/// <summary>
		/// If LargeTickStep isn't specified, then this will be calculated 
		/// automatically. The calculated value will not be less than this
		/// amount.
		/// </summary>
		public double MinPhysicalLargeTickStep { get; set; }

		/// <summary>
		/// If true, automated tick placement will be independent of the physical
		/// extent of the axis. Tick placement will look good for charts of typical
		/// size (say physical dimensions of 640x480). If you want to produce the
		/// same chart on two graphics surfaces of different sizes [eg Windows.Forms
		/// control and printer], then you will want to set this property to true.
		/// If false [default], the number of ticks and their placement will be 
		/// optimally calculated to look the best for the given axis extent. This 
		/// is very useful if you are creating a cart with particularly small or
		/// large physical dimensions.
		/// </summary>
		public bool TicksIndependentOfPhysicalExtent { get; set; }

		/// <summary>
		/// If true, Tick Text is flipped about the text center line parallel to the text.
		/// </summary>
		public bool FlipTickText { get; set; }

		/// <summary>
		/// Angle to draw ticks at (measured anti-clockwise from axis direction)
		/// The default of 3*PI/2 places X-axis ticks below the axis, and Y-axis
		/// ticks to the left of the axis, ie outside the actual plotting area
		/// </summary>
		public double TicksAngle { get; set; }

		/// <summary>
		/// Angle to draw large tick labels at (clockwise from horizontal). Note: 
		/// this is currently only implemented well for the lower x-axis. 
		/// </summary>
		public double TickTextAngle { get; set; }

		/// <summary>
		/// The color used to draw the ticks and the axis line.
		/// </summary>
		public Color LineColor { get; set; }

		/// <summary>
		/// The color used to draw the axis tick text.
		/// </summary>
		public Color TickTextColor { get; set; }

		/// <summary>
		/// The color used to draw the axis label.
		/// </summary>
		public Color LabelColor { get; set; }

		/// <summary>
		/// Set the Axis color (sets all of axis Line color, Tick text color, and Label color).
		/// </summary>
		public void SetColor (Color color)
		{
			LineColor = color;
			TickTextColor = color;
			LabelColor = color;
		}

		/// <summary>
		/// Tick lengths will be scaled to match the PlotSurface size.
		/// </summary>
		/// <remarks>Could also be argued this belongs in PlotSurface</remarks>
		public bool AutoScaleTicks { get; set; }

		/// <summary>
		/// If true, label and tick text will be scaled to match size
		/// </summary>
		/// <remarks>Could also be argued this belongs in PlotSurface</remarks>
		public bool AutoScaleText { get; set; }

		/// <summary>
		/// Scale label and tick fonts by this factor. Set by PlotSurface 
		/// Draw method.
		/// </summary>
		internal double FontScale 
		{
			get { return fontScale; }
			set { 
				fontScale = value;
				UpdateScale();
			}
		}

		/// <summary>
		/// Scale tick mark lengths by this factor. Set by PlotSurface
		/// Draw method.
		/// </summary>		
		internal double TickScale { get; set; }

		private void UpdateScale()	
		{
			if (labelFont != null) {
				labelFontScaled = Utils.ScaleFont (labelFont, FontScale);
			}

			if (tickTextFont != null) {
				tickTextFontScaled = Utils.ScaleFont (tickTextFont, FontScale);
			}
		}

		/// <summary>
		/// Default behaviour is to return true.  Override if otherwise.
		/// </summary>
		public virtual bool IsLinear
		{
			get { return true; }
		}

		/// <summary>
		/// If LabelOffsetAbsolute is false (default) then this is the offset 
		/// added to default axis label position. If LabelOffsetAbsolute is 
		/// true, then this is the absolute offset of the label from the axis.
		/// 
		/// If positive, offset is further away from axis, if negative, towards
		/// the axis.
		/// </summary>
		public double LabelOffset { get; set; }

		/// <summary>
		/// If true, the value specified by LabelOffset is the absolute distance
		/// away from the axis that the label is drawn. If false, the value 
		/// specified by LabelOffset is added to the pre-calculated value to 
		/// determine the axis label position.
		/// </summary>
		/// <value></value>
		public bool LabelOffsetAbsolute { get; set; }

		/// <summary>
		/// Whether or not the supplied LabelOffset should be scaled by 
		/// a factor as specified by FontScale.
		/// </summary>
		public bool LabelOffsetScaled { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public Axis ()
		{
			Init();
		}

		/// <summary>
		/// Constructor that takes only world min and max values.
		/// </summary>
		/// <param name="worldMin">The minimum world coordinate.</param>
		/// <param name="worldMax">The maximum world coordinate.</param>
		public Axis (double worldMin, double worldMax)
		{
			Init();
			WorldMin = worldMin;
			WorldMax = worldMax;
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="src">The Axis to clone.</param>
		public Axis (Axis src )
		{
			Axis.DoClone (src, this);
		}

		/// <summary>
		/// Helper function for constructors.
		/// Initialize all properties here so that Clear() method is handled properly
		/// </summary>
		private void Init()
		{
			Reversed = false;
			Hidden = false;
			LineColor = Colors.Black;

			worldMax = double.NaN;
			worldMin = double.NaN;

			TicksCrossAxis = false;
			SmallTickSize = 2;
			LargeTickSize = 6;
			MinPhysicalLargeTickStep = 30;
			TicksIndependentOfPhysicalExtent = false;
			AutoScaleTicks = false;
			TickScale = 1.0;
			TicksAngle = Math.PI * 3.0 / 2.0;

			HideTickText = false;
			TickTextNextToAxis = false;
			FlipTickText = false;
			AutoScaleText = false;
			FontScale = 1.0;
			TickTextColor = Colors.Black;
			TickTextFont = Font.SystemSansSerifFont.WithSize (10);
			TickTextAngle = 0;

			Label = "" ;
			LabelColor = Colors.Black;
			LabelFont = Font.SystemSansSerifFont.WithSize (12);
			LabelOffset = 0.0;
			LabelOffsetAbsolute = false;
			LabelOffsetScaled = true;

			NumberFormat = null;

		}

		/// <summary>
		/// Deep copy of Axis.
		/// </summary>
		/// <remarks>
		/// This method includes a check that guards against derived classes forgetting
		/// to implement their own Clone method. If Clone is called on a object derived
		/// from Axis, and the Clone method hasn't been overridden by that object, then
		/// the test GetType == typeof(Axis) will fail.
		/// </remarks>
		/// <returns>A copy of the Axis Class</returns>
		public virtual object Clone()
		{
			// ensure that this isn't being called on a derived type. If that is the case
			// then the derived type didn't override this method as it should have.
			if (GetType() != typeof(Axis)) {
				throw new XwPlotException ( "Clone not defined in derived type." );
			}

			Axis a = new Axis ();
			DoClone (this, a);
			return a;
		}

		/// <summary>
		/// Helper method for Clone. Does all the copying - can be called by derived
		/// types so they don't need to implement this part of the copying themselves.
		/// also useful in constructor of derived types that takes Axis class.
		/// </summary>
		protected static void DoClone (Axis src, Axis dest)
		{
			dest.Reversed = src.Reversed;
			dest.Hidden = src.Hidden;
			dest.WorldMax = src.WorldMax;
			dest.WorldMin = src.WorldMin;
			dest.LineColor = src.LineColor;

			dest.TicksCrossAxis = src.TicksCrossAxis;
			dest.SmallTickSize = src.SmallTickSize;
			dest.LargeTickSize = src.LargeTickSize;
			dest.MinPhysicalLargeTickStep = src.MinPhysicalLargeTickStep;
			dest.TicksIndependentOfPhysicalExtent = src.TicksIndependentOfPhysicalExtent;
			dest.AutoScaleTicks = src.AutoScaleTicks;
			dest.TickScale = src.TickScale;
			dest.TicksAngle = src.TicksAngle;

			dest.HideTickText = src.HideTickText;
			dest.TickTextNextToAxis = src.TickTextNextToAxis;
			dest.FlipTickText = src.FlipTickText;
			dest.AutoScaleText = src.AutoScaleText;
			dest.FontScale = src.FontScale;

			dest.TickTextColor = src.TickTextColor;
			dest.TickTextFont = src.TickTextFont.WithScaledSize (1.0);
			dest.TickTextAngle = src.TickTextAngle;

			dest.Label = (string)src.Label.Clone();
			dest.LabelColor = src.LabelColor;
			dest.LabelFont = src.LabelFont.WithScaledSize (1.0);
			dest.LabelOffset = src.LabelOffset;
			dest.LabelOffsetAbsolute = src.LabelOffsetAbsolute;
			dest.LabelOffsetScaled = src.LabelOffsetScaled;

			dest.NumberFormat = src.NumberFormat;

		}

		/// <summary>
		/// returns a suitable offset for the axis label in the case that there are no
		/// ticks or tick text in the way.
		/// </summary>
		/// <param name="physicalMin">physical point corresponding to the axis world maximum.</param>
		/// <param name="physicalMax">physical point corresponding to the axis world minimum.</param>
		/// <returns>axis label offset</returns>
		protected Point getDefaultLabelOffset (Point physicalMin, Point physicalMax)
		{
			Rectangle tBoundingBox;
			Point tLabelOffset;

			using (ImageBuilder ib = new ImageBuilder (1,1)) {
				using (Context ctx = ib.Context) {
					DrawTick (ctx, WorldMax, LargeTickSize, 
						"",
						new Point (0,0),
						physicalMin, physicalMax,
						out tLabelOffset, out tBoundingBox );
				}
			}
			return tLabelOffset;
		}

		/// <summary>
		/// Determines whether a world value is outside range WorldMin -> WorldMax
		/// </summary>
		/// <param name="coord">the world value to test</param>
		/// <returns>true if outside limits, false otherwise</returns>
		public bool OutOfRange( double coord )
		{
			if (double.IsNaN(WorldMin) || double.IsNaN(WorldMax)) {
				throw new XwPlotException( "world min / max not set" );
			}

			if (coord > WorldMax || coord < WorldMin) {
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>
		/// Sets the world extent of the current axis to be just large enough (Least Upper Bound) to
		/// encompass the current world extent of the axis, and the extent of the specified axis
		/// </summary>
		/// <param name="a">The other Axis instance.</param>
		public void LUB (Axis a)
		{ 
			if (a == null) {
				return;
			}
			// minima
			if (!double.IsNaN(a.WorldMin)) {
				if (double.IsNaN(WorldMin)) {
					WorldMin = a.WorldMin;
				}
				else  {
					if (a.WorldMin < WorldMin) {
						WorldMin = a.WorldMin;
					}
				}
			}
			// maxima
			if (!double.IsNaN(a.WorldMax)) {
				if (double.IsNaN(WorldMax)) {
					WorldMax = a.WorldMax;
				}
				else {
					if (a.WorldMax > WorldMax) {
						WorldMax = a.WorldMax;
					}
				}
			}
		}

		/// <summary>
		/// World to physical coordinate transform.
		/// </summary>
		/// <param name="coord">The coordinate value to transform.</param>
		/// <param name="physicalMin">The physical position corresponding to the world minimum of the axis.</param>
		/// <param name="physicalMax">The physical position corresponding to the world maximum of the axis.</param>
		/// <param name="clip">if false, then physical value may extend outside worldMin / worldMax. If true,
		/// 			 the physical value returned will be clipped to physicalMin or physicalMax</param>
		/// <returns>The transformed coordinates.</returns>
		/// <remarks>Not sure how much time is spent in this often called function. If it's lots, then
		/// worth optimizing (there is scope to do so).</remarks>
		public virtual Point WorldToPhysical (double coord, Point physicalMin, Point physicalMax, bool clip)
		{
			// (1) account for reversed axis. Could be tricky and move
			// this out, but would be a little messy.
			Point _physicalMin;
			Point _physicalMax;

			if (Reversed) {
				_physicalMin = physicalMax;
				_physicalMax = physicalMin;
			}
			else {
				_physicalMin = physicalMin;
				_physicalMax = physicalMax;
			}

			// (2) if want clipped value, return extrema if outside range.
			if (clip) {
				if (WorldMin < WorldMax) {
					if (coord > WorldMax) {
						return _physicalMax;
					}
					if (coord < WorldMin) {
						return _physicalMin;
					}
				}
				else {
					if (coord < WorldMax) {
						return _physicalMax;
					}
					if (coord > WorldMin) {
						return _physicalMin;
					}
				}
			}

			// (3) we are inside range or don't want to clip.
			double range = WorldMax - WorldMin;
			double prop = (coord - WorldMin)/range;

			// Force clipping at bounding box largeClip times that of real bounding box 
			// anyway. This is effectively at infinity.
			double largeClip = 100.0;
			if (prop > largeClip && clip) {
				prop = largeClip;
			}
			if (prop < -largeClip && clip) {
				prop = -largeClip;
			}
			if (range == 0) {
				if (coord >= WorldMin) {
					prop = largeClip;
				}
				else {
					prop = -largeClip;
				}
			}
			// calculate the physical coordinate.
			Point offset = new Point ( 
				prop * (_physicalMax.X - _physicalMin.X),
				prop * (_physicalMax.Y - _physicalMin.Y) );

			return new Point (_physicalMin.X+offset.X, _physicalMin.Y+offset.Y);
		}

		/// <summary>
		/// Return the world coordinate of the projection of the point p onto
		/// the axis.
		/// </summary>
		/// <param name="p">The point to project onto the axis</param>
		/// <param name="physicalMin">The physical position corresponding to the world minimum of the axis.</param>
		/// <param name="physicalMax">The physical position corresponding to the world maximum of the axis.</param>
		/// <param name="clip">If true, the world value will be clipped to WorldMin or WorldMax as appropriate.</param>
		/// <returns>The world value corresponding to the projection of the point p onto the axis.</returns>
		public virtual double PhysicalToWorld (Point p, Point physicalMin, Point physicalMax, bool clip)
		{
			// (1) account for reversed axis. Could be tricky and move
			// this out, but would be a little messy.
			Point _physicalMin;
			Point _physicalMax;

			if (Reversed) {
				_physicalMin = physicalMax;
				_physicalMax = physicalMin;
			}
			else {
				_physicalMin = physicalMin;
				_physicalMax = physicalMax;
			}

			// normalised axis dir vector
			double axis_X = _physicalMax.X - _physicalMin.X;
			double axis_Y = _physicalMax.Y - _physicalMin.Y;
			double len = Math.Sqrt (axis_X * axis_X + axis_Y * axis_Y);
			axis_X /= len;
			axis_Y /= len;

			// point relative to axis physical minimum.
			Point posRel = new Point (p.X - _physicalMin.X, p.Y - _physicalMin.Y);

			// dist of point projection on axis, normalised.
			double prop = (axis_X * posRel.X + axis_Y * posRel.Y) / len;
			double world = prop * (WorldMax - WorldMin) + WorldMin;

			// if want clipped value, return extrema if outside range.
			if (clip) {
				world = Math.Max (world, WorldMin);
				world = Math.Min (world, WorldMax);
			}
			return world;
		}

		/// <summary>
		/// Draw the Axis Label
		/// </summary>
		/// <param name="ctx>The Drawing Context with which to draw.</param>
		/// <param name="offset">offset from axis. Should be calculated so as to make sure axis label misses tick labels.</param>
		/// <param name="axisPhysicalMin">The physical position corresponding to the world minimum of the axis.</param>
		/// <param name="axisPhysicalMax">The physical position corresponding to the world maximum of the axis.</param>
		/// <returns>boxed Rectangle indicating bounding box of label. null if no label printed.</returns>
		public object DrawLabel (Context ctx, Point offset, Point axisPhysicalMin, Point axisPhysicalMax)
		{
			if (Label != "") {
	
				// first calculate any extra offset for axis label spacing.
				double extraOffsetAmount = LabelOffset;
				extraOffsetAmount += 2; // empirically determed - text was too close to axis before this.
				if (AutoScaleText && LabelOffsetScaled) {
					extraOffsetAmount *= FontScale;
				}
				// now extend offset.
				double offsetLength = Math.Sqrt (offset.X*offset.X + offset.Y*offset.Y);
				if (offsetLength > 0.01) {
					double x_component = offset.X / offsetLength;
					double y_component = offset.Y / offsetLength;

					x_component *= extraOffsetAmount;
					y_component *= extraOffsetAmount;

					if (LabelOffsetAbsolute) {
						offset.X = x_component;
						offset.Y = y_component;
					}
					else {
						offset.X += x_component;
						offset.Y += y_component;
					}
				}
				
				// determine angle of axis in degrees
				double theta = Math.Atan2 (
					axisPhysicalMax.Y - axisPhysicalMin.Y,
					axisPhysicalMax.X - axisPhysicalMin.X);
				theta = theta * 180.0 / Math.PI;

				Point average = new Point (
					(axisPhysicalMax.X + axisPhysicalMin.X)/2,
					(axisPhysicalMax.Y + axisPhysicalMin.Y)/2);

				ctx.Save ();

				ctx.Translate (average.X + offset.X , average.Y + offset.Y);	// this is done last.
				ctx.Rotate (theta);												// this is done first.

				TextLayout layout = new TextLayout ();
				layout.Font = labelFontScaled;
				layout.Text = Label;
				Size labelSize = layout.GetSize ();

				//Draw label centered around zero.
				ctx.DrawTextLayout (layout, -labelSize.Width/2, -labelSize.Height/2);

				// now work out physical bounds of Rotated and Translated label. 
				Point [] recPoints = new Point [2];
				recPoints[0] = new Point (-labelSize.Width/2, -labelSize.Height/2);
				recPoints[1] = new Point ( labelSize.Width/2, labelSize.Height/2);
				ctx.TransformPoints (recPoints);

				double x1 = Math.Min (recPoints[0].X, recPoints[1].X);
				double x2 = Math.Max (recPoints[0].X, recPoints[1].X);
				double y1 = Math.Min (recPoints[0].Y, recPoints[1].Y);
				double y2 = Math.Max (recPoints[0].Y, recPoints[1].Y);

				ctx.Restore ();

				// and return label bounding box.
				return new Rectangle (x1, y1, (x2-x1), (y2-y1));
			}
			return null;
		}

		/// <summary>
		/// Draw a tick on the axis.
		/// </summary>
		/// <param name="ctx">The Drawing Context with on which to draw.</param>
		/// <param name="w">The tick position in world coordinates.</param>
		/// <param name="size">The size of the tick (in pixels)</param>
		/// <param name="text">The text associated with the tick</param>
		/// <param name="textOffset">The Offset to draw from the auto calculated position</param>
		/// <param name="axisPhysMin">The minimum physical extent of the axis</param>
		/// <param name="axisPhysMax">The maximum physical extent of the axis</param>
		/// <param name="boundingBox">out: The bounding rectangle for the tick and tickLabel drawn</param>
		/// <param name="labelOffset">out: offset from the axies required for axis label</param>
		public virtual void DrawTick ( 
			Context ctx, 
			double w,
			double size,
			string text,
			Point textOffset,
			Point axisPhysMin,
			Point axisPhysMax,
			out Point labelOffset,
			out Rectangle boundingBox )
		{

			// determine physical location where tick touches axis. 
			Point tickStart = WorldToPhysical (w, axisPhysMin, axisPhysMax, true);

			// determine offset from start point.
			Point  axisDir = Utils.UnitVector (axisPhysMin, axisPhysMax);

			// rotate axisDir anti-clockwise by TicksAngle radians to get tick direction. Note that because
			// the physical (pixel) origin is at the top left, a RotationTransform by a positive angle will
			// be clockwise.  Consequently, for anti-clockwise rotations, use cos(A-B), sin(A-B) formulae
			double x1 = Math.Cos (TicksAngle) * axisDir.X + Math.Sin (TicksAngle) * axisDir.Y;
			double y1 = Math.Cos (TicksAngle) * axisDir.Y - Math.Sin (TicksAngle) * axisDir.X;

			// now get the scaled tick vector.
			Point tickVector = new Point (TickScale * size * x1, TickScale * size * y1);

			if (TicksCrossAxis) {
				tickStart.X -= tickVector.X / 2;
				tickStart.Y -= tickVector.Y / 2;
			}

			// and the end point [point off axis] of tick mark.
			Point  tickEnd = new Point (tickStart.X + tickVector.X, tickStart.Y + tickVector.Y);

			// and draw it
			ctx.SetLineWidth (1);
			ctx.SetColor (LineColor);
			ctx.MoveTo (tickStart.X+0.5, tickStart.Y+0.5);
			ctx.LineTo (tickEnd.X+0.5, tickEnd.Y+0.5);
			ctx.Stroke ();

			// calculate bounds of tick.
			double minX = Math.Min (tickStart.X, tickEnd.X);
			double minY = Math.Min (tickStart.Y, tickEnd.Y);
			double maxX = Math.Max (tickStart.X, tickEnd.X);
			double maxY = Math.Max (tickStart.Y, tickEnd.Y);
			boundingBox = new Rectangle (minX, minY, maxX-minX, maxY-minY);
			
			// by default, label offset from axis is 0. TODO: revise this.
			labelOffset = new Point (-tickVector.X, -tickVector.Y);

			// ------------------------

			// now draw associated text.

			// **** TODO ****
			// The following code needs revising. A few things are hard coded when
			// they should not be. Also, angled tick text currently just works for
			// the bottom x-axis. Also, it's a bit hacky.

			if (text != "" && !HideTickText) {
				TextLayout layout = new TextLayout ();
				layout.Font = tickTextFontScaled;
				layout.Text = text;
				Size textSize = layout.GetSize ();

				// determine the center point of the tick text.
				double textCenterX;
				double textCenterY;

				// if text is at pointy end of tick.
				if (!TickTextNextToAxis) {
					// offset due to tick.
					textCenterX = tickStart.X + tickVector.X*1.2;
					textCenterY = tickStart.Y + tickVector.Y*1.2;

					// offset due to text box size.
					textCenterX += 0.5 * x1 * textSize.Width;
					textCenterY += 0.5 * y1 * textSize.Height;
				}
					// else it's next to the axis.
				else {
					// start location.
					textCenterX = tickStart.X;
					textCenterY = tickStart.Y;

					// offset due to text box size.
					textCenterX -= 0.5 * x1 * textSize.Width;
					textCenterY -= 0.5 * y1 * textSize.Height;

					// bring text away from the axis a little bit.
					textCenterX -= x1*(2.0+FontScale);
					textCenterY -= y1*(2.0+FontScale);
				}

				// If tick text is angled.. 
				if (TickTextAngle != 0) {

					// determine the point we want to rotate text about.
					Point textScaledTickVector = new Point (
												TickScale * x1 * (textSize.Height/2),
												TickScale * y1 * (textSize.Height/2) );
					Point rotatePoint;
					if (TickTextNextToAxis) {
						rotatePoint = new Point (
											tickStart.X - textScaledTickVector.X,
											tickStart.Y - textScaledTickVector.Y);
					}
					else {
						rotatePoint = new Point (
											tickEnd.X + textScaledTickVector.X,
											tickEnd.Y + textScaledTickVector.Y);
					}
 
					double actualAngle;
					if (FlipTickText) {
						double radAngle = TickTextAngle * Math.PI / 180;
						rotatePoint.X += textSize.Width * Math.Cos (radAngle);
						rotatePoint.Y += textSize.Width * Math.Sin (radAngle);
						actualAngle = TickTextAngle + 180;
					}
					else {
						actualAngle = TickTextAngle;
					}
					
					ctx.Save ();

					ctx.Translate (rotatePoint.X, rotatePoint.Y);
					ctx.Rotate (actualAngle);
					
					Point [] recPoints = new Point [2];
					recPoints[0] = new Point (0.0, -textSize.Height/2);
					recPoints[1] = new Point (textSize.Width, textSize.Height);
					ctx.TransformPoints (recPoints);

					double t_x1 = Math.Min (recPoints[0].X, recPoints[1].X);
					double t_x2 = Math.Max (recPoints[0].X, recPoints[1].X);
					double t_y1 = Math.Min (recPoints[0].Y, recPoints[1].Y);
					double t_y2 = Math.Max (recPoints[0].Y, recPoints[1].Y);
					
					boundingBox = Rectangle.Union (boundingBox, new Rectangle (t_x1, t_y1, (t_x2-t_x1), (t_y2-t_y1)));

					ctx.DrawTextLayout (layout, 0, -textSize.Height/2);

					t_x2 -= tickStart.X;
					t_y2 -= tickStart.Y;
					t_x2 *= 1.25;
					t_y2 *= 1.25;

					labelOffset = new Point (t_x2, t_y2);

					ctx.Restore ();

					//ctx.Rectangle (boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
					//ctx.Stroke ();

				}
				else 				{
					double bx1 = (textCenterX - textSize.Width/2);
					double by1 = (textCenterY - textSize.Height/2);
					double bx2 = textSize.Width;
					double by2 = textSize.Height;

					Rectangle drawRect = new Rectangle (bx1, by1, bx2, by2);
					// ctx.Rectangle (drawRect);

					boundingBox = Rectangle.Union (boundingBox, drawRect);

					// ctx.Rectangle (boundingBox);

					ctx.DrawTextLayout (layout, bx1, by1);

					textCenterX -= tickStart.X;
					textCenterY -= tickStart.Y;
					textCenterX *= 2.3;
					textCenterY *= 2.3;

					labelOffset = new Point (textCenterX, textCenterY);
				}
			} 
		}


		/// <summary>
		/// Draw the axis. This involves three steps:
		///	 (1) Draw the axis line.
		///	 (2) Draw the tick marks.
		///	 (3) Draw the label.
		/// </summary>
		/// <param name="ctx">The Drawing Context with which to draw.</param>
		/// <param name="physicalMin">The physical position corresponding to the world minimum of the axis.</param>
		/// <param name="physicalMax">The physical position corresponding to the world maximum of the axis.</param>
		/// <param name="boundingBox">out The bounding rectangle of the axis including axis line, label, tick marks and tick mark labels</param>
		public virtual void Draw (Context ctx, Point physicalMin, Point physicalMax, out Rectangle boundingBox)
		{
			// calculate the bounds of the axis line only.
			double x1 = Math.Min (physicalMin.X, physicalMax.X);
			double x2 = Math.Max (physicalMin.X, physicalMax.X);
			double y1 = Math.Min (physicalMin.Y, physicalMax.Y);
			double y2 = Math.Max (physicalMin.Y, physicalMax.Y);
			Rectangle bounds = new Rectangle (x1, y1, x2-x1, y2-y1);

			if (!Hidden) {
				// (1) Draw the axis line.
				ctx.Save ();
				ctx.SetLineWidth (1);
				ctx.SetColor (LineColor);
				ctx.MoveTo (physicalMin.X+0.5, physicalMin.Y+0.5);
				ctx.LineTo (physicalMax.X+0.5, physicalMax.Y+0.5);
				ctx.Stroke ();
				ctx.Restore ();

				// (2) draw tick marks (subclass responsibility). 

				object labelOffset;
				object tickBounds;
				DrawTicks (ctx, physicalMin, physicalMax, out labelOffset, out tickBounds);

				// (3) draw the axis label
				object labelBounds = null;
				if (!HideTickText) {
					labelBounds = DrawLabel (ctx, (Point)labelOffset, physicalMin, physicalMax);
				}

				// (4) merge bounds and return.
				if (labelBounds != null) {
					bounds = Rectangle.Union (bounds, (Rectangle)labelBounds);
				}

				if (tickBounds != null) {
					bounds = Rectangle.Union (bounds, (Rectangle)tickBounds);
				}
			}
			boundingBox = bounds;
		}

		/// <summary>
		/// Update the bounding box and label offset associated with an axis
		/// to encompass the additionally specified mergeBoundingBox and 
		/// mergeLabelOffset respectively.
		/// </summary>
		/// <param name="labelOffset">Current axis label offset.</param>
		/// <param name="boundingBox">Current axis bounding box.</param>
		/// <param name="mergeLabelOffset">the label offset to merge. The current label offset will be replaced by this if it's norm is larger.</param>
		/// <param name="mergeBoundingBox">the bounding box to merge. The current bounding box will be replaced by this if null, or by the least upper bound of bother bounding boxes otherwise.</param>
		protected static void UpdateOffsetAndBounds (ref object labelOffset, ref object boundingBox, Point mergeLabelOffset, Rectangle mergeBoundingBox)
		{
			// determining largest label offset and use it.
			Point lo = (Point)labelOffset;
			double norm1 = Math.Sqrt( lo.X*lo.X + lo.Y*lo.Y );
			double norm2 = Math.Sqrt( mergeLabelOffset.X*mergeLabelOffset.X + mergeLabelOffset.Y*mergeLabelOffset.Y );
			if (norm1 < norm2) {
				labelOffset = mergeLabelOffset;
			}
			// determining bounding box.
			Rectangle b = mergeBoundingBox;
			if (boundingBox == null) {
				boundingBox = b;
			}
			else {
				boundingBox = Rectangle.Union ((Rectangle)boundingBox, b);
			}
		}

		/// <summary>
		/// DrawTicks method. In base axis class this does nothing.
		/// </summary>
		/// <param name="ctx">The Drawing Context with which to draw</param>
		/// <param name="physicalMin">The physical position corresponding to the world minimum of the axis.</param>
		/// <param name="physicalMax">The physical position corresponding to the world maximum of the axis.</param>
		/// <param name="labelOffset">is set to a suitable offset from the axis to draw the axis label. In this base method, set to null.</param>
		/// <param name="boundingBox">is set to the smallest box that bounds the ticks and the tick text. In this base method, set to null.</param>
		protected virtual void DrawTicks ( 
			Context ctx, 
			Point physicalMin, 
			Point physicalMax, 
			out object labelOffset,
			out object boundingBox )
		{
			labelOffset = null;
			boundingBox = null;
			// do nothing. This class is not abstract because a subclass may
			// want to override the Axis.Draw method to one that doesn't 
			// require DrawTicks.
		}

		/// <summary>
		/// World extent of the axis.
		/// </summary>
		public double WorldLength
		{
			get { return Math.Abs (WorldMax - WorldMin); }
		}

		/// <summary>
		/// Determines the positions, in world coordinates, of the large ticks. 
		/// When the physical extent of the axis is small, some of the positions 
		/// that were generated in this pass may be converted to small tick 
		/// positions and returned as well.
		/// 
		/// This default implementation returns empty large ticks list and null
		/// small tick list.
		/// </summary>
		/// <param name="physicalMin">The physical position corresponding to the world minimum of the axis.</param>
		/// <param name="physicalMax">The physical position corresponding to the world maximum of the axis.</param>
		/// <param name="largeTickPositions">ArrayList containing the positions of the large ticks.</param>
		/// <param name="smallTickPositions">ArrayList containing the positions of the small ticks if calculated, null otherwise.</param>
		internal virtual void WorldTickPositions_FirstPass(
			Point physicalMin, 
			Point physicalMax,
			out ArrayList largeTickPositions,
			out ArrayList smallTickPositions
			)
		{
			largeTickPositions = new ArrayList();
			smallTickPositions = null;
		}

		/// <summary>
		/// Determines the positions, in world coordinates, of the small ticks
		/// if they have not already been generated.
		/// 
		/// This default implementation creates an empty smallTickPositions list 
		/// if it doesn't already exist.
		/// </summary>
		/// <param name="physicalMin">The physical position corresponding to the world minimum of the axis.</param>
		/// <param name="physicalMax">The physical position corresponding to the world maximum of the axis.</param>
		/// <param name="largeTickPositions">The positions of the large ticks.</param>
		/// <param name="smallTickPositions">If null, small tick positions are returned via this parameter. Otherwise this function does nothing.</param>
		internal virtual void WorldTickPositions_SecondPass( 
			Point physicalMin,
			Point physicalMax,
			ArrayList largeTickPositions, 
			ref ArrayList smallTickPositions )
		{
			if (smallTickPositions == null) {
				smallTickPositions = new ArrayList();
			}
		}

		/// <summary>
		/// Determines the positions of all Large and Small ticks.
		/// </summary>
		/// <param name="physicalMin">The physical position corresponding to the world minimum of the axis.</param>
		/// <param name="physicalMax">The physical position corresponding to the world maximum of the axis.</param>
		/// <param name="largeTickPositions">ArrayList containing the positions of the large ticks.</param>
		/// <param name="smallTickPositions">ArrayList containing the positions of the small ticks.</param>
		public void WorldTickPositions (
			Point physicalMin,
			Point physicalMax,
			out ArrayList largeTickPositions,
			out ArrayList smallTickPositions )
		{
			WorldTickPositions_FirstPass (physicalMin, physicalMax, out largeTickPositions, out smallTickPositions);
			WorldTickPositions_SecondPass (physicalMin, physicalMax, largeTickPositions, ref smallTickPositions);
		}

		#region Axis Range Utilities
		//
		// The following utilities are provided to simplify expansion, contraction and translation of the Axis.
		// They are all based on modifying the WorldRange by a given proportion, which is convenient for those
		// user-interactions which move/re-range the plot based on mouse movements in the plot's Physical space.
		// In order to handle non-linear axes correctly, it is not possible to simply adjust the WorldMin and
		// WorldMax values directly, since this may result in invalid endpoints (eg LogAxis).	Instead, the
		// notional physical axis is modified by the appropriate amounts, and the new physicalMin and physicalMax
		// are then re-mapped to World coordinates using the specific Axis transforms. Since an Axis does not
		// depend on the physical size or orientation to which it is drawn, the World limits may for convenience
		// be mapped to the unit vector on the real axis, then transformed to their new values after adjustment.
		//
		
		/// <summary>
		/// Modifies WorldMin and WorldMax by the respective increments, specified as proportions of the existing range
		/// Typically, to restrict the range, deltaWorldMin should be +ve, and deltaWorldMax -ve, while increments of
		/// the opposite sign will extend the range. This is a private helper routine used by the public interface,
		/// which assumes all parameter validation and clipping has already been done.
		/// </summary>
		/// <param name="deltaWorldMin"></param>
		/// <param name="deltaWorldMax"></param>
		private void ModifyRange (double deltaWorldMin, double deltaWorldMax)
		{
			Point origin = new Point (0,0);	// Unit vector origin
			Point vector = new Point (1,0);	// ...and endpoint
			
			Point newMin = origin;	// copy unit vector, since original
			Point newMax = vector;	// will be used in final transform
			
			// Adjust unit vector by WorldMin/Max increments
			newMin.X += deltaWorldMin;
			newMax.X += deltaWorldMax;
			
			// map new physical axis to World coordinates, then update WorldMin/Max
			double newWorldMin = PhysicalToWorld (newMin, origin, vector, false);
			double newWorldMax = PhysicalToWorld (newMax, origin, vector, false);
			WorldMin = newWorldMin;
			WorldMax = newWorldMax;
		}

	
		/// <summary>
		/// Moves the WorldMin and WorldMax values so that the world axis length is
		/// [proportion] bigger, with the value [focusRatio] remaining at the same
		/// relative position along the axis
		/// </summary>
		/// <param name="proportion">Proportion to increase world length by.</param>
		/// <param name="focusRatio">Remains at same relative position on axis.</param>
		/// <remarks>
		/// [focusRatio] should be in the range 0.0 (corresponding to WorldMin) to 1.0
		/// (corresponding to WorldMax).  At present, it is clipped if outside this
		/// range, though this is only a 'sensible' limit. If [proportion] is -ve, the
		/// range will be reduced, although a limit of -0.99 is imposed (ie the World
		/// axis range will not be reduced to less than 0.01 (1%) of its original value.
		/// This arbitrary amount may be made configurable at some stage.
		/// </remarks>
		public void IncreaseRange (double proportion, double focusRatio)
		{
			double lo = -0.99;
			
			// clip proportion and focusRatio
			proportion = Math.Max (proportion,lo);
			focusRatio = Math.Max (focusRatio,0.0);
			focusRatio = Math.Min (focusRatio,1.0);
			
			// calculate WorldMin/Max increments, preserving focusRatio
			double deltaWorldMin = -proportion*focusRatio;
			double deltaWorldMax = +proportion*(1.0-focusRatio);
			
			ModifyRange (deltaWorldMin, deltaWorldMax);
		}

		/// <summary>
		/// Modifies the WorldMin and WorldMax values so that the world axis length is
		/// [proportion] bigger (eg 0.1 increases range by 10%). WorldMax and WorldMin
		/// are changed so that the expansion/contraction of the axis is symmetrical
		/// about its midpoint. If the	current WorldMax and WorldMin are the same,
		/// they are (currently) moved apart by an arbitrary amount (epsilon). This
		/// condition seems arbitrary, and may be removed at some stage.
		/// </summary>
		/// <param name="proportion">Proportion to increase world length by.</param>
		public void IncreaseRange (double proportion)
		{
			double epsilon = 0.01;
			double range = WorldMax - WorldMin;
			
			if (Utils.DoubleEqual(range, 0.0)) {
				// TODO remove this arbitrary condition?
				proportion = epsilon;
			}
			IncreaseRange(proportion, 0.5);
		}
		
		/// <summary>
		/// Redefines the WorldMin and WorldMax values based on [newWorldMin] and
		/// [newWorldMax] expressed as proportions of the existing axes, where both
		/// are in the range (0.0,1.0) with [newWorldMin] less than [newWorldMax]
		/// </summary>
		/// <param name="newWorldMin">as a proportion of current range</param>
		/// <param name="newWorldMax">as a proportion of current range</param>
		/// <remarks>
		/// Values will be swapped (and clipped if clipToWorld is true) and a minimum
		/// new range of 1% of the existing range is arbitrarily enforced (for safety).
		/// If clipToWorld is false, new WorldRange may lie outside the current limits 
		/// </remarks>
		public void DefineRange (double newWorldMin, double newWorldMax, bool clipToWorld)
		{
			double lo = 0.0, hi = 1.0;
			double epsilon = 0.01;
			
			// order the min and max values
			if (newWorldMin > newWorldMax) {
				Utils.Swap (ref newWorldMin, ref newWorldMax);
			}
			// clip to existing range if requested
			if (clipToWorld) {
				newWorldMin = Math.Max(newWorldMin,lo);
				newWorldMin = Math.Min(newWorldMin,hi);
				newWorldMax = Math.Max(newWorldMax,lo);
				newWorldMax = Math.Min(newWorldMax,hi);
			}
			
			// enforce minimum range limit
			if ((newWorldMax - newWorldMin) < epsilon) {
				if (newWorldMax <= (hi - epsilon)) {
					newWorldMax = newWorldMin + epsilon;
				}
				else {
					newWorldMin = newWorldMax - epsilon;
				}
			}
			
			double deltaWorldMin = newWorldMin;
			double deltaWorldMax = newWorldMax - 1.0;
			
			ModifyRange (deltaWorldMin, deltaWorldMax);
			
		}
		
		/// <summary>
		/// Modifies the WorldMin and WorldMax values so that the world axis is
		/// translated by the specified [shiftProportion].	If [shiftProportion] is
		/// positive, WorldMin and WorldMax are both increased, so that the range is
		/// shifted to the right (assuming WorldMin to the left of WorldMax), while
		/// negative values of [shiftProportion] decrease both WorldMin and WorldMax.
		/// </summary>
		/// <param name="shiftProportion">as a proportion of current range</param>
		public void TranslateRange (double shiftProportion)
		{
			double deltaWorldMin = shiftProportion;
			double deltaWorldMax = shiftProportion;
			
			ModifyRange(deltaWorldMin, deltaWorldMax);
			
		}
		#endregion Axis Utilities

		// TODO: finish events implementation some point...

		//public class WorldValueChangedArgs
		//{
		//	public WorldValueChangedArgs( double value, MinMaxType minOrMax )
		//	{
		//		Value = value;
		//		MinOrMax = minOrMax;
		//	}

		//	public double Value;

		//	public enum MinMaxType
		//	{
		//		Min = 0,
		//		Max = 1
		//	}

		//	public MinMaxType MinOrMax;
		//}

		//public delegate void WorldValueChangedHandler (object sender, WorldValueChangedArgs e);

		//public event WorldValueChangedHandler WorldMinChanged;
		//public event WorldValueChangedHandler WorldMaxChanged;
		//public event WorldValueChangedHandler WorldExtentsChanged;

	}
}
