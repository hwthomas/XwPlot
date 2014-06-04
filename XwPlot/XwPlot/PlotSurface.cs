//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// PlotSurface.cs
// 
// Derived originally from NPlot (Copyright (C) 2003-2006 Matt Howlett and others)
// Port to Xwt 2013 : Hywel Thomas <hywel.w.thomas@gmail.com>
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

// #define DEBUG_BOUNDING_BOXES

using System;
using System.Diagnostics;
using System.Collections;

using Xwt;
using Xwt.Drawing;

namespace XwPlot
{
	/// <summary>
	/// Combines all the components required for a plot, bringing together
	/// the X and Y Axes and any IDrawables (eg Legend, Plots, etc).
	/// </summary>
	/// <remarks>
	/// The PlotSurface may be drawn to any surface that can supply a Drawing Context,
	/// and the size of the physical surface is only supplied at the time of drawing
	/// </remarks>
	public class PlotSurface : IPlotSurface
	{
		//private double padding;
		private PhysicalAxis pXAxis1Cache;
		private PhysicalAxis pYAxis1Cache;
		private PhysicalAxis pXAxis2Cache;
		private PhysicalAxis pYAxis2Cache;
		private bool autoScaleAutoGeneratedAxes = false;
		private bool autoScaletitle = false;

		private object plotAreaBoundingBoxCache;
		private object bbXAxis1Cache;
		private object bbXAxis2Cache;
		private object bbYAxis1Cache;
		private object bbYAxis2Cache;
		private object bbTitleCache;

		private Color plotBackColor = Colors.Gray;
		private ColorGradient plotBackGradient = null;
		private BitmapImage plotBackImage = null;

		private ArrayList drawables;
		private ArrayList xAxisPositions;
		private ArrayList yAxisPositions;
		private ArrayList axesConstraints = null;

		private ArrayList zPositions;
		private SortedList ordering;
		private int legendZOrder = -1;
		private int uniqueCounter = 0;

		private Legend legend;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public PlotSurface ()
		{
			Init();
		}

		private void Init()
		{
			drawables = new ArrayList ();
			xAxisPositions = new ArrayList ();
			yAxisPositions = new ArrayList ();
			zPositions = new ArrayList ();
			ordering = new SortedList ();

			try {
				TitleFont = Font.FromName ("Tahoma 14");
			}
			catch (System.ArgumentException) {
				throw new XwPlotException("Error: Tahoma font is not installed on this system");
			}

			PlotBackColor = Colors.White;
			TitleColor = Colors.Black;
			Padding = 10;
			Title = "";
			Legend = null;

			autoScaletitle = false;
			autoScaleAutoGeneratedAxes = false;
			XAxis1 = null;
			XAxis2 = null;
			YAxis1 = null;
			YAxis2 = null;
			pXAxis1Cache = null;
			pYAxis1Cache = null;
			pXAxis2Cache = null;
			pYAxis2Cache = null;

			axesConstraints = new ArrayList ();

		}

		/// <summary>
		/// The physical bounding box of the last drawn plot surface area is available here.
		/// </summary>
		public Rectangle PlotAreaBoundingBoxCache
		{
			get {
				if (plotAreaBoundingBoxCache == null) {
					return Rectangle.Zero;
				}
				else {
					return (Rectangle)plotAreaBoundingBoxCache;
				}
			}
		}

		/// <summary>
		/// The bottom abscissa axis.
		/// </summary>
		public Axis XAxis1 { get; set; }

		/// <summary>
		/// The left ordinate axis.
		/// </summary>
		public Axis YAxis1 { get; set; }

		/// <summary>
		/// The top abscissa axis.
		/// </summary>
		public Axis XAxis2 { get; set; }

		/// <summary>
		/// The right ordinate axis.
		/// </summary>
		public Axis YAxis2 { get; set; }

		/// <summary>
		/// The physical XAxis1 that was last drawn.
		/// </summary>
		public PhysicalAxis PhysicalXAxis1Cache
		{
			get { return pXAxis1Cache; }
		}

		/// <summary>
		/// The physical YAxis1 that was last drawn.
		/// </summary>
		public PhysicalAxis PhysicalYAxis1Cache
		{
			get { return pYAxis1Cache; }
		}

		/// <summary>
		/// The physical XAxis2 that was last drawn.
		/// </summary>
		public PhysicalAxis PhysicalXAxis2Cache
		{
			get { return pXAxis2Cache; }
		}

