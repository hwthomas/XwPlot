//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// PlotParticles.cs
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
using System.IO;
using System.Reflection;

using Xwt;
using Xwt.Drawing;
using XwPlot;

namespace Samples
{
	public class PlotParticles : PlotSample
	{
		public PlotParticles ()
		{
			infoText = "";
			infoText += "Particles Example. Demonstrates - \n";
			infoText += " * How to chart multiple data sets against multiple axes at the same time.";

			plotCanvas.Clear();

			Grid mygrid = new Grid ();
			mygrid.HorizontalGridType = Grid.GridType.Fine;
			mygrid.VerticalGridType = Grid.GridType.Fine;
			plotCanvas.Add (mygrid);

			// in this example we synthetize a particle distribution
			// in the x-x' phase space and plot it, with the rms Twiss
			// ellipse and desnity distribution
			const int Particle_Number = 500;
			double [] x = new double [Particle_Number];
			double [] y = new double [Particle_Number];
			// Twiss parameters for the beam ellipse
			// 5 mm mrad max emittance, 1 mm beta function
			double alpha, beta, gamma, emit;
			alpha = -2.0;
			beta = 1.0;
			gamma = (1.0 + alpha * alpha) / beta;
			emit = 4.0;

			double da, xmax, xpmax;
			da = -alpha / gamma;
			xmax = Math.Sqrt (emit / gamma);
			xpmax = Math.Sqrt (emit * gamma);

			Random rand = new Random ();

			// cheap randomizer on the unit circle
			for (int i = 0; i<Particle_Number; i++) {
				double r;
				do {
					x[i] = 2.0 * rand.NextDouble () - 1.0;
					y[i] = 2.0 * rand.NextDouble () - 1.0;
					r = Math.Sqrt (x[i] * x[i] + y[i] * y[i]);
				} while (r > 1.0);
			}

			// transform to the tilted twiss ellipse
			for (int i =0; i<Particle_Number; ++i) {
				y[i] *= xpmax;
				x[i] = x[i] * xmax + y[i] * da;
			}
			plotCanvas.Title = "Beam Horizontal Phase Space and Twiss ellipse";

			PointPlot pp = new PointPlot ();
			pp.OrdinateData = y;
			pp.AbscissaData = x;
			pp.Marker = new Marker (Marker.MarkerType.FilledCircle ,4, Colors.Blue);
			plotCanvas.Add (pp, XAxisPosition.Bottom, YAxisPosition.Left);

			// set axes
			LinearAxis lx = (LinearAxis) plotCanvas.XAxis1;
			lx.Label = "Position - x [mm]";
			lx.NumberOfSmallTicks = 2;
			LinearAxis ly = (LinearAxis) plotCanvas.YAxis1;
			ly.Label = "Divergence - x' [mrad]";
			ly.NumberOfSmallTicks = 2;
			
			// Draws the rms Twiss ellipse computed from the random data
			double [] xeli = new double [40];
			double [] yeli = new double [40];

			double a_rms, b_rms, g_rms, e_rms;

			Twiss (x, y, out a_rms, out b_rms, out g_rms, out e_rms);
			TwissEllipse (a_rms, b_rms, g_rms, e_rms, ref xeli, ref yeli);

			LinePlot lp = new LinePlot ();
			lp.OrdinateData = yeli;
			lp.AbscissaData = xeli;
			plotCanvas.Add (lp, XAxisPosition.Bottom, YAxisPosition.Left);
			lp.LineColor = Colors.Red;
			lp.LineWidth = 2;
			// Draws the ellipse containing 100% of the particles
			// for a uniform distribution in 2D the area is 4 times the rms
			double [] xeli2 = new double [40];
			double [] yeli2 = new double [40];
			TwissEllipse (a_rms, b_rms, g_rms, 4.0F * e_rms, ref xeli2, ref yeli2);

			LinePlot lp2 = new LinePlot ();
			lp2.OrdinateData = yeli2;
			lp2.AbscissaData = xeli2;
			plotCanvas.Add (lp2, XAxisPosition.Bottom, YAxisPosition.Left);
			double[] pattern = new double[] { 5, 20 };
			lp2.LineDash = pattern;
			lp2.LineColor = Colors.Red;

			// now bin the particle position to create beam density histogram
			double range, min, max;
			min = lx.WorldMin;
			max = lx.WorldMax;
			range = max - min;

			const int Nbin = 30;
			double[] xbin = new double[Nbin+1];
			double[] xh = new double [Nbin+1];

			for (int j=0; j<=Nbin; ++j){
				xbin[j] = min + j * range;
				if (j < Nbin) xh[j] = 0.0;
			}
			for (int i =0; i<Particle_Number; ++i) {
				if (x[i] >= min && x[i] <= max) {
					int j;
					j = Convert.ToInt32(Nbin * (x[i] - min) / range);
					xh[j] += 1;
				}
			}
			StepPlot sp= new StepPlot ();
			sp.OrdinateData = xh;
			sp.AbscissaData = new StartStep( min, range / Nbin );
			sp.Center = true;
			plotCanvas.Add (sp, XAxisPosition.Bottom, YAxisPosition.Right);
			// axis formatting
			LinearAxis ly2 = (LinearAxis)plotCanvas.YAxis2;
			ly2.WorldMin = 0.0f;
			ly2.Label = "Beam Density [a.u.]";
			ly2.NumberOfSmallTicks = 2;
			sp.Color = Colors.Green;

			PackStart (plotCanvas.Canvas, true);
			Label la = new Label (infoText);
			PackStart (la);
		}

