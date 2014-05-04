//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// HistogramPlot.cs
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
	/// Provides the ability to draw histogram plots
	/// </summary>
	public class HistogramPlot : BaseSequencePlot, IPlot, ISequencePlot
	{
		Color fillColor = Colors.DarkGray;
		ColorGradient fillGradient = null;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public HistogramPlot ()
		{
			// set default properties
			Init ();
		}

		void Init ()
		{
			// Set up all default properties
			fillColor = Colors.DarkGray;
			fillGradient = null;
			BorderColor = Colors.Black;
			BaseOffset = 0;
			Center = true;
			IsStacked = false;
			Filled = false;
		}

		/// <summary>
		/// The fill color used for the bars. Mutually exclusive with FillGradient
		/// </summary>
		public Color FillColor {
			get { return fillColor; }
			set {
				fillColor = value;
				fillGradient = null;
			}
		}

		/// <summary>
		/// The ColorGradient used for the bars. Mutually exclusive with FillColor
		/// </summary>
		public ColorGradient FillGradient {
			get { return fillGradient; }
			set {
				fillGradient = value;
				fillColor = Colors.DarkGray;
			}
		}

		/// <summary>
		/// The color of the pen used to draw lines in this plot.
		/// </summary>
		public Color BorderColor { get; set; }

		/// <summary>
		/// Horizontal position of histogram columns is offset by this much (in world coordinates).
		/// </summary>
		public double BaseOffset { get; set; }

		/// <summary>
		/// If true, each histogram column will be centered on the associated abscissa value.
		/// If false, each histogram colum will be drawn between the associated abscissa value, and the next abscissa value.
		/// Default value is true.
		/// </summary>
		public bool Center { get; set; }

		/// <summary>
		/// If this histogram plot has another stacked on top, this will be true. Else false.
		/// </summary>
		public bool IsStacked { get; set; }

		/// <summary>
		/// Whether or not the histogram columns will be filled.
		/// </summary>
		public bool Filled { get; set; }

		private double baseWidth = 1.0;
		/// <summary>
		/// The width of the histogram bar as a proportion of the data spacing 
		/// (in range 0.0 - 1.0).
		/// </summary>
		public double BaseWidth
		{
			get { return baseWidth; }
			set
			{
				if (value > 0.0 && value <= 1.0) {
					baseWidth = value;
				}
				else {
						throw new XwPlotException ("Base width must be between 0.0 and 1.0");
				}
			}
		}

		/// <summary>
		/// Returns an x-axis that is suitable for drawing this plot.
		/// </summary>
		/// <returns>A suitable x-axis.</returns>
		public Axis SuggestXAxis()
		{
			SequenceAdapter data = new SequenceAdapter (DataSource, DataMember, OrdinateData, AbscissaData);

			Axis a = data.SuggestXAxis();
			if (data.Count==0) {
				return a;
			}

			Point p1;
			Point p2;
			Point p3;
			Point p4;
			if (data.Count < 2) {
				p1 = data [0];
				p1.X -= 1.0;
				p2 = data [0];
				p3 = p1;
				p4 = p2;
			}
			else {
				p1 = data [0];
				p2 = data [1];
				p3 = data [data.Count-2];
				p4 = data [data.Count-1];
			}

			double offset1;
			double offset2;

			if (!Center) {
				offset1 = 0.0;
				offset2 = p4.X - p3.X;
			}
			else {
				offset1 = (p2.X - p1.X)/2.0f;
				offset2 = (p4.X - p3.X)/2.0f;
			}

			a.WorldMin -= offset1;
			a.WorldMax += offset2;

			return a;
		}

		/// <summary>
		/// Returns a y-axis that is suitable for drawing this plot.
		/// </summary>
		/// <returns>A suitable y-axis.</returns>
		public Axis SuggestYAxis ()
		{
			if (IsStacked) {
				double tmpMax = 0.0f;
				ArrayList adapterList = new ArrayList();

				HistogramPlot currentPlot = this;
				do {
					adapterList.Add (new SequenceAdapter ( 
						currentPlot.DataSource,
						currentPlot.DataMember,
						currentPlot.OrdinateData, 
						currentPlot.AbscissaData)
					);
				} while ((currentPlot = currentPlot.stackedTo) != null);
				
				SequenceAdapter[] adapters = (SequenceAdapter[])adapterList.ToArray (typeof(SequenceAdapter));
				
				for (int i=0; i<adapters[0].Count; ++i) {
					double tmpHeight = 0.0f;
					for (int j=0; j<adapters.Length; ++j) {
						tmpHeight += adapters[j][i].Y;
					}
					tmpMax = Math.Max (tmpMax, tmpHeight);
				}

				Axis a = new LinearAxis (0.0,tmpMax);
				// TODO make 0.08 a parameter.
				a.IncreaseRange (0.08);
				return a;
			}
			else {
				SequenceAdapter data = new SequenceAdapter (DataSource, DataMember, OrdinateData, AbscissaData);
				return data.SuggestYAxis();
			}
		}

		/// <summary>
		/// Draws the histogram.
		/// </summary>
		/// <param name="ctx">The Drawing Context with which to draw</param>
		/// <param name="xAxis">The X-Axis to draw against.</param>
		/// <param name="yAxis">The Y-Axis to draw against.</param>
		public void Draw (Context ctx, PhysicalAxis xAxis, PhysicalAxis yAxis)
		{
			double yoff;
			SequenceAdapter data = new SequenceAdapter (DataSource, DataMember, OrdinateData, AbscissaData);

			ctx.Save ();
			ctx.SetLineWidth (1);

			for (int i=0; i<data.Count; ++i ) {

				// (1) determine the top left hand point of the bar (assuming not centered)
				Point p1 = data[i];
				if (double.IsNaN(p1.X) || double.IsNaN(p1.Y)) {
					continue;
				}

				// (2) determine the top right hand point of the bar (assuming not centered)
				Point p2 = Point.Zero;;
				if (i+1 != data.Count) {
					p2 = data[i+1];
					if (double.IsNaN(p2.X) || double.IsNaN(p2.Y)) {
						continue;
					}
					p2.Y = p1.Y;
				}
				else if (i != 0) {
					p2 = data[i-1];
					if (double.IsNaN(p2.X) || double.IsNaN(p2.Y)) {
						continue;
					}
					double offset = p1.X - p2.X;
					p2.X = p1.X + offset;
					p2.Y = p1.Y;
				}
				else {
					double offset = 1.0;
					p2.X = p1.X + offset;
					p2.Y = p1.Y;
				}

				// (3) now account for plots this may be stacked on top of.
				HistogramPlot currentPlot = this;
				yoff = 0.0;
				double yval = 0.0;
				while (currentPlot.IsStacked) {
					SequenceAdapter stackedToData = new SequenceAdapter (
						currentPlot.stackedTo.DataSource, 
						currentPlot.stackedTo.DataMember,
						currentPlot.stackedTo.OrdinateData, 
						currentPlot.stackedTo.AbscissaData );

					yval += stackedToData[i].Y;
					yoff = yAxis.WorldToPhysical (yval, false).Y;
					p1.Y += stackedToData[i].Y;
					p2.Y += stackedToData[i].Y;
					currentPlot = currentPlot.stackedTo;
				}

				// (4) now account for centering
				if (Center) {
					double offset = (p2.X - p1.X) / 2.0;
					p1.X -= offset;
					p2.X -= offset;
				}

				// (5) now account for BaseOffset (shift of bar sideways).
				p1.X += BaseOffset;
				p2.X += BaseOffset;

				// (6) now get physical coordinates of top two points.
				Point xPos1 = xAxis.WorldToPhysical (p1.X, false);
				Point yPos1 = yAxis.WorldToPhysical (p1.Y, false);
				Point xPos2 = xAxis.WorldToPhysical (p2.X, false);

				if (IsStacked) {
					currentPlot = this;
					while (currentPlot.IsStacked) {
						currentPlot = currentPlot.stackedTo;
					}
					baseWidth = currentPlot.baseWidth;
				}

				double width = xPos2.X - xPos1.X;
				double height;
				if (IsStacked) {
					height = -yPos1.Y+yoff;
				}
				else {
					height = -yPos1.Y+yAxis.PhysicalMin.Y;
				}

				double xoff = (1.0 - baseWidth)/2.0*width;
				Rectangle bar = new Rectangle (xPos1.X+xoff, yPos1.Y, width-2*xoff, height);

				ctx.Rectangle (bar);
				if (Filled) {
					if (bar.Height != 0 && bar.Width != 0) {
						if (FillGradient != null) {
							// Scale plotBackGradient to bar rectangle
							double sX = bar.X + fillGradient.StartPoint.X * bar.Width;
							double sY = bar.Y + fillGradient.StartPoint.Y * bar.Height;
							double eX = bar.X + fillGradient.EndPoint.X * bar.Width;
							double eY = bar.Y + fillGradient.EndPoint.Y * bar.Height;
							LinearGradient g = new LinearGradient (sX, sY, eX, eY);
							g.AddColorStop (0, FillGradient.StartColor);
							g.AddColorStop (1, FillGradient.EndColor);
							ctx.Pattern = g;
						} else {
							ctx.SetColor (FillColor);
						}
						ctx.FillPreserve ();
					}
				}
				ctx.SetColor (BorderColor);
				ctx.Stroke ();
			}
			ctx.Restore ();
		}

	/*
		private double centerLine = 0.0;
		/// <summary>
		/// Histogram bars extend from the data value to this value. Default value is 0.
		/// </summary>
		public double CenterLine
		{
			get { return centerLine; }
			set { centerLine = value; }
		}
	*/

		private HistogramPlot stackedTo;
		/// <summary>
		/// Stack the histogram to another HistogramPlot.
		/// </summary>
		public void StackedTo (HistogramPlot hp)
		{
			SequenceAdapter data = new SequenceAdapter (DataSource, DataMember, OrdinateData, AbscissaData);
			SequenceAdapter hpData = new SequenceAdapter (hp.DataSource, hp.DataMember, hp.OrdinateData, hp.AbscissaData);
			if (hp != null) {
				IsStacked = true;
				if (hpData.Count != data.Count) {
					throw new XwPlotException ("Can stack HistogramPlot data only with the same number of datapoints");
				}
				for (int i=0; i < data.Count; ++i) {
					if (data[i].X != hpData[i].X) {
						throw new XwPlotException ("Can stack HistogramPlot data only with the same X coordinates");
					}
					if (hpData[i].Y < 0.0) {
						throw new XwPlotException ("Can stack HistogramPlot data only with positive Y coordinates");
					}
				}
			}
			stackedTo = hp;
		}

		/// <summary>
		/// Draws a representation of this plot in the legend.
		/// </summary>
		/// <param name="ctx">The Drawing Context with which to draw.</param>
		/// <param name="r">
		/// A rectangle specifying the bounds of the area in the legend set aside for drawing
		/// </param>
		public void DrawInLegend (Context ctx, Rectangle r)
		{
			ctx.Save ();
			ctx.SetLineWidth (1);
			ctx.Rectangle (r);
			if (Filled) {
				if (FillGradient != null) {
					// Scale plotBackGradient to bar rectangle
					double sX = r.X + fillGradient.StartPoint.X * r.Width;
					double sY = r.Y + fillGradient.StartPoint.Y * r.Height;
					double eX = r.X + fillGradient.EndPoint.X * r.Width;
					double eY = r.Y + fillGradient.EndPoint.Y * r.Height;
					LinearGradient g = new LinearGradient (sX, sY, eX, eY);
					g.AddColorStop (0, FillGradient.StartColor);
					g.AddColorStop (1, FillGradient.EndColor);
					ctx.Pattern = g;
				} else {
					ctx.SetColor (FillColor);
				}
				ctx.FillPreserve ();
			}
			ctx.SetColor (BorderColor);
			ctx.Stroke ();
			ctx.Restore ();
		}
	}
}