		/// <summary>
		/// The physical YAxis2 that was last drawn.
		/// </summary>
		public PhysicalAxis PhysicalYAxis2Cache
		{
			get { return pYAxis2Cache; }
		}

		/// <summary>
		/// The chart title.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Sets the color of the title to be drawn
		/// </summary>
		public Color TitleColor { get; set; }

		/// <summary>
		/// The plot title font
		/// </summary>
		public Font TitleFont { get; set; }

		/// <summary>
		/// Whether or not the title will be scaled according to size of the plot surface
		/// </summary>
		public bool AutoScaleTitle
		{
			get { return autoScaletitle; }
			set { autoScaletitle = value; }
		}

		/// <summary>
		/// When plots are added to the plot surface, the Axes they are attached to are
		/// immediately modified to reflect data of the plot. If AutoScaleAutoGeneratedAxes
		/// is true when a plot is added, the axes will be turned into auto scaling ones if they
		/// are not already [tick marks, tick text and label scaled to size of plot surface].
		/// If AutoScaleAutoGeneratedAxes is false, axes will not be autoscaling.
		/// </summary>
		public bool AutoScaleAutoGeneratedAxes
		{
			get { return autoScaleAutoGeneratedAxes; }
			set { autoScaleAutoGeneratedAxes = value; }
		}

		/// <summary>
		/// The distance in pixels to leave between of the edge of the bounding rectangle
		/// supplied to the Draw method, and the markings that make up the plot.
		/// </summary>
		public double Padding { get; set; }

		/// <summary>
		/// A (simple) Color used to paint the plot background.
		/// Mutually exclusive with PlotBackGradient and PlotBackImage.
		/// This is the default if both BackImage and BackGradient are null.
		/// </summary>
		public Color PlotBackColor
		{
			get { return plotBackColor; }
			set {
				plotBackColor = value;
				plotBackGradient = null;
				plotBackImage = null;
			}
		}

		/// <summary>
		/// A (linear) Color Gradient used to paint the plot background.
		/// Mutually exclusive with PlotBackColor and PlotBackImage
		/// </summary>
		/// <remarks>
		/// Any Color Gradient should be defined on a Rectangle (0, 0, 1, 1)
		/// with any ColorStops added within this range.  When the Background
		/// is drawn, this rectangle will be scaled to the current PlotArea
		/// </remarks>
		public ColorGradient PlotBackGradient
		{
			get { return plotBackGradient; }
			set {
				plotBackGradient = value;
				plotBackColor = Colors.White;
				plotBackImage = null;
			}
		}

		/// <summary>
		/// An Image used to paint the plot background.
		/// Mutually exclusive with PlotBackColor and PlotBackGradient
		/// </summary>
		public BitmapImage PlotBackImage
		{
			get { return plotBackImage; }
			set {
				plotBackImage = value;
				plotBackColor = Colors.White;
				plotBackGradient = null;
			}
		}

		/// <summary>
		/// Gets an array list containing all drawables currently added to the PlotSurface.
		/// </summary>
		public ArrayList Drawables
		{
			get { return drawables; }
		}

		/// <summary>
		/// Legend to use. If this property is null [default], then the plot
		/// surface will have no corresponding legend.
		/// </summary>
		public Legend Legend
		{
			get { return legend; }
			set { legend = value; }
		}

		/// <summary>
		/// Setting this value determines the order (relative to IDrawables added to the plot surface)
		/// that the legend is drawn.
		/// </summary>
		public int LegendZOrder
		{
			get { return legendZOrder; }
			set { legendZOrder = value; }
		}

		/// <summary>
		/// Clears the plot and resets all state to the default.
		/// </summary>
		public void Clear()
		{
			Init ();
		}