		// Fill the array containing the rms twiss ellipse data points
		// ellipse is g*x^2+a*x*y+b*y^2=e
		private void TwissEllipse (double a, double b, double g, double e, ref double [] x, ref double [] y)
		{
			double rot, sr, cr, brot;
			if (a == 0) {
				rot = 0;
			}
			else {
				rot = 0.5 * Math.Atan (2.0 * a / (g - b));
			}
			sr = Math.Sin (rot);
			cr = Math.Cos (rot);
			brot = g * sr * sr - 2.0F * a * sr * cr + b * cr * cr;
			int npt=x.Length;
			double theta;
		
			for (int i=0; i<npt;++i) {
				double xr,yr;
				theta = i * 2.0 * Math.PI / (npt-1);
				xr = Math.Sqrt (e * brot) * Math.Cos (theta);
				yr = Math.Sqrt (e / brot) * Math.Sin (theta);
				x[i] = xr * cr - yr * sr;
				y[i] = xr * sr + yr * cr;
			}
		}

		// Evaluates the rms Twiss parameters from the particle coordinates
		private void Twiss (double [] x, double [] y, out double a, out double b, out double g, out double e)
		{
			double xave, xsqave, yave, ysqave, xyave;
			double sigmaxsq, sigmaysq, sigmaxy;
			int Npoints= x.Length;
			xave = 0;
			yave = 0;
			for (int i=0; i<Npoints; ++i) {
				xave += x[i];
				yave += y[i];
			}
			xave /= Npoints;
			yave /= Npoints;
			xsqave = 0;
			ysqave = 0;
			xyave = 0;
			for (int i=0;i<Npoints;i++) {
				xsqave += x[i] * x[i];
				ysqave += y[i] * y[i];
				xyave += x[i] * y[i];
			}
			xsqave /= Npoints;
			ysqave /= Npoints;
			xyave /= Npoints;
			sigmaxsq = xsqave - xave * xave;
			sigmaysq = ysqave - yave * yave;
			sigmaxy = xyave - xave * yave;
			// Now evaluates rms Twiss parameters
			e = Math.Sqrt (sigmaxsq * sigmaysq - sigmaxy * sigmaxy);
			a = -sigmaxy / e;
			b = sigmaxsq / e;
			g = (1.0 + a * a) / b;
		}

	}
}

