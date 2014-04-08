//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// ImagePlot.cs
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
using Xwt;
using Xwt.Drawing;

namespace XwPlot
{
	/// <summary>
	/// Encapsulates functionality for plotting data as a 2D image chart.
	/// </summary>
	public class ImagePlot : IPlot
	{
		private double[,] data;
		private double xStart = 0.0;
		private double yStart = 0.0;
		private double xStep = 1.0;
		private double yStep = 1.0;
		
		private double dataMin;
		private double dataMax;
		private IGradient gradient;
		private string label = "";
		private bool center = true;
		private bool showInLegend = true;

		/// <summary>
		/// At or below which value a minimum gradient color should be used.
		/// </summary>
		public double DataMin  
		{
			get	{
				return dataMin;
			}
			set {
				dataMin = value;
			}
		}

		/// <summary>
		/// At or above which value a maximum gradient color should be used.
		/// </summary>
		public double DataMax
		{
			get {
				return dataMax;
			}
			set {
				dataMax = value;
			}
		}

		/// <summary>
		/// Calculates the minimum and maximum values of the data array.
		/// </summary>
		private void calculateMinMax()
		{
			dataMin = data[0,0];
			dataMax = data[0,0];
			for (int i=0; i<data.GetLength(0); ++i) {
				for (int j=0; j<data.GetLength(1); ++j) {
					if (data[i,j]<dataMin) {
						dataMin = data[i,j];
					}
					if (data[i,j]>dataMax) {
						dataMax = data[i,j];
					}
				}
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="data">the 2D array to plot</param>
		/// <param name="xStart">the world value corresponding to the 1st position in the x-direction</param>
		/// <param name="xStep">the world step size between pixels in the x-direction.</param>
		/// <param name="yStart">the world value corresponding to the 1st position in the y-direction</param>
		/// <param name="yStep">the world step size between pixels in the y-direction.</param>
		/// <remarks>no adapters for this yet - when we get some more 2d
		/// plotting functionality, then perhaps create some.</remarks>
		public ImagePlot (double[,] data, double xStart, double xStep, double yStart, double yStep)
		{
			this.data = data;
			this.xStart = xStart;
			this.xStep = xStep;
			this.yStart = yStart;
			this.yStep = yStep;
			this.calculateMinMax();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="data">The 2D array to plot.</param>
		public ImagePlot (double[,] data)
		{
			this.data = data;
			this.calculateMinMax();
		}

		/// <summary>
		/// Draw using the Drawing Context and Axes supplied
		/// </summary>
		/// <remarks>TODO: block positions may be off by a pixel or so. maybe. Re-think calculations</remarks>
		public void Draw (Context ctx, PhysicalAxis xAxis, PhysicalAxis yAxis)
		{
			if (data==null || data.GetLength(0) == 0 || data.GetLength(1) == 0) {
				return;
			}

			double worldWidth = xAxis.Axis.WorldMax - xAxis.Axis.WorldMin;
			double numBlocksHorizontal = worldWidth / this.xStep;
			double worldHeight = yAxis.Axis.WorldMax - yAxis.Axis.WorldMin;
			double numBlocksVertical = worldHeight / this.yStep;

			double physicalWidth = xAxis.PhysicalMax.X - xAxis.PhysicalMin.X;
			double blockWidth = physicalWidth / numBlocksHorizontal;
			bool wPositive = true;
			if (blockWidth < 0.0) {
				wPositive = false;
			}
			blockWidth = Math.Abs (blockWidth)+1;

			double physicalHeight = yAxis.PhysicalMax.Y - yAxis.PhysicalMin.Y;
			double blockHeight = physicalHeight / numBlocksVertical;
			bool hPositive = true;
			if (blockHeight < 0.0) {
				hPositive = false;
			}
			blockHeight = Math.Abs(blockHeight)+1;

			ctx.Save ();
			for (int i=0; i<data.GetLength(0); ++i) {
				for (int j=0; j<data.GetLength(1); ++j) {
					double wX = (double)j*xStep + xStart;
					double wY = (double)i*yStep + yStart;
					if (!hPositive) {
						wY += yStep;
					}
					if (!wPositive ) {
						wX += xStep;
					}
					if (this.center) {
						wX -= this.xStep/2.0;
						wY -= this.yStep/2.0;
					}
					Color color = Gradient.GetColor ((data[i,j]-dataMin)/(dataMax-this.dataMin)); 
					ctx.SetColor (color);
					double x = xAxis.WorldToPhysical(wX,false).X;
					double y = yAxis.WorldToPhysical(wY,false).Y;
					ctx.Rectangle (x, y, blockWidth, blockHeight);
					ctx.Fill ();
				}
			}
			ctx.Restore ();
		}

		/// <summary>
		/// The gradient that specifies the mapping between value and color.
		/// </summary>
		/// <remarks>memory allocation in get may be inefficient.</remarks>
		public IGradient Gradient
		{
			get {
				if (gradient == null) {
					// TODO: suboptimal.
					gradient = new LinearGradient (Colors.Black, Colors.White);
				}
				return gradient;
			}
			set {
				gradient = value;
			}
		}

		/// <summary>
		/// Draws a representation of this plot in the legend.
		/// </summary>
		/// <param name="g">The graphics surface on which to draw.</param>
		/// <param name="startEnd">A rectangle specifying the bounds of the area in the legend set aside for drawing.</param>
		public void DrawInLegend (Context ctx, Rectangle startEnd)
		{
			// not implemented yet.
		}

		/// <summary>
		/// A label to associate with the plot - used in the legend.
		/// </summary>
		public string Label
		{
			get {
				return label;
			}
			set {
				this.label = value;
			}
		}

		/// <summary>
		/// Returns an x-axis that is suitable for drawing this plot.
		/// </summary>
		/// <returns>A suitable x-axis.</returns>
		public Axis SuggestXAxis ()
		{
			if (center) {
				return new LinearAxis (xStart - xStep/2.0, xStart + xStep * data.GetLength (1) - xStep/2.0);
			}
			return new LinearAxis (xStart, xStart + xStep * data.GetLength (1));
		}


		/// <summary>
		/// Returns a y-axis that is suitable for drawing this plot.
		/// </summary>
		/// <returns>A suitable y-axis.</returns>
		public Axis SuggestYAxis()
		{
			if (center) {
				return new LinearAxis (yStart - yStep/2.0, yStart + yStep * data.GetLength (0) - yStep/2.0);
			}
			return new LinearAxis (yStart, yStart + yStep * data.GetLength (0));
		}

		/// <summary>
		/// If true, pixels are centered on their respective coordinates. If false, they are drawn
		/// between their coordinates and the coordinates of the the next point in each direction.
		/// </summary>
		public bool Center
		{
			get {
				return center;
			}
			set {
				center = value;
			}
		}

		/// <summary>
		/// Whether or not to include an entry for this plot in the legend if it exists.
		/// </summary>
		public bool ShowInLegend
		{
			get {
				return showInLegend;
			}
			set {
				showInLegend = value;
			}
		}

		/// <summary>
		/// Write data associated with the plot as text.
		/// </summary>
		/// <param name="sb">the string builder to write to.</param>
		/// <param name="region">Only write out data in this region if onlyInRegion is true.</param>
		/// <param name="onlyInRegion">If true, only data in region is written, else all data is written.</param>
		/// <remarks>TODO: not implemented.</remarks>
		public void WriteData (System.Text.StringBuilder sb, Rectangle region, bool onlyInRegion )
		{
		}
	}
}