		/// <summary>
		/// Performs a hit test with the given point and returns information about the object being hit.
		/// </summary>
		/// <param name="p">The point to test.</param>
		/// <remarks>
		/// At present, only a single item is returned in the list, as soon as a match has been made.
		/// It appears that the intention might have been to return all the objects in the match?
		/// </remarks>
		public System.Collections.ArrayList HitTest(Point p)
		{
			System.Collections.ArrayList a = new System.Collections.ArrayList();

			// this is the case if PlotSurface has been cleared.
			if (bbXAxis1Cache == null) {
				return a;
			}
			else if (bbXAxis1Cache != null && ((Rectangle) bbXAxis1Cache).Contains(p)) {
				a.Add ( XAxis1 );
				return a;
			}
			else if (bbYAxis1Cache != null && ((Rectangle) bbYAxis1Cache).Contains(p)) {
				a.Add ( YAxis1 );
				return a;
			}
			else if (bbXAxis2Cache != null && ((Rectangle) bbXAxis2Cache).Contains(p)) {
				a.Add ( XAxis2 );
				return a;
			}
			else if (bbXAxis2Cache != null && ((Rectangle) bbYAxis2Cache).Contains(p)) {
				a.Add ( YAxis2 );
				return a;
			}
			else if (bbTitleCache != null && ((Rectangle) bbTitleCache).Contains(p)) {
				a.Add( this );
				return a;
			}
			else if (plotAreaBoundingBoxCache != null && ((Rectangle)plotAreaBoundingBoxCache).Contains(p)) {
				a.Add( this );
				return a;
			}
			return a;
		}

		private double DetermineScaleFactor (double w, double h)
		{
			double diag = Math.Sqrt (w*w +  h*h);
			double scaleFactor = (diag / 1400.0)*2.4;
			
			if (scaleFactor > 1.0) {
				return scaleFactor;
			}
			else {
				return 1.0;
			}
		}

		/// <summary>
		/// Adds a drawable object to the plot surface with z-order 0. If the object is an IPlot,
		/// the PlotSurface axes will also be updated. 
		/// </summary>
		/// <param name="p">The IDrawable object to add to the plot surface.</param>
		public void Add (IDrawable p)
		{
			Add (p, 0);
		}

		/// <summary>
		/// Adds a drawable object to the plot surface. If the object is an IPlot, 
		/// the PlotSurface axes will also be updated.
		/// </summary>
		/// <param name="p">The IDrawable object to add to the plot surface.</param>
		/// <param name="zOrder">The z-ordering when drawing (objects with lower numbers are drawn first)</param>
		public void Add (IDrawable p, int zOrder)
		{
			Add (p, XAxisPosition.Bottom, YAxisPosition.Left, zOrder);
		}

		/// <summary>
		/// Adds a drawable object to the plot surface against the specified axes with
		/// z-order of 0. If the object is an IPlot, the PlotSurface axes will also
		/// be updated.
		/// </summary>
		/// <param name="p">the IDrawable object to add to the plot surface</param>
		/// <param name="xp">the x-axis to add the plot against.</param>
		/// <param name="yp">the y-axis to add the plot against.</param>
		public void Add (IDrawable p, XAxisPosition xp, YAxisPosition yp)
		{
			Add (p, xp, yp, 0);
		}

		/// <summary>
		/// Adds a drawable object to the plot surface against the specified axes. If
		/// the object is an IPlot, the PlotSurface axes will also be updated.
		/// </summary>
		/// <param name="p">the IDrawable object to add to the plot surface</param>
		/// <param name="xp">the x-axis to add the plot against.</param>
		/// <param name="yp">the y-axis to add the plot against.</param>
		/// <param name="zOrder">The z-ordering when drawing (objects with lower numbers are drawn first)</param>
		public void Add (IDrawable p, XAxisPosition xp, YAxisPosition yp, int zOrder)
		{
			drawables.Add (p);
			xAxisPositions.Add (xp);
			yAxisPositions.Add (yp);
			zPositions.Add ((double)zOrder);
			// fraction is to make key unique. With 10 million plots at same z, this buggers up.. 
			double fraction = (double)(++uniqueCounter)/10000000.0; 
			ordering.Add ((double)zOrder + fraction, drawables.Count - 1);
			
			// if p is just an IDrawable, then it can't affect the axes, but Plots do.
			if (p is IPlot) {
				UpdateAxes (false);
			}
		}

		private void UpdateAxes (bool recalculateAll)
		{
			if (drawables.Count != xAxisPositions.Count || drawables.Count != yAxisPositions.Count) {
				throw new XwPlotException("plots and axis position arrays our of sync");
			}

			int position = 0;

			// if we're not recalculating axes using all iplots then set
			// position to last one in list.
			if (!recalculateAll) {
				position = drawables.Count - 1;
				if (position < 0) position = 0;
			}

			if (recalculateAll) {
				XAxis1 = null;
				YAxis1 = null;
				XAxis2 = null;
				YAxis2 = null;
			}

			for (int i = position; i < drawables.Count; ++i) {
				// only update axes if this drawable is an IPlot.
				if (!(drawables[i] is IPlot)) 
					continue;

				IPlot p = (IPlot)drawables[i];
				XAxisPosition xap = (XAxisPosition)xAxisPositions[i];
				YAxisPosition yap = (YAxisPosition)yAxisPositions[i];

				if (xap == XAxisPosition.Bottom) {
					if (XAxis1 == null) {
						XAxis1 = p.SuggestXAxis();
						if (XAxis1 != null) {
							XAxis1.TicksAngle = -Math.PI / 2.0;
						}
					}
					else {
						XAxis1.LUB(p.SuggestXAxis());
					}

					if (XAxis1 != null) {
						XAxis1.MinPhysicalLargeTickStep = 50;

						if (AutoScaleAutoGeneratedAxes) {
							XAxis1.AutoScaleText = true;
							XAxis1.AutoScaleTicks = true;
							XAxis1.TicksIndependentOfPhysicalExtent = true;
						}
						else {
							XAxis1.AutoScaleText = false;
							XAxis1.AutoScaleTicks = false;
							XAxis1.TicksIndependentOfPhysicalExtent = false;
						}
					}
				}

				if (xap == XAxisPosition.Top) {
					if (XAxis2 == null) {
						XAxis2 = p.SuggestXAxis();
						if (XAxis2 != null) {
							XAxis2.TicksAngle = Math.PI / 2.0;
						}
					}
					else 					{
						XAxis2.LUB(p.SuggestXAxis());
					}

					if (XAxis2 != null) {
						XAxis2.MinPhysicalLargeTickStep = 50;

						if (AutoScaleAutoGeneratedAxes) {
							XAxis2.AutoScaleText = true;
							XAxis2.AutoScaleTicks = true;
							XAxis2.TicksIndependentOfPhysicalExtent = true;
						}
						else {
							XAxis2.AutoScaleText = false;
							XAxis2.AutoScaleTicks = false;
							XAxis2.TicksIndependentOfPhysicalExtent = false;
						}
					}
				}

				if (yap == YAxisPosition.Left) {
					if (YAxis1 == null) {
						YAxis1 = p.SuggestYAxis();
						if (YAxis1 != null) {
							YAxis1.TicksAngle = Math.PI / 2.0;
						}
					}
					else {
						YAxis1.LUB(p.SuggestYAxis());
					}

					if (YAxis1 != null) {
						if (AutoScaleAutoGeneratedAxes) {
							YAxis1.AutoScaleText = true;
							YAxis1.AutoScaleTicks = true;
							YAxis1.TicksIndependentOfPhysicalExtent = true;
						}
						else {
							YAxis1.AutoScaleText = false;
							YAxis1.AutoScaleTicks = false;
							YAxis1.TicksIndependentOfPhysicalExtent = false;
						}
					}
				}

				if (yap == YAxisPosition.Right) {
					if (YAxis2 == null) {
						YAxis2 = p.SuggestYAxis();
						if (YAxis2 != null) {
							YAxis2.TicksAngle = -Math.PI / 2.0;
						}
					}
					else {
						YAxis2.LUB(p.SuggestYAxis());
					}

					if (YAxis2 != null) {
						if (AutoScaleAutoGeneratedAxes) {
							YAxis2.AutoScaleText = true;
							YAxis2.AutoScaleTicks = true;
							YAxis2.TicksIndependentOfPhysicalExtent = true;
						}
						else {
							YAxis2.AutoScaleText = false;
							YAxis2.AutoScaleTicks = false;
							YAxis2.TicksIndependentOfPhysicalExtent = false;
						}
					}
				}
			}
		}

