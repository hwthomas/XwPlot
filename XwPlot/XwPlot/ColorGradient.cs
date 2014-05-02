//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// ColorGradient.cs
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
	/// Class for creating a simple (linear) color gradient
	/// </summary>
	/// <remarks>
	/// This is used to map plot-values to colors for a GradientPlot, and also
	/// to define color gradients used in PlotBackgrounds and Histogram Plots.
	/// </remarks>
	public class ColorGradient : IGradient
	{
		/// <summary>
		/// Default Constructor with standard default values set up
		/// </summary>
		public ColorGradient ()
		{
			// Default gradient is a vertical color change
			StartPoint = new Point (0, 0);
			StartColor = Colors.LightBlue;
			EndPoint = new Point (0, 1);
			EndColor = Colors.LightGreen;
			VoidColor = Colors.Yellow;
		}

		/// <summary>
		/// Constructor used for proportional color only
		/// </summary>
		/// <param name="startColor">The start color (corresponding to 0.0)</param>
		/// <param name="endColor">The end color (corresponding to 1.0)</param>
		public ColorGradient (Color startColor, Color endColor)
		{
			StartPoint = new Point (0, 0);
			StartColor = startColor;
			EndPoint = new Point (1, 1);
			EndColor = endColor;
			VoidColor = Colors.Yellow;
		}

		/// <summary>
		/// Constructor used for defining a linear color gradient
		/// </summary>
		/// <param name="startPoint">The start point for a color gradient</param>
		/// <param name="startColor">The start color (corresponding to 0.0)</param>
		/// <param name="endPoint">The end point for a color gradient</param>
		/// <param name="endColor">The end color (corresponding to 1.0)</param>
		public ColorGradient (Point startPoint, Color startColor, Point endPoint, Color endColor)
		{
			StartPoint = startPoint;
			StartColor = startColor;
			EndPoint = endPoint;
			EndColor = endColor;
			VoidColor = Colors.Yellow;
		}

		/// <summary>
		/// The color corresponding to 0.0
		/// </summary>
		public Color StartColor { get; set; }

		/// <summary>
		/// The color corresponding to 1.0
		/// </summary>
		public Color EndColor { get; set; }

		/// <summary>
		/// The color corresponding to NaN
		/// </summary>
		public Color VoidColor { get; set; } 

		/// <summary>
		/// The Point on a Unit Square defining the Start of the gradient
		/// </summary>
		/// <remarks>
		/// Start and End Points must be on opposite sides of a unit square, which
		/// only allows simple vertical, horizontal, or diagonal color gradients.
		/// </remarks>
		public Point StartPoint { get; set; }

		/// <summary>
		/// The Point on a Unit Square defining the End of the gradient
		/// </summary>
		/// <remarks>
		/// Start and End Points must be on opposite sides of a unit square, which
		/// only allows simple vertical, horizontal, or diagonal color gradients.
		/// </remarks>
		public Point EndPoint { get; set; }

		/// <summary>
		/// Gets a color corresponding to a number between 0.0 and 1.0 inclusive.
		/// The color will be a linear interpolation of the start and end colors
		///</summary>
		/// <param name="prop">the number between 0.0 and 1.0 to get corresponding color for
		/// </param>
		/// <returns>The color corresponding to the (clipped) supplied number</returns>
		public Color GetColor (double prop)
		{
			if (Double.IsNaN(prop)) {
				return VoidColor;
			}

			if (prop <= 0.0) {
				return StartColor;
			}

			if (prop >= 1.0) {
				return EndColor;
			}

			double r = StartColor.Red + (EndColor.Red - StartColor.Red)*prop;
			double g = StartColor.Green + (EndColor.Green - StartColor.Green)*prop;
			double b = StartColor.Blue + (EndColor.Blue - StartColor.Blue)*prop;

			return new Color (r,g,b);
		}
	}
}
