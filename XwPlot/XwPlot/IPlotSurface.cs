//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// IPlotSurface.cs
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
	/// Defines the PlotSurface interface - All specific PlotSurface classes that
	/// use PlotSurface for their underlying operations should implement this class
	/// </summary>
	public interface IPlotSurface
	{
		/// <summary>
		/// Adds a drawable object to the plot surface. If the object is an IPlot, 
		/// the PlotSurface axes will also be updated.
		/// </summary>
		/// <param name="p">The IDrawable object to add to the plot surface.</param>
		/// <param name="zOrder">The z-ordering when drawing (objects with lower numbers are drawn first)</param>
		void Add (IDrawable p, int zOrder);

		/// <summary>
		/// Adds a drawable object to the plot surface against the specified axes. If
		/// the object is an IPlot, the PlotSurface axes will also be updated.
		/// </summary>
		/// <param name="p">the IDrawable object to add to the plot surface</param>
		/// <param name="xp">the x-axis to add the plot against.</param>
		/// <param name="yp">the y-axis to add the plot against.</param>
		/// <param name="zOrder">The z-ordering when drawing (objects with lower numbers are drawn first)</param>

		/// TODO: void Add (IDrawable p, NPlot.PlotSurface.XAxisPosition xp, NPlot.PlotSurface.YAxisPosition yp, int zOrder);

		/// <summary>
		/// Adds a drawable object to the plot surface. If the object is an IPlot, 
		/// the PlotSurface axes will also be updated.
		/// </summary>
		/// <param name="p">The IDrawable object to add to the plot surface.</param>
		void Add (IDrawable p);

		/// <summary>
		/// Adds a drawable object to the plot surface against the specified axes. If
		/// the object is an IPlot, the PlotSurface axes will also be updated.
		/// </summary>
		/// <param name="p">the IDrawable object to add to the plot surface</param>
		/// <param name="xax">the x-axis to add the plot against.</param>
		/// <param name="yax">the y-axis to add the plot against.</param>

		void Add (IDrawable p, XAxisPosition xp, YAxisPosition yp);

		/// <summary>
		/// Remove a drawable object from the plot surface.
		/// </summary>
		/// <param name="p">the object to remove</param>
		/// <param name="updateAxes">whether or not to update the axes after removal.</param>
		void Remove (IDrawable p, bool updateAxes);

		/// <summary>
		/// Clears the PlotSurface.
		/// </summary>
		void Clear ();

		/// <summary>
		/// Gets or Sets the legend to use with this plot surface.
		/// </summary>

		// TODO: NPlot.Legend Legend { get; set; }

		/// <summary>
		/// Setting this value determines the order (relative to IDrawables added to the plot surface)
		/// that the legend is drawn.
		/// </summary>
		int LegendZOrder { get; set; }

		/// <summary>
		/// The distance in pixels to leave between of the edge of the bounding rectangle
		/// supplied to the Draw method, and the markings that make up the plot.
		/// </summary>
		double Padding { get; set; }

		/// <summary>
		/// A color used to paint the plot background. Mutually exclusive with PlotBackImage and PlotBackBrush
		/// </summary>
		Color PlotBackColor { set; }
		
		/// <summary>
		/// An imaged used to paint the plot background.
		/// Mutually exclusive with PlotBackColor and PlotBackBrush
		/// </summary>
		BitmapImage PlotBackImage { set; }

		/// <summary>
		/// The plot surface title.
		/// </summary>
		string Title { get; set; }
		
		/// <summary>
		/// Whether or not the title will be scaled according to size of the plot 
		/// surface.
		/// </summary>
		bool AutoScaleTitle { get; set; }

		/// <summary>
		/// When plots are added to the plot surface, the axes they are attached to are
		/// immediately modified to reflect data of the plot. If AutoScaleAutoGeneratedAxes
		/// is true when a plot is added, the axes will be turned in to auto scaling ones if
		/// they are not already [ie tick marks, tick text and label size scaled to size of
		/// plot surface]. If false, axes will not be autoscaling.
		/// </summary>
		bool AutoScaleAutoGeneratedAxes { get; set; }

		/// <summary>
		/// Sets the title to be drawn using a solid brush of this color.
		/// </summary>
		Color TitleColor { set; }

		/// <summary>
		/// The plot title font.
		/// </summary>
		Font TitleFont { get; set; }

		/// <summary>
		/// Add an axis constraint to the plot surface. Axis constraints can
		/// specify relative world-pixel scalings, absolute axis positions etc.
		/// </summary>
		/// <param name="c">The axis constraint to add.</param>
		void AddAxesConstraint (AxesConstraint c);

		/// <summary>
		/// The bottom abscissa axis.
		/// </summary>
		Axis XAxis1 { get; set; }

		/// <summary>
		/// The top abscissa axis.
		/// </summary>
		Axis XAxis2 { get; set; }

		/// <summary>
		/// The left ordinate axis.
		/// </summary>
		Axis YAxis1 { get; set; }

		/// <summary>
		/// The right ordinate axis.
		/// </summary>
		Axis YAxis2 { get; set; }

		/// <summary>
		/// Gets an array list containing all drawables currently added to the PlotSurface.
		/// </summary>
		ArrayList Drawables { get; }

	}
}