		private void DetermineAxesToDraw (out Axis xAxis_1, out Axis xAxis_2, out Axis yAxis_1, out Axis yAxis_2)
		{
			xAxis_1 = XAxis1;
			xAxis_2 = XAxis2;
			yAxis_1 = YAxis1;
			yAxis_2 = YAxis2;

			if (XAxis1 == null) {
				if (XAxis2 == null) {
					throw new XwPlotException ("Error: No X-Axis specified");
				}
				xAxis_1 = (Axis)XAxis2.Clone ();
				xAxis_1.HideTickText = true;
				xAxis_1.TicksAngle = -Math.PI / 2;
			}

			if (XAxis2 == null) {
				// don't need to check if XAxis1 == null, as case already handled above.
				xAxis_2 = (Axis)XAxis1.Clone ();
				xAxis_2.HideTickText = true;
				xAxis_2.TicksAngle = Math.PI / 2.0;
			}

			if (YAxis1 == null) {
				if (YAxis2 == null) {
					throw new XwPlotException  ("Error: No Y-Axis specified");
				}
				yAxis_1 = (Axis)YAxis2.Clone();
				yAxis_1.HideTickText = true;
				yAxis_1.TicksAngle = Math.PI / 2.0;
			}

			if (YAxis2 == null) {
				// don't need to check if YAxis1 == null, as case already handled above.
				yAxis_2 = (Axis)YAxis1.Clone();
				yAxis_2.HideTickText = true;
				yAxis_2.TicksAngle = -Math.PI / 2.0;
			}
		}

		private void DeterminePhysicalAxesToDraw (Rectangle bounds, 
			Axis XAxis1, Axis XAxis2, Axis YAxis1, Axis YAxis2,
			out PhysicalAxis pXAxis1, out PhysicalAxis pXAxis2, 
			out PhysicalAxis pYAxis1, out PhysicalAxis pYAxis2 )
		{
			Rectangle cb = bounds;

			pXAxis1 = new PhysicalAxis (XAxis1,
				new Point (cb.Left, cb.Bottom), new Point (cb.Right, cb.Bottom) );
			pYAxis1 = new PhysicalAxis (YAxis1,
				new Point (cb.Left, cb.Bottom), new Point (cb.Left, cb.Top) );
			pXAxis2 = new PhysicalAxis (XAxis2,
				new Point (cb.Left, cb.Top), new Point (cb.Right, cb.Top) );
			pYAxis2 = new PhysicalAxis (YAxis2,
				new Point (cb.Right, cb.Bottom), new Point (cb.Right, cb.Top) );

			double bottomIndent = Padding;
			if (!pXAxis1.Axis.Hidden) {
				// evaluate its bounding box
				Rectangle bb = pXAxis1.GetBoundingBox ();
				// finally determine its indentation from the bottom
				bottomIndent = bottomIndent + bb.Bottom - cb.Bottom;
			}

			double leftIndent = Padding;
			if (!pYAxis1.Axis.Hidden) {
				// evaluate its bounding box
				Rectangle bb = pYAxis1.GetBoundingBox();
				// finally determine its indentation from the left
				leftIndent = leftIndent - bb.Left + cb.Left;
			}

			// determine title size
			double scale = DetermineScaleFactor (bounds.Width, bounds.Height);
			Font scaled_font;
			if (AutoScaleTitle) {
				scaled_font = TitleFont.WithScaledSize (scale);
			}
			else {
				scaled_font = TitleFont;
			}

			Size titleSize;
			using (TextLayout layout = new TextLayout ()) {
				layout.Font = scaled_font;
				layout.Text = Title;
				titleSize = layout.GetSize ();
			};
			double topIndent = Padding;

			if (!pXAxis2.Axis.Hidden) {
				// evaluate its bounding box
				Rectangle bb = pXAxis2.GetBoundingBox();
				topIndent = topIndent - bb.Top + cb.Top;

				// finally determine its indentation from the top
				// correct top indendation to take into account plot title
				if (Title != "" ) {
					topIndent += titleSize.Height * 1.3;
				}
			}

			double rightIndent = Padding;
			if (!pYAxis2.Axis.Hidden) {
				// evaluate its bounding box
				Rectangle bb = pYAxis2.GetBoundingBox();

				// finally determine its indentation from the right
				rightIndent += (bb.Right-cb.Right);
			}

			// now we have all the default calculated positions and we can proceed to
			// "move" the axes to their right places

			// primary axes (bottom, left)
			pXAxis1.PhysicalMin = new Point( cb.Left+leftIndent, cb.Bottom-bottomIndent );
			pXAxis1.PhysicalMax = new Point( cb.Right-rightIndent, cb.Bottom-bottomIndent );
			pYAxis1.PhysicalMin = new Point( cb.Left+leftIndent, cb.Bottom-bottomIndent );
			pYAxis1.PhysicalMax = new Point( cb.Left+leftIndent, cb.Top+topIndent );

			// secondary axes (top, right)
			pXAxis2.PhysicalMin = new Point( cb.Left+leftIndent, cb.Top+topIndent );
			pXAxis2.PhysicalMax = new Point( cb.Right-rightIndent, cb.Top+topIndent );
			pYAxis2.PhysicalMin = new Point( cb.Right-rightIndent, cb.Bottom-bottomIndent );
			pYAxis2.PhysicalMax = new Point( cb.Right-rightIndent, cb.Top+topIndent );
		}

