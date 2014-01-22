/*
 * CSPlot - the ControlSuite Plotting Library
 * 
 * Axis.cs
 * 
 * Copyright (c) 2011 Hywel Thomas
 *
 * Released under the MIT/X11 License
 * http://www.opensource.org/licenses/mit-license.html
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;


///Ruler to check 100 columns.	Suggest this as maximum for printing sources.=>	   Mono uses 80
///456789012356789012345678901234567890123456789012345678901234567890123456789012345678901234567890
 
namespace CSPlot{

	/// <summary>
	/// Encapsulates functionality common to all axis classes.	Complex
	/// axis classes derive from this, which is a linear axis with basic
	/// embellishments (Label, ticks, and tick labels). The virtual Draw
	/// methods should be over-ridden to provide additional features,
	/// such as Logarithmic scales, DateTime scales, etc.
	/// This class defines both the World Coordinates (WorldMin, WorldMax) and
	/// Physical Coordinates (PhysicalMin, PhysicalMax) on the drawing surface.
	/// </summary>
	public class Axis : System.ICloneable {

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public Axis(){
			this.Init();
		}


		/// <summary>
		/// Constructor that takes World and Physical Minimum and Maximum values.
		/// </summary>
		/// <param name="worldMin">The minimum world coordinate.</param>
		/// <param name="worldMax">The maximum world coordinate.</param>
		/// <param name="physicalMin">The Physical coordinates corresponding to WorldMin</param>
		/// <param name="physicalMax">The Physical coordinates corresponding to WorldMax</param>
		public Axis(double worldMin, double worldMax, Point physicalMin, Point physicalMax){
			this.Init();
			this.WorldMin = worldMin;
			this.WorldMax = worldMax;
			this.PhysicalMin = physicalMin;
			this.PhysicalMax = physicalMax;
		}



		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="a">The Axis to clone.</param>
		public Axis(Axis a) {
			Axis.DoClone(a, this);
		}



		#endregion

		#region Properties

		#region World and Physical Extents

		/// <summary>
		/// The minimum world extent of the axis. By definition, WorldMin MUST be numerically
		/// less than WorldMax, with WorldMin -> WorldMax defining the axis +ve direction.
		/// The direction of the displayed Axis is defined by PhysicalMin and PhysicalMax,
		/// which can be a vector at any direction in the physical drawing space.
		/// Note: World coordinate origin is BottomLeft, but GDI+ (pixel) origin is TopLeft.
		/// </summary>
		public virtual double WorldMin{
			get{ return worldMin; }
			set{ worldMin = value; }
		}

		private double worldMin = 0.0f;


		/// <summary>
		/// The maximum world extent of the axis
		/// </summary>
		public virtual double WorldMax{
			get{ return worldMax; }
			set{ worldMax = value; }
		}

		private double worldMax = 1.0f;


		/// <summary>
		/// World extent of the axis
		/// </summary>
		public double WorldExtent{
			get { return (worldMax-worldMin); }
		}


		/// <summary>
		/// Determines whether a world value is outside range WorldMin -> WorldMax
		/// </summary>
		/// <param name="worldValue">the world value to test</param>
		/// <returns>true if outside limits, false otherwise</returns>
		public bool OutOfRange(double worldValue){
			if(double.IsNaN(worldMin) || double.IsNaN(worldMax)){
				throw new CSPlotException("Axis WorldMin/WorldMax not set");
			}
			return ((worldValue < worldMin) || (worldValue > worldMax));
		}


		/// <summary>
		/// The Physical (pixel x,y) coordinates corresponding to the WorldMin extent of the axis.
		/// The origin (0,0) in Physical (display) coordinates is at the TopLeft of the Display.
		/// </summary>
		public Point PhysicalMin{
			get{ return physicalMin; }
			set{
				physicalMin = value;
				EvaluateAngle();
			}
		}

		private Point physicalMin = Point.Empty;


		/// <summary>
		/// The Physical (pixel x,y) coordinates corresponding to the WorldMax extent of the axis.
		/// </summary>
		public Point PhysicalMax{
			get{ return physicalMax; }
			set{
				physicalMax = value;
				EvaluateAngle();
			}
		}

		private Point physicalMax = new Point(100,0);


		/// <summary> 
		/// The Physical (pixel) extent	 of the axis.
		/// This may not be an integer if the axis is not orthogonal
		/// </summary>
		public double PhysicalExtent{
			get{
				double dx = physicalMax.X - physicalMin.X;
				double dy = physicalMax.Y - physicalMin.Y;
				return Math.Sqrt(dx*dx + dy*dy);
			}
		}


		/// <summary>
		/// The length in world coordinates of one pixel
		/// </summary>
		public double PixelWorldLength{
			get{ return WorldExtent/PhysicalExtent; }
		} 


		/// <summary>
		/// The Axis angle on the drawing surface in degrees, measured anti-clockwise from horizontal
		/// </summary>
		/// <remarks>This is consistent with the World Coordinate system, but is the -ve
		///		 of that evaluated in the (GDI+) Physical or pixel coordinate system.
		/// </remarks>
		public float PhysicalAngle{
			get{ return physicalAngle; }
		}

		private float physicalAngle = 0.0f;


		/// <summary>
		/// Whether or not this axis is linear.
		/// </summary>
		public virtual bool IsLinear {
			get { return true; }
		}




		#endregion

		#region Embellishments (Tick, TickText, and Axis Label)

		/// <summary>
		/// If set to true, the axis is hidden. That is, the axis line and all embellishments
		/// (ie, axis label, ticks, and tick labels) will not be drawn. The Axis is, however,
		/// still defined for its World and Physical positioning and Transform properties.
		/// </summary>
		public bool Hidden {
			get{ return hidden; }
			set{ hidden = value; }
		}
		
		private bool hidden = false;


		/// <summary>
		/// If true, no tick labels will be drawn next to the ticks
		/// NB: ticks may be hidden by setting the tickLength to zero
		/// </summary>
		public bool TickTextHidden {
			get { return tickTextHidden; }
			set { tickTextHidden = value; }
		}

		private bool tickTextHidden = false;


		/// <summary>
		/// If true, Axis embellishments (label, ticks and tick labels)
		/// will be scaled as the Axis Physical extents are changed
		/// </summary>
		public bool EmbellishmentsScale {
			get { return embellishmentsScale; }
			set {
				embellishmentsScale = value;
				if (!embellishmentsScale) {
					scaleFactor = 1.0f;
				}
			}
		}

		private bool embellishmentsScale = false;


		/// <summary>
		/// If EmbellishmentsScale, the ScaleFactor to be applied, otherwise 1.0. 
		/// </summary>
		/// <remarks>This must be set by the PlotSurface as it re-sizes the Axis</remarks>
		public float ScaleFactor {
			get { return scaleFactor; }
			set { scaleFactor = value; }
		}

		private float scaleFactor = 1.0f;


		/// <summary>
		/// Length (in pixels) of a large tick.	 NOT the distance 
		/// between large ticks, but the length of the tick itself.
		/// </summary>
		public int LargeTickSize{
			get{ return largeTickSize; }
			set{ largeTickSize = value; }
		}
		
		private int largeTickSize = 7; // symmetrical if TicksCrossAxis

		
		/// <summary>
		/// Length (in pixels) of the small ticks.
		/// </summary>
		public int SmallTickSize{
			get{ return smallTickSize; }
			set{ smallTickSize = value;}
		}

		private int smallTickSize = 3; // symmetrical if TicksCrossAxis


		/// <summary>
		/// Angle (degrees) to draw ticks at (anti-clockwise from +ve X-axis direction)
		/// The default of -90 places X-axis ticks below the axis, outside the plot area
		/// </summary>
		public float TickAngle {
			get { return tickAngle; }
			set { tickAngle = value; }
		}

		private float tickAngle = -90.0f;


		/// <summary>
		/// If true, tick marks will cross the axis, with their centre on the axis line.
		/// If false, tick marks will be drawn with their origin starting on the axis line.
		/// </summary>
		public bool TicksCrossAxis {
			get { return ticksCrossAxis; }
			set { ticksCrossAxis = value; }
		}

		bool ticksCrossAxis = false;


		/// <summary>
		/// If true, text (values) associated with Large ticks will be drawn at
		/// the tick origin end, ie on the other side of the axis line.
		/// If false, tick text will be drawn at the far end of the tick mark.
		/// </summary>
		/// <remarks>NB this also controls the Axis Label positioning</remarks>
		public bool TextAtTickOrigin {
			get { return textAtTickOrigin; }
			set { textAtTickOrigin = value; }
		}

		bool textAtTickOrigin = false;


		/// <summary>
		/// Angle (degrees) to draw Large tick labels (anti-clockwise from Axis direction)
		/// For the X-Axis, this would normally be zero, ie parallel to the X-Axis, whereas
		/// for the Y-Axis, +90 degrees would give horizontal tick labels.
		/// </summary>
		public float TickTextAngle {
			get { return tickTextAngle; }
			set { tickTextAngle = value; }
		}

		private float tickTextAngle = 0.0f;


		/// <summary>
		/// The color of the brush used to draw the axis tick labels.
		/// </summary>
		public Color TickTextColor {
			set { tickTextBrush = new SolidBrush(value); }
		}


		/// <summary>
		/// The brush used to draw the tick text.
		/// </summary>
		public Brush TickTextBrush {
			get { return tickTextBrush; }
			set { tickTextBrush = value; }
		}

		private Brush tickTextBrush;


		/// <summary>
		/// This font is used for drawing labels next to the axis tick marks.
		/// </summary>
		public Font TickTextFont {
			get { return tickTextFont; }
			set {
				tickTextFont = value;
				UpdateScaleFactor(); // Also re-evaluate axis bounds?
			}
		}

		private Font tickTextFont;
		private Font tickTextFontScaled;


		/// <summary>
		/// Specifies the format used for drawing tick labels.
		/// See	 StringBuilder.AppendFormat for a description of this string.
		/// </summary>
		public string NumberFormat {
			get { return numberFormat; }
			set { numberFormat = value; }
		}

		private string numberFormat = "{0:g5}";


		/// <summary>
		/// The Axis Label (or Title).	May be hidden by setting Label = ""
		/// This is drawn parallel to the Axis, in the +ve Axis direction.
		/// For a normal X-Axis, this is from left to right, and for a 
		/// normal Y-Axis, from bottom to top.	If an axis is reversed, by
		/// selecting PhysicalMin and PhysicalMax values appropriately,
		/// then the Axis Label will be drawn in the opposite direction.
		/// Use LabelReversed if necessary for readability.
		/// </summary>
		public string Label{
			get{ return label; }
			set{ label = value;}
		}
		
		private string label = "";


		/// <summary>
		/// Axis Label will be rotated by 180 degrees
		/// </summary>
		public bool LabelReversed {
			get { return labelReversed; }
			set { labelReversed = value; }
		}

		private bool labelReversed = false;


		/// <summary>
		/// This font is used to draw the axis Label.
		/// </summary>
		public Font LabelFont{
			get{ return labelFont; }
			set{
				labelFont = value;
				UpdateScaleFactor();
			}
		}
		
		private Font labelFont;
		private Font labelFontScaled;

  
		/// <summary>
		/// Sets distance of Axis Label from axis, dependent on LabelOffsetAbsolute
		/// </summary>
		/// <remarks>
		/// A +ve offset moves the Label towards (or beyond) the Tick endpoint.
		/// A -ve offset moves the Label towards (or beyond) the Tick Origin.
		/// NB: This value is NOT scaled when the embellishments scale.
		/// </remarks>
		public float LabelOffset {
			get { return labelOffset; }
			set { labelOffset = value; }
		}

		private float labelOffset = 0.0f;


		/// <summary>
		/// If true, LabelOffset is the absolute offset of the label from the axis.
		/// If false, LabelOffset is added to the calculated axis label position.
		/// </summary>
		/// <remarks>
		/// By default, Axis Label is positioned to just clear the Ticks and TickText.
		/// An absolute value may be preferred if Axis Labels need special alignment.
		/// </remarks>
		public bool LabelOffsetAbsolute {
			get { return labelOffsetAbsolute; }
			set { labelOffsetAbsolute = value; }
		}

		private bool labelOffsetAbsolute = false;


		/// <summary>
		/// The color of the pen used to draw the ticks and the axis line.
		/// </summary>
		public Color AxisColor{
			get{ return linePen.Color; }
			set{ linePen = new Pen((Color)value); }
		}


	   /// <summary>
		/// The pen used to draw the ticks and the axis line.
		/// </summary>
		public System.Drawing.Pen AxisPen{
			get{ return linePen; }
			set{ linePen = value; }
		}
		
		private System.Drawing.Pen linePen;


		/// <summary>
		/// The color of the brush used to draw the axis label.
		/// </summary>
		public Color LabelColor{
			set{ labelBrush = new SolidBrush(value); }
		}

		
		/// <summary>
		/// The brush used to draw the axis label.
		/// </summary>
		public Brush LabelBrush{
			get{ return labelBrush; }
			set{ labelBrush = value; }
		}
		
		private Brush labelBrush;


		/// <summary>
		/// The bounding rectangle enclosing the Axis and all embellishments
		/// </summary>
		public Rectangle Bounds{
			get{ return bounds; }
		}
		
		private Rectangle bounds = Rectangle.Empty;


		#endregion

		#region Tick Spacing

		/// <summary>
		/// If true (default) Large and Small tick placements will be calculated automatically.
		/// If false, LargeTickStep, LargeTickValue, and NumberSmallTicks must be user-defined.
		/// </summary>
		public bool AutoTickPlacement {
			get { return autoTickPlacement; }
			set { autoTickPlacement = value; }
		}

		private bool autoTickPlacement = true;


		/// <summary>
		/// The world distance between large ticks, either manually set or calculated automatically
		/// </summary>
		public double LargeTickStep {
			get { return largeTickStep; }
			set { largeTickStep = value; }
		}

		private double largeTickStep = 30;


		/// <summary>
		/// If manually set, a large tick will be placed at this initial world position,
		/// and other large ticks will then be placed relative to this position.
		/// </summary>
		public double LargeTickValue {
			get { return largeTickValue; }
			set { largeTickValue = value; }
		}

		private double largeTickValue = 0;


		/// <summary>
		/// The number of small ticks between large ticks.
		/// </summary>
		public int NumberOfSmallTicks {
			get { return numberSmallTicks; }
			set { numberSmallTicks = value; }
		}

		private int numberSmallTicks = 1;


		/// <summary>
		/// If LargeTickStep is not specified, it will be calculated automatically.
		/// However, the Physical (pixel) value will not be less than this minimum.
		/// </summary>
		public int MinPhysicalLargeTickStep {
			get { return minPhysicalLargeTickStep; }
			set { minPhysicalLargeTickStep = value; }
		}

		private int minPhysicalLargeTickStep = 30;


		/// <summary>
		/// If true, automated tick placement will be independent of the physical extent of the axis.
		/// Tick placement will look good for charts of typical size (eg physical dimensions of 640x480),
		/// and will look the same on different size graphics surfaces, eg Windows.Forms and a printer.
		/// If false (default), the number of ticks and placement will be  optimally calculated to look
		/// best for the axis physical extent, eg for particularly small or large pixel dimensions.
		/// </summary>
		public bool TicksIndependentOfPhysicalExtent {
			get { return ticksIndependentOfPhysicalExtent; }
			set { ticksIndependentOfPhysicalExtent = value; }
		}

		private bool ticksIndependentOfPhysicalExtent = false;

		#endregion

		#endregion Properties

		#region Methods

		#region Axis Scaling and Transformations

		/// <summary>
		/// World to Physical transform using Axis current World and Physical Extents
		/// </summary>
		/// <param name="world">The World value to transform.</param>
		/// <param name="clip">if false, physical value may extend outside WorldMin, WorldMax.
		///		If true, physical value will be clipped to PhysicalMin, PhysicalMax</param>
		/// <returns>physical point corresponding to world value</returns>
		public virtual PointF WorldToPhysical( double world, bool clip )
		{

			// if clip is true, return Min or Max if outside range.

			if(clip){
				if(world > worldMax){ return physicalMax; }
				if(world < worldMin){ return physicalMin; }
			}

			// clip true and within range, OR outside range but clip false

			// evaluate worldValue as a proportion of WorldExtent
			double prop = (world - worldMin)/(worldMax - worldMin);

			// For safety, clip at a limit maxClip times that of WorldExtent
			double maxClip = 100.0;

			if(prop >  maxClip){ prop =	 maxClip; }
			if(prop < -maxClip){ prop = -maxClip; }

			// calculate the physical coordinate as a proportion of range
			float dx = (float)( prop * (physicalMax.X - physicalMin.X) );
			float dy = (float)( prop * (physicalMax.Y - physicalMin.Y) );
		   
			return new PointF((physicalMin.X + dx), (physicalMin.Y + dy));

		}


		/// <summary>
		/// Physical to World transform using current World Extents
		/// and the specific physical points corresponding to these
		/// </summary>
		/// <param name="p">The point to project onto the axis</param>
		/// <param name="pMin">physical point corresponding to WorldMin</param>
		/// <param name="pMax">physical point corresponding to WorldMax</param>
		/// <param name="clip">If true, the world value will be clipped to WorldMin
		///		or WorldMax as appropriate if it lies outside this range.</param>
		/// <returns>world value corresponding to the projection of p onto the axis.</returns>
		public virtual double PhysicalToWorld(
			PointF p,
			PointF pMin,
			PointF pMax,
			bool clip )
		{

			// evaluate axis vector and point vector from common origin pMin
			PointF aV = new PointF( (pMax.X-pMin.X), (pMax.Y-pMin.Y) );
			PointF pV = new PointF( (p.X - pMin.X),	 (p.Y - pMin.Y)	 );

			 // project the point vector onto the physical axis
			double projection = pV.X * aV.X + pV.Y * aV.Y;

			// evaluate axis physical extent squared
			double pE2 = aV.X * aV.X + aV.Y * aV.Y;

			// world value is normalised projection (ie proportion) of WorldExtent
			double world = worldMin + (worldMax - worldMin) * projection / pE2;

			// if clipped value required, return World Min/Max if outside range.
			if(clip){
				if(world < worldMin){ return worldMin; }
				if(world > worldMax){ return worldMax; }
			}

			return world;
		}


		#region Axis Range Utilities

		//
		// The following utilities are provided to simplify expansion, contraction, and
		// translation of the Axis.	 They are all based on modifying the WorldRange by a
		// given proportion, which is convenient for those user-interactions which move
		// or re-range the plot based on mouse movements in the plot's Physical space.
		// In order to handle non-linear axes correctly, it is not possible to simply
		// adjust the WorldMin and WorldMax values directly, since this may result in
		// invalid endpoints (eg a LogAxis).  Instead, the notional physical axis is 
		// modified by the required proportion, and the new physicalMin and physicalMax
		// are then re-mapped to World coordinates using the specific Axis transform.
		// Since an Axis does not depend on the physical size or orientation to which it
		// is drawn, the World limits may for convenience be mapped to a nominal unit
		// vector on the real axis, then transformed to their new values after adjustment.
		//

		/// <summary>
		/// Modifies WorldMin and WorldMax by the respective increments, specified as
		/// proportions of the existing range. Typically, to reduce the range, deltaWorldMin
		/// should be +ve, and deltaWorldMax -ve, while increments of the opposite sign will
		/// extend the range. This is a private helper routine used by the public interface,
		/// which assumes all parameter validation and clipping has already been done.
		/// </summary>
		/// <param name="deltaWorldMin"></param>
		/// <param name="deltaWorldMax"></param>
		private void ModifyRange(double deltaWorldMin, double deltaWorldMax) {
			PointF origin = new PointF(0, 0);	// Unit vector origin
			PointF vector = new PointF(1, 0);	// ...and endpoint

			PointF newMin = origin;	// copy unit vector, since original
			PointF newMax = vector;	// will be used in final transform

			// Adjust unit vector by WorldMin/Max increments
			newMin.X += (float)(deltaWorldMin);
			newMax.X += (float)(deltaWorldMax);

			// map new physical axis to World coordinates, then update WorldMin/Max
			double newWorldMin = PhysicalToWorld(newMin, origin, vector, false);
			double newWorldMax = PhysicalToWorld(newMax, origin, vector, false);
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
		public void IncreaseRange(double proportion, double focusRatio) {
			double lo = -0.99;

			// clip proportion and focusRatio
			proportion = Math.Max(proportion, lo);
			focusRatio = Math.Max(focusRatio, 0.0);
			focusRatio = Math.Min(focusRatio, 1.0);

			// calculate WorldMin/Max increments, preserving focusRatio
			double deltaWorldMin = -proportion * focusRatio;
			double deltaWorldMax = +proportion * (1.0 - focusRatio);

			ModifyRange(deltaWorldMin, deltaWorldMax);

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
		public void DefineRange(double newWorldMin, double newWorldMax, bool clipToWorld) {
			double lo = 0.0, hi = 1.0;
			double epsilon = 0.01;

			// order the min and max values
			if (newWorldMin > newWorldMax) {
				Utils.Swap(ref newWorldMin, ref newWorldMax);
			}
			// clip to existing range if requested
			if(clipToWorld){
				newWorldMin = Math.Max(newWorldMin, lo);
				newWorldMin = Math.Min(newWorldMin, hi);
				newWorldMax = Math.Max(newWorldMax, lo);
				newWorldMax = Math.Min(newWorldMax, hi);
			}

			// enforce minimum range limit
			if((newWorldMax - newWorldMin) < epsilon){
				if(newWorldMax <= (hi - epsilon)){
					newWorldMax = newWorldMin + epsilon;
				}
				else{
					newWorldMin = newWorldMax - epsilon;
				}
			}

			double deltaWorldMin = newWorldMin;
			double deltaWorldMax = newWorldMax - 1.0;

			ModifyRange(deltaWorldMin, deltaWorldMax);

		}

		/// <summary>
		/// Modifies the WorldMin and WorldMax values so that the world axis is
		/// translated by the specified [shiftProportion].	If [shiftProportion] is
		/// positive, WorldMin and WorldMax are both increased, so that the range is
		/// shifted to the right (assuming WorldMin to the left of WorldMax), while
		/// negative values of [shiftProportion] decrease both WorldMin and WorldMax.
		/// </summary>
		/// <param name="shiftProportion">as a proportion of current range</param>
		public void TranslateRange(double shiftProportion) {
			double deltaWorldMin = shiftProportion;
			double deltaWorldMax = shiftProportion;

			ModifyRange(deltaWorldMin, deltaWorldMax);

		}


		#endregion Axis Range Utilities

		#endregion


		#region Drawing/Rendering

		/// <summary>
		/// PreDraw the Axis in order to evaluate the bounds including
		/// embellishments (ie Axis line, Label, Ticks and TickText)
		/// </summary>
		internal void PreDraw(out Rectangle bounds)
		{
			// define a dummy Bitmap and use its Graphics to draw axis
			Bitmap bmp = new Bitmap(1, 1);
			using (Graphics g = Graphics.FromImage(bmp)) {

				this.Draw(g, out bounds);
			}
			bmp.Dispose();
		}


		/// <summary>
		/// Draw the axis. This involves three steps:
		///	 (1) Draw the axis line.
		///	 (2) Draw the tick marks and tick labels.
		///	 (3) Draw the Axis Label.
		/// </summary>
		/// <param name="g">The drawing surface on which to draw.</param>
		/// <param name="bounds">out The bounding rectangle of the axis including all
		///		embellishments (ie axis line, axis label, ticks and tick labels)
		/// </param>
		public virtual void Draw(Graphics g, out Rectangle bounds)
		{
			// normalise the bounds of the axis line
			bounds = Rectangle.Round( Utils.NormalisedRectangleF(physicalMin, physicalMax) );

			float labelOffset;
			Rectangle tickBounds = Rectangle.Empty;
			Rectangle labelBounds = Rectangle.Empty;

			if(!Hidden){

				// (1) Draw the axis line.
				g.ResetTransform();
				g.DrawLine(linePen, physicalMin, physicalMax);

				// (2) draw tick marks, obtaining overall tick bounds and label offset
				DrawTicks(g, out labelOffset, out tickBounds);

				// (3) draw the axis label, 
				if(!TickTextHidden){
					DrawLabel(g, labelOffset, out labelBounds);
				}

				// (4) merge line, ticks and label bounds
				bounds = Rectangle.Union(bounds, tickBounds);
				bounds = Rectangle.Union(bounds, labelBounds);
			}
		}



		/// <summary>
		/// Draws the large and small ticks and tick labels for this axis
		/// </summary>
		/// <param name="g">The graphics surface on which to draw.</param>
		/// <param name="labelOffset">out: offset from the axis to draw the Axis Label.</param>
		/// <param name="bounds">out: Rectangle enclosing all ticks and embellishments</param>
		protected virtual void DrawTicks(Graphics g, out float labelOffset, out Rectangle bounds)
		{
			float tLabelOffset;
			Rectangle tBounds;

			WorldTickPlacement();

			labelOffset = 0;
			bounds = Rectangle.Empty;	// Needs changing.	Don't want to start with (0,0,0,0) !
 
			if(largeTickPositions.Count > 0){
				for(int i = 0; i < largeTickPositions.Count; ++i){

					double tickValue = (double)largeTickPositions[i];

					// TODO: Find out why zero is sometimes not zero [seen as high as 10^-16].
					if (Math.Abs(tickValue) < 0.000000000000001){
						tickValue = 0.0;
					}

					StringBuilder text = new StringBuilder();
					text.AppendFormat(numberFormat, tickValue);

					DrawTick(g, tickValue, largeTickSize, text.ToString(),
						out tLabelOffset, out tBounds);

					UpdateOffsetAndBounds(ref labelOffset, ref bounds,
						tLabelOffset, tBounds);
				}
			}

			for(int i = 0; i < smallTickPositions.Count; ++i){
				DrawTick(g, (double)smallTickPositions[i], smallTickSize, "",
					out tLabelOffset, out tBounds);

				// bounds and label offset unchanged by small ticks

			}
		}


		/// <summary>
		/// Draw a single tick and its associated value string (TickText)
		/// </summary>
		/// <param name="g">The graphics surface on which to draw.</param>
		/// <param name="world">The tick origin in world coordinates.</param>
		/// <param name="size">The physical size of the tick (pixels)</param>
		/// <param name="text">The text (value) associated with the tick</param>
		/// <param name="clearance">out: overall perpendicular clearance from Axis</param>
		/// <param name="bounds">out: tick and tickText bounds</param>
		public virtual void DrawTick(
			Graphics g,
			double world,
			float size,
			string text,
			out float clearance,
			out Rectangle bounds)
		{
			// initialise clearance and bounds
			clearance = 0;
			bounds = Rectangle.Empty;

			if (size <= 0) { return; }		// just to be safe

			// determine physical origin of tick on axis
			PointF physical = WorldToPhysical(world, true);

			// evaluate tick start and end points
			float tickSize = size * ScaleFactor;
			float tickStart = 0;
			float tickEnd = tickSize;

			// translate points if TicksCrossAxis
			if(ticksCrossAxis){
				tickStart -= tickSize/2;
				tickEnd -= tickSize/2;
			}

			// measure tickText string, which is drawn by default with text topLeft at the origin
			SizeF sz = g.MeasureString(text, tickTextFontScaled);

			// calculate the projection of tickText rectangle onto tick vector line
			float delta = tickTextAngle - tickAngle;	// angle between tick and text (degrees)
			double r = delta * Math.PI / 180;			// convert to radians for Sin() and Cos()
			double projection = 0;
			if(!TickTextHidden){
				projection =  Math.Abs(sz.Width*Math.Cos(r)) + Math.Abs(sz.Height*Math.Sin(r));
			}

			// define centre of tickText, and overall clearance, for various tick/text scenarios
			float centre = 0;
			if(textAtTickOrigin){
				centre = tickStart - (float)projection/2;
				clearance = centre - (float)projection/2;
			}
			else{
				centre = tickEnd + (float)projection/2;
				clearance = centre + (float)projection/2;
			}

			// define tickOrigin and tickVector points along the reference X-axis
			PointF tickOrigin = new PointF(tickStart, 0);
			PointF tickVector = new PointF(tickEnd, 0);

			// set up rotation by (tickAngle+axisAngle) and translation to final position.
			// this partial transform is common to (and retained for) drawing the tickText
			g.ResetTransform();
			g.TranslateTransform(physical.X, physical.Y);	// done last (MatrixOrder.Prepend)
			g.RotateTransform(-tickAngle-physicalAngle);	// done first

			// draw tick line
			g.DrawLine(linePen, tickOrigin, tickVector);

			// Set up and transform tick extents to define initial bounds
			PointF[] tickX = new PointF[2];

			tickX[0] = tickOrigin;
			tickX[1] = tickVector;
			g.TransformPoints(CoordinateSpace.Device, CoordinateSpace.World, tickX);

			// adjust clearance because of tick rotation 
			clearance = clearance * (float)Math.Abs( Math.Sin(tickAngle * Math.PI/180) );

			// set initial bounds to rectangle enclosing tick
			bounds = Rectangle.Round( Utils.NormalisedRectangleF( tickX[0], tickX[1]) );

			if(text == "" || TickTextHidden){ return; }		// return now if no tickText to draw

			// add further graphic transforms (in reverse order) to align and draw tickText
			g.TranslateTransform(centre, 0);					// (3) translate to text centre
			g.RotateTransform(-delta);							// (2) align with tick vector
			g.TranslateTransform(-sz.Width/2, -sz.Height/2);	// (1) centre text prior to rotation
 
			// draw label at the origin (0,0) using accumulated transforms
			g.DrawString(text, tickTextFontScaled, tickTextBrush, PointF.Empty);

			// Set up tickText rectangle extents to define overall bounds
			PointF[] textX = new PointF[4];

			textX[0] = new PointF(0, 0);					// origin of rectangle
			textX[1] = new PointF(sz.Width, sz.Height);		// bottom Right corner
			textX[2] = new PointF(sz.Width, 0);				// top Right corner
			textX[3] = new PointF(0, sz.Height);			// bottom Left corner
 
			// transform to final text position and orientation
			g.TransformPoints(CoordinateSpace.Device, CoordinateSpace.World, textX);

			// evaluate the normalised rectangle from each opposing diagonal
			RectangleF r1 = Utils.NormalisedRectangleF( textX[0], textX[1] );
			RectangleF r2 = Utils.NormalisedRectangleF( textX[2], textX[3] );

			// combine rectangles for overall text bounds
			Rectangle textBounds = Rectangle.Round( RectangleF.Union(r1,r2) );
 
			// combine current tickBounds and TextBounds
			bounds = Rectangle.Union( bounds, textBounds );

			g.ResetTransform();			// clear current graphics transform matrix

		}


		/// <summary>
		/// Draw the Axis Label
		/// </summary>
		/// <param name="g">The GDI+ drawing surface on which to draw</param>
		/// <param name="clearance">perpendicular clearance of label from axis</param>
		/// <param name="bounds">out: Rectangle indicating label bounds</param>
		public void DrawLabel(Graphics g, float clearance, out Rectangle bounds)
		{
			bounds = Rectangle.Empty;

			if(Label == ""){ return; }		// no label to draw

			// determine label clearance
			float offset = 0;
			if(labelOffsetAbsolute){
				offset = labelOffset;
			}
			else{
				offset += labelOffset;
			}
			clearance += offset;
 
			SizeF sz = g.MeasureString(Label, labelFontScaled);

			// if textAtTickOrigin, clearance needs extending by sz.Height
			if(textAtTickOrigin){
				clearance += sz.Height * Math.Sign(clearance);
			}

			// define centre point of physical axis at which to draw label
			float xCentre = (physicalMin.X + physicalMax.X) / 2.0f;
			float yCentre = (physicalMin.Y + physicalMax.Y) / 2.0f;
 
			// MatrixOrder.Prepend is default, so reversed order used for transforms
			g.ResetTransform();
			g.TranslateTransform(xCentre, yCentre);		// (4) move to final position
			g.RotateTransform(-physicalAngle);			// (3) rotate by AxisAngle
			g.TranslateTransform(0, clearance);			// (2) offset label vertically
			g.TranslateTransform(-sz.Width / 2, 0);		// (1) centre label horizontally

			g.DrawString(Label, labelFontScaled, labelBrush, PointF.Empty);

			// now work out physical bounds of label. 
			Matrix m = g.Transform;
			PointF[] labelX = new PointF[2];
			labelX[0] = new PointF(0, 0);
			labelX[1] = new PointF(sz.Width, sz.Height);
			m.TransformPoints(labelX);
 
			bounds = Rectangle.Round( Utils.NormalisedRectangleF( labelX[0], labelX[1] ));

			g.ResetTransform();

 
		}


		/// <summary>
		/// Determines the positions of all Large and Small ticks.
		/// </summary>
		/// <param name="largeTickPositions">ArrayList containing the positions of the large ticks.</param>
		/// <param name="smallTickPositions">ArrayList containing the positions of the small ticks.</param>
		public void WorldTickPlacement()
		{
			LargeTickPlacement();
			SmallTickPlacement();
		}


		private ArrayList largeTickPositions = null;
		private ArrayList smallTickPositions = null;
 

		/// <summary>
		/// Determines the positions, in world coordinates, of the large ticks. 
		/// When the physical extent of the axis is small, some of the positions 
		/// that were generated in this pass may be converted to small tick 
		/// positions and returned as well.
		///
		/// If the LargeTickStep isn't set then this is calculated automatically and
		/// depends on the physical extent of the axis. 
		/// </summary>
		protected virtual void LargeTickPlacement() 
		{

			// determine distance between large ticks.
			bool shouldCullMiddle;
			double tickDist = DetermineLargeTickStep(out shouldCullMiddle);

			// determine starting position.
			double first = 0.0f;

			if(!AutoTickPlacement){
				// this works for both case when largTickValue_ lt or gt WorldMin.
				first = largeTickValue + (Math.Ceiling((worldMin - largeTickValue) / tickDist)) * tickDist;
			}
			else{
				if(worldMin > 0.0){
					double nToFirst = Math.Floor(worldMin / tickDist) + 1.0f;
					first = nToFirst * tickDist;
				}
				else{
					double nToFirst = Math.Floor(-worldMin / tickDist) - 1.0f;
					first = -nToFirst * tickDist;
				}

				// could miss one, if first is just below zero.
				if((first - tickDist) >= worldMin){
					first -= tickDist;
				}
			}

			// now make list of large tick positions.
			largeTickPositions = new ArrayList();

			double position = first;
			int safetyCount = 0;
			while(
				(position <= worldMax) &&
				(++safetyCount < 5000)) {
				largeTickPositions.Add(position);
				position += tickDist;
			}

			// if the physical extent is too small, and the middle 
			// ticks should be turned into small ticks, do this now.
			smallTickPositions = null;
			if(shouldCullMiddle){
				smallTickPositions = new ArrayList();

				if(largeTickPositions.Count > 2){
					for(int i = 1; i < largeTickPositions.Count - 1; ++i){
						smallTickPositions.Add(largeTickPositions[i]);
					}
				}

				ArrayList culledPositions = new ArrayList();
				culledPositions.Add(largeTickPositions[0]);
				culledPositions.Add(largeTickPositions[largeTickPositions.Count - 1]);
				largeTickPositions = culledPositions;
			}
		}


		/// <summary>
		/// Determines the positions, in world coordinates, of the small ticks
		/// if they have not already been generated in LargeTickPlacement.
		/// 
		/// </summary>
		protected virtual void SmallTickPlacement()
		{

			// return if already generated.
			if (smallTickPositions != null)
				return;

			int physicalAxisLength = Utils.Distance(physicalMin, physicalMax);

			smallTickPositions = new ArrayList();

			// TODO: Can optimize this now.
			bool shouldCullMiddle;
			double bigTickSpacing = DetermineLargeTickStep(out shouldCullMiddle);

			int nSmall = this.DetermineNumberSmallTicks(bigTickSpacing);
			double smallTickSpacing = bigTickSpacing / (double)nSmall;

			// if there is at least one big tick
			if (largeTickPositions.Count > 0) {
				double pos1 = (double)largeTickPositions[0] - smallTickSpacing;
				while (pos1 > WorldMin) {
					smallTickPositions.Add(pos1);
					pos1 -= smallTickSpacing;
				}
			}

			for (int i = 0; i < largeTickPositions.Count; ++i) {
				for (int j = 1; j < nSmall; ++j) {
					double pos = (double)largeTickPositions[i] + ((double)j) * smallTickSpacing;
					if (pos <= WorldMax) {
						smallTickPositions.Add(pos);
					}
				}
			}
		}

 
		/// <summary>
		/// Calculates the world spacing between large ticks, based on the physical
		/// axis length (parameter), world axis length, Mantissa values and 
		/// MinPhysicalLargeTickStep. A value is calculated such that at least two
		/// large tick marks will be displayed.
		/// </summary>
		/// <param name="shouldCullMiddle">Returns true if we were forced to make spacing of 
		/// large ticks too small in order to ensure that there are at least two of them.
		/// The draw ticks method should not draw more than two large ticks if this returns true.
		/// </param>
		/// <returns>Large tick spacing</returns>
		private double DetermineLargeTickStep(out bool shouldCullMiddle){
			shouldCullMiddle = false;

			// if the large tick has been explicitly set, then return this.
			if(!AutoTickPlacement){ return largeTickStep; }

			// otherwise auto-calculate the large tick step

			// if axis has zero world length, then return arbitrary number.
			if (Utils.DoubleEqual(WorldMax, WorldMin)) {
				return 1.0f;
			}

			double approxTickStep;
			if (TicksIndependentOfPhysicalExtent) {
				approxTickStep = WorldExtent / 6.0f;
			}
			else {
				approxTickStep = MinPhysicalLargeTickStep * PixelWorldLength;
			}

			double exponent = Math.Floor(Math.Log10(approxTickStep));
			double mantissa = Math.Pow(10.0, Math.Log10(approxTickStep) - exponent);

			// determine next whole mantissa below the approx one.
			int mantissaIndex = Mantissas.Length - 1;
			for (int i = 1; i < Mantissas.Length; ++i) {
				if (mantissa < Mantissas[i]) {
					mantissaIndex = i - 1;
					break;
				}
			}

			// then choose next largest spacing. 
			mantissaIndex += 1;
			if(mantissaIndex == Mantissas.Length){
				mantissaIndex = 0;
				exponent += 1.0;
			}

			if(!TicksIndependentOfPhysicalExtent){
				// make sure that the returned value is such that at least two 
				// large tick marks will be displayed.
				double tickStep = Math.Pow(10.0, exponent) * Mantissas[mantissaIndex];
				float physicalStep = (float)(tickStep / PixelWorldLength);

				while(physicalStep > PhysicalExtent / 2){
					shouldCullMiddle = true;

					mantissaIndex -= 1;
					if (mantissaIndex == -1) {
						mantissaIndex = Mantissas.Length - 1;
						exponent -= 1.0;
					}

					tickStep = Math.Pow(10.0, exponent) * Mantissas[mantissaIndex];
					physicalStep = (float)(tickStep / PixelWorldLength);
				}
			}

			return Math.Pow(10.0, exponent) * Mantissas[mantissaIndex];
		}


		/// <summary>
		/// Given the large tick step, determine the number of small ticks that should
		/// be placed in between.
		/// </summary>
		/// <param name="bigTickDist">the large tick step.</param>
		/// <returns>the number of small ticks to place between large ticks.</returns>
		private int DetermineNumberSmallTicks(double bigTickDist) {
			if(numberSmallTicks != 0){
				return numberSmallTicks + 1;
			}

			if(this.SmallTickCounts.Length != this.Mantissas.Length) {
				throw new CSPlotException("Mantissa.Length != SmallTickCounts.Length");
			}

			if (bigTickDist > 0.0f) {

				double exponent = Math.Floor(Math.Log10(bigTickDist));
				double mantissa = Math.Pow(10.0, Math.Log10(bigTickDist) - exponent);

				for (int i = 0; i < Mantissas.Length; ++i) {
					if (Math.Abs(mantissa - Mantissas[i]) < 0.001) {
						return SmallTickCounts[i] + 1;
					}
				}
			}

			return 0;
		}


		/// <summary>
		/// If LargeTickStep isn't specified, then a suitable value is 
		/// calculated automatically. To determine the tick spacing, the
		/// world axis length is divided by ApproximateNumberLargeTicks
		/// and the next lowest distance m*10^e for some m in the Mantissas
		/// set and some integer e is used as the large tick spacing. 
		/// </summary>
		public float ApproxNumberLargeTicks = 3.0f;


	   /// <summary>
		/// If AutoTickPlacement, the LargeTickStep is calculated automatically.
		/// The value will be of the form m*10^e for some m in the following set.
		/// The corresponding set of SmallTickCounts for each mantissa are below.
		/// </summary>

		public double[] Mantissas = { 1.0, 2.0, 5.0 };
 
		public int[] SmallTickCounts = { 4, 1, 4 };


		#endregion

		
 
		#endregion

		#region Implementation

		/// <summary>
		/// Deep copy of Axis.
		/// </summary>
		/// <remarks>
		/// This method includes a check that guards against derived classes forgetting
		/// to implement their own Clone method. If Clone is called on a object derived
		/// from Axis, and the Clone method hasn't been overridden by that object, then
		/// the test this.GetType == typeof(Axis) will fail.
		/// </remarks>
		/// <returns>A copy of the Axis Class</returns>
		public virtual object Clone() {
			// Check that this isn't being called on a derived type. If it is, then
			// the derived type didn't override this method as it should have done.
			if (this.GetType() != typeof(Axis)) {
				throw new CSPlotException("Clone method not overridden in Axis derived type");
			}

			Axis a = new Axis();
			DoClone(this, a);
			return a;
		}


		/// <summary>
		/// Helper method for Clone. Does all the copying - can be called by derived
		/// types so they don't need to implement this part of the copying themselves.
		/// also useful in constructor of derived types that extend Axis class.
		/// </summary>
		protected static void DoClone(Axis src, Axis dest) {
			// value items
			dest.embellishmentsScale = src.embellishmentsScale;
			dest.worldMax = src.worldMax;
			dest.worldMin = src.worldMin;
			dest.textAtTickOrigin = src.textAtTickOrigin;
			dest.hidden = src.hidden;
			dest.tickTextHidden = src.tickTextHidden;
			dest.tickAngle = src.tickAngle;
			dest.tickTextAngle = src.tickTextAngle;
			dest.minPhysicalLargeTickStep = src.minPhysicalLargeTickStep;
			dest.ticksIndependentOfPhysicalExtent = src.ticksIndependentOfPhysicalExtent;
			dest.largeTickSize = src.largeTickSize;
			dest.smallTickSize = src.smallTickSize;
			dest.ticksCrossAxis = src.ticksCrossAxis;
			dest.labelOffset = src.labelOffset;
			dest.labelOffsetAbsolute = src.labelOffsetAbsolute;

			dest.numberSmallTicks = src.numberSmallTicks;
			dest.largeTickValue = src.largeTickValue;
			dest.largeTickStep = src.largeTickStep;



			// reference items.
			dest.tickTextFont = (Font)src.tickTextFont.Clone();
			dest.label = (string)src.label.Clone();
			if (src.numberFormat != null) {
				dest.numberFormat = (string)src.numberFormat.Clone();
			}
			else {
				dest.numberFormat = null;
			}

			dest.labelFont = (Font)src.labelFont.Clone();
			dest.linePen = (Pen)src.linePen.Clone();
			dest.tickTextBrush = (Brush)src.tickTextBrush.Clone();
			dest.labelBrush = (Brush)src.labelBrush.Clone();

		}


		/// <summary>
		/// Helper function for constructors.
		/// Do initialization here so that Clear() method is handled properly
		/// </summary>
		private void Init() {
			this.worldMax = double.NaN;
			this.worldMin = double.NaN;
			this.Hidden = false;
			this.SmallTickSize = 2;
			this.LargeTickSize = 6;
			this.EmbellishmentsScale = false;
			this.TextAtTickOrigin = false;
			this.TickTextHidden = false;
			this.TicksCrossAxis = false;
			this.LabelOffset = 0.0f;
			this.LabelOffsetAbsolute = false;

			this.Label = "";
			this.NumberFormat = "{0:g5}";

			FontFamily fontFamily = new FontFamily("Arial");
			this.TickTextFont = new Font(fontFamily, 10, FontStyle.Regular, GraphicsUnit.Pixel);
			this.LabelFont = new Font(fontFamily, 12, FontStyle.Regular, GraphicsUnit.Pixel);
			this.LabelColor = System.Drawing.Color.Black;
			this.TickTextColor = System.Drawing.Color.Black;
			this.linePen = new Pen(System.Drawing.Color.Black);
			this.linePen.Width = 1.0f;

		}


		/// <summary>
		/// Sets the world extent of the current axis to be just large enough
		/// to encompas the current world extent of the axis, and the world
		/// extent of the passed in axis
		/// </summary>
		/// <param name="a">The other Axis instance.</param>
		public void LUB(Axis a) {
			if (a == null) {
				return;
			}

			// Minima
			if (!double.IsNaN(a.WorldMin)) {
				if (double.IsNaN(WorldMin)) {
					WorldMin = a.WorldMin;
				}
				else {
					if (a.WorldMin < WorldMin) {
						WorldMin = a.WorldMin;
					}
				}
			}

			// Maxima
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



		///<summary>
		/// Evaluate physical angle of axis in degrees *anti-clockwise* from horizontal
		/// NB this is the -ve of the angle as defined in GDI+ (pixel/display) coordinates
		///</summary>
		private void EvaluateAngle()
		{
			double dx = (physicalMax.X - physicalMin.X);
			double dy = (physicalMax.Y - physicalMin.Y);
			double theta = Math.Atan2(-dy,dx); // reverse dy for angle convention
			physicalAngle = (float)(theta * 180.0f / Math.PI);	// convert angle to degrees
		}

		
		/// <summary>
		/// Evaluate Axis bounds, including all embellishments 
		/// </summary>
		private void EvaluateBounds()
		{
			
		}


		/// <summary>
		/// Update the bounds and label offset associated with an axis
		/// to include mergeBounds and mergeLabelOffset respectively.
		/// </summary>
		/// <param name="labelOffset">Current axis label offset.</param>
		/// <param name="bounds">Current axis bounds.</param>
		/// <param name="mergeLabelOffset">the label offset to merge. The current offset will be replaced by this if its norm is larger.</param>
		/// <param name="mergeBounds">the bounds to merge. The current bounds will be replaced by the least upper bound of both bounds.</param>
		protected static void UpdateOffsetAndBounds(
			ref float labelOffset, ref Rectangle bounds,
			float mergeLabelOffset, Rectangle mergeBounds) {
			// determining largest label offset and use it.
			if (Math.Abs(labelOffset) < Math.Abs(mergeLabelOffset) ) {
				labelOffset = mergeLabelOffset;
			}

			// merge bounds.
			Rectangle b = mergeBounds;
			bounds = Rectangle.Union(bounds, b);
		}



		private void UpdateScaleFactor()
		{
			if(labelFont != null){
				labelFontScaled = Utils.ScaleFont(labelFont, ScaleFactor);
			}

			if(tickTextFont != null){
				tickTextFontScaled = Utils.ScaleFont(tickTextFont, ScaleFactor);
			}

		}


		/// <summary>
		/// returns a suitable default offset for the axis label
		/// for the case when there is no tick text to be drawn
		/// </summary>
		/// <returns>axis label offset</returns>
		protected float DefaultLabelOffset() {
			System.Drawing.Rectangle tickBounds;
			float labelOffset;

			DrawTick(null, worldMin, largeTickSize,"",
				out labelOffset,
				out tickBounds);

			return labelOffset;
		}
	}
}


		#endregion
		
 
 