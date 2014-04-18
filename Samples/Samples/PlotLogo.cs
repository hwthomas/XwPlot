//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// PlotLogo.cs
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
	public class PlotLogo : PlotSample
	{
		public PlotLogo ()
		{
			infoText = "";
			infoText += "ABC (logo for australian broadcasting commission) Example. Demonstrates - \n";
			infoText += " * How to set the background of a plotCanvas as an image. \n";
			infoText += " * EqualAspectRatio axis constraint \n";
			//infoText += " * Plot Zoom with Mouse Wheel, and mouse position Focus point";
		   
			plotCanvas.Clear();
			const int size = 200;
			double [] xs = new double [size];
			double [] ys = new double [size];
			for (int i=0; i<size; i++) {
				xs[i] = Math.Sin ((double)i/(size-1)*2.0*Math.PI);
				ys[i] = Math.Cos ((double)i/(size-1)*6.0*Math.PI);
			}

			LinePlot lp = new LinePlot ();
			lp.OrdinateData = ys;
			lp.AbscissaData = xs;
			lp.LineColor = Colors.Yellow;
			lp.LineWidth = 2;
			plotCanvas.Add (lp);
			plotCanvas.Title = "AxisConstraint.EqualScaling in action...";

			// Image downloaded from http://squidfingers.com. Thanks!
			Assembly asm = Assembly.GetExecutingAssembly ();
			Stream file = asm.GetManifestResourceStream ("Samples.Resources.LogoBackground.jpg" );

			Image im = Image.FromStream (file);
			plotCanvas.PlotBackImage = im.ToBitmap ();
			//plotCanvas.PlotBackColor = Colors.LightGreen;

			//plotCanvas.AddInteraction (new PlotZoom());
			//plotCanvas.AddInteraction (new KeyActions());
			plotCanvas.AddAxesConstraint (new AxesConstraint.AspectRatio (1.0, XAxisPosition.Top, YAxisPosition.Left) );
			
			plotCanvas.XAxis1.WorldMin = plotCanvas.YAxis1.WorldMin;
			plotCanvas.XAxis1.WorldMax = plotCanvas.YAxis1.WorldMax;

			PackStart (plotCanvas.Canvas, true);
			Label la = new Label (infoText);
			PackStart (la);
		}
	}
}