		/// <summary>
		/// Draw the the PlotSurface and contents (axes, drawables, and legend) using the
		/// Drawing Context supplied and the bounding rectangle for the PlotSurface to cover
		/// </summary>
		/// <param name="ctx">The Drawing Context with which to draw.</param>
		/// <param name="bounds">The rectangle within which to draw</param>
		public void Draw (Context ctx, Rectangle bounds)
		{
			Point titleOrigin = Point.Zero;

			ctx.Save ();

			// determine font sizes and tick scale factor.
			double scale = DetermineScaleFactor (bounds.Width, bounds.Height);

			// if there is nothing to plot, draw title and return.
			if (drawables.Count == 0) {
				// draw title
				//TODO: Title should be centred here - not its origin
				Point origin = Point.Zero;
				titleOrigin.X = bounds.Width/2;
				titleOrigin.Y = bounds.Height/2;
				DrawTitle (ctx, titleOrigin, scale);
				ctx.Restore ();
				return;
			}

			// determine the [non physical] axes to draw based on the axis properties set.
			Axis XAxis1 = null;
			Axis XAxis2 = null;
			Axis YAxis1 = null;
			Axis YAxis2 = null;
			DetermineAxesToDraw (out XAxis1, out XAxis2, out YAxis1, out YAxis2);

			// apply scale factor to axes as desired.

			if (XAxis1.AutoScaleTicks) {
				XAxis1.TickScale = scale;
			}
			if (XAxis1.AutoScaleText) {
				XAxis1.FontScale = scale;
			}
			if (YAxis1.AutoScaleTicks) {
				YAxis1.TickScale = scale;
			}
			if (YAxis1.AutoScaleText) {
				YAxis1.FontScale = scale;
			}
			if (XAxis2.AutoScaleTicks) {
				XAxis2.TickScale = scale;
			} 
			if (XAxis2.AutoScaleText) {
				XAxis2.FontScale = scale;
			}
			if (YAxis2.AutoScaleTicks) {
				YAxis2.TickScale = scale;
			}
			if (YAxis2.AutoScaleText) {
				YAxis2.FontScale = scale;
			}

			// determine the default physical positioning of those axes.
			PhysicalAxis pXAxis1 = null;
			PhysicalAxis pYAxis1 = null;
			PhysicalAxis pXAxis2 = null;
			PhysicalAxis pYAxis2 = null;
			DeterminePhysicalAxesToDraw ( 
				bounds, XAxis1, XAxis2, YAxis1, YAxis2,
				out pXAxis1, out pXAxis2, out pYAxis1, out pYAxis2 );

			double oldXAxis2Height = pXAxis2.PhysicalMin.Y;

			// Apply axes constraints
			for (int i=0; i<axesConstraints.Count; ++i) {
				((AxesConstraint)axesConstraints[i]).ApplyConstraint( 
					pXAxis1, pYAxis1, pXAxis2, pYAxis2 );
			}

			// draw legend if have one.
			// Note: this will update axes if necessary. 
			Point legendPosition = new Point(0,0);
			if (legend != null) {
				legend.UpdateAxesPositions ( 
					pXAxis1, pYAxis1, pXAxis2, pYAxis2,
					drawables, scale, Padding, bounds, 
					out legendPosition );
			}

			double newXAxis2Height = pXAxis2.PhysicalMin.Y;
			double titleExtraOffset = oldXAxis2Height - newXAxis2Height;
	
			// now we are ready to define the clipping region
			plotAreaBoundingBoxCache = new Rectangle ( 
				Math.Min (pXAxis1.PhysicalMin.X, pXAxis1.PhysicalMax.X),
				Math.Min (pYAxis1.PhysicalMax.Y, pYAxis1.PhysicalMin.Y),
				Math.Abs (pXAxis1.PhysicalMax.X - pXAxis1.PhysicalMin.X + 1),
				Math.Abs (pYAxis1.PhysicalMin.Y - pYAxis1.PhysicalMax.Y + 1)
			);
			bbXAxis1Cache = pXAxis1.GetBoundingBox ();
			bbXAxis2Cache = pXAxis2.GetBoundingBox ();
			bbYAxis1Cache = pYAxis1.GetBoundingBox ();
			bbYAxis2Cache = pYAxis2.GetBoundingBox ();

			Rectangle plotBounds = (Rectangle)plotAreaBoundingBoxCache;

			// set the clipping region.. (necessary for zoom)
			// Note: although clipping is enforced by the clip region, it is probably more efficient
			// for each Drawable to check against the plotBounds and not draw if points are outside.
			// This hasn't yet been implemented
			ctx.Save ();
			ctx.Rectangle (plotBounds);
			ctx.Clip ();

			// Fill in the plot background. 
			if (plotBackImage != null) {
				// Ensure plotBounds has integer size for correct tiling/drawing
				plotBounds.Width = Math.Truncate (plotBounds.Width);
				plotBounds.Height = Math.Truncate (plotBounds.Height);
				ctx.DrawImage (Utils.TiledImage (plotBackImage , plotBounds.Size), plotBounds);
			}
			else if (plotBackGradient != null) {
				// Scale plotBackGradient to plotBounds
				double startX = plotBounds.X + (plotBackGradient.StartPoint.X * plotBounds.Width);
				double startY = plotBounds.Y + (plotBackGradient.StartPoint.Y * plotBounds.Height);
				double endX = plotBounds.X + (plotBackGradient.EndPoint.X * plotBounds.Width);
				double endY = plotBounds.Y + (plotBackGradient.EndPoint.Y * plotBounds.Height);
				LinearGradient g = new LinearGradient (startX, startY, endX, endY);
				g.AddColorStop (0, plotBackGradient.StartColor);
				g.AddColorStop (1, plotBackGradient.EndColor);
				ctx.Rectangle (plotBounds);
				ctx.Pattern = g;
				ctx.Fill ();
			} 
			else {
				ctx.Rectangle (plotBounds);
				ctx.SetColor (plotBackColor);
				ctx.Fill ();
			}

			// draw title at centre of Physical X-axis and at top of plot

			titleOrigin.X = (pXAxis2.PhysicalMax.X + pXAxis2.PhysicalMin.X)/2.0;
			titleOrigin.Y = bounds.Top + Padding - titleExtraOffset;
			Size s = DrawTitle (ctx, titleOrigin, scale);

			bbTitleCache = new Rectangle (titleOrigin.X-s.Width/2, titleOrigin.Y, s.Width, s.Height);

			// draw drawables..
			bool legendDrawn = false;

			for (int i_o = 0; i_o < ordering.Count; ++i_o) {
	
				int i = (int)ordering.GetByIndex (i_o);
				double zOrder = (double)ordering.GetKey (i_o);
				if (zOrder > legendZOrder) {
					// draw legend.
					if (!legendDrawn && legend != null) {
						legend.Draw (ctx, legendPosition, drawables, scale);
						legendDrawn = true;
					}
				}

				IDrawable drawable = (IDrawable)drawables[i];
				XAxisPosition xap = (XAxisPosition)xAxisPositions[i];
				YAxisPosition yap = (YAxisPosition)yAxisPositions[i];

				PhysicalAxis drawXAxis;
				PhysicalAxis drawYAxis;

				if (xap == XAxisPosition.Bottom) {
					drawXAxis = pXAxis1;
				}
				else {
					drawXAxis = pXAxis2;
				}

				if (yap == YAxisPosition.Left) {
					drawYAxis = pYAxis1;
				}
				else {
					drawYAxis = pYAxis2;
				}
	
				drawable.Draw (ctx, drawXAxis, drawYAxis);

			}

			if (!legendDrawn && legend != null) {
				legend.Draw (ctx, legendPosition, drawables, scale);
			}

			ctx.Restore ();		// end of clipping region

			// cache the physical axes we used on this draw;
			pXAxis1Cache = pXAxis1;
			pYAxis1Cache = pYAxis1;
			pXAxis2Cache = pXAxis2;
			pYAxis2Cache = pYAxis2;

			// now draw axes.
			Rectangle axisBounds;
			pXAxis1.Draw (ctx, out axisBounds);
			pXAxis2.Draw (ctx, out axisBounds);
			pYAxis1.Draw (ctx, out axisBounds);
			pYAxis2.Draw (ctx, out axisBounds);

#if DEBUG_BOUNDING_BOXES
			ctx.SetColor (Colors.Orange);
			ctx.Rectangle ((Rectangle)bbXAxis1Cache);
			ctx.Rectangle ((Rectangle)bbXAxis2Cache);
			ctx.Rectangle ((Rectangle)bbYAxis1Cache);
			ctx.Rectangle ((Rectangle)bbYAxis2Cache);
			ctx.Stroke ();
			ctx.SetColor (Colors.Red);
			ctx.Rectangle ((Rectangle)plotAreaBoundingBoxCache);
			ctx.Rectangle ((Rectangle)bbTitleCache);
			ctx.Stroke ();
#endif
			ctx.Restore ();
		}

		/// <summary>
		/// Draws the title using the Drawing Context, Origin (x,y) and scale
		/// </summary>
		/// <returns>
		/// The Size required for the title
		/// </returns>
		/// TODO: Add a MeasureTitle routine, since can measure TextLayout now
		/// 
		private Size DrawTitle (Context ctx, Point origin, double scale)
		{
			ctx.Save ();
			ctx.SetColor (TitleColor);
			Font scaled_font;
			if (AutoScaleTitle) {
				scaled_font = TitleFont.WithScaledSize (scale);
			}
			else {
				scaled_font = TitleFont;
			}

			TextLayout layout = new TextLayout ();
			layout.Font = scaled_font;
			layout.Text = Title;
			Size titleSize = layout.GetSize ();
			origin.X -= titleSize.Width/2;

			ctx.DrawTextLayout (layout, origin);
			ctx.Restore ();

			return titleSize;
		}


		/// <summary>
		/// Add an axis constraint to the plot surface. Axes constraints give you 
		/// control over where NPlot positions each axes, and the world - pixel
		/// ratio.
		/// </summary>
		/// <param name="constraint">The axis constraint to add.</param>
		public void AddAxesConstraint( AxesConstraint constraint )
		{
			axesConstraints.Add( constraint );
		}


		/// <summary>
		/// Remove a drawable object. 
		/// Note that axes are not updated.
		/// </summary>
		/// <param name="p">Drawable to remove.</param>
		/// <param name="updateAxes">if true, the axes are updated.</param>
		public void Remove (IDrawable p, bool updateAxes) 
		{
			int index = drawables.IndexOf (p);
			if (index < 0)
				return;
			drawables.RemoveAt (index);
			xAxisPositions.RemoveAt (index);
			yAxisPositions.RemoveAt (index);
			zPositions.RemoveAt(index);

			if (updateAxes) {
				UpdateAxes (true);
			}

			RefreshZOrdering();
		}


		/// <summary>
		/// If a plot is removed, then the ordering list needs to be 
		/// recalculated. 
		/// </summary>
		private void RefreshZOrdering () 
		{
			uniqueCounter = 0;
			ordering = new SortedList ();
			for (int i = 0; i < zPositions.Count; ++i) {
				double zpos = Convert.ToDouble (zPositions[i]);
				double fraction = (double)(++uniqueCounter) / 10000000.0;
				double d = zpos + fraction;
				ordering.Add (d, i);
			}
		}


		/// <summary>
		/// Returns the x-axis associated with a given plot (XAxis1 is at Bottom).
		/// </summary>
		/// <param name="plot">the plot to get associated x-axis.</param>
		/// <returns>the axis associated with the plot.</returns>
		public Axis WhichXAxis (IPlot plot)
		{
			int index = drawables.IndexOf (plot);
			XAxisPosition p = (XAxisPosition)xAxisPositions[index];
			if (p == XAxisPosition.Bottom) 
				return XAxis1;
			else
				return XAxis2;
		}


		/// <summary>
		/// Returns the y-axis associated with a given plot (YAxis1 is at Left).
		/// </summary>
		/// <param name="plot">the plot to get associated y-axis.</param>
		/// <returns>the axis associated with the plot.</returns>
		public Axis WhichYAxis (IPlot plot)
		{
			int index = drawables.IndexOf (plot);
			YAxisPosition p = (YAxisPosition)yAxisPositions[index];
			if (p == YAxisPosition.Left)
				return YAxis1;
			else
				return YAxis2;
		}

	} 

} 


