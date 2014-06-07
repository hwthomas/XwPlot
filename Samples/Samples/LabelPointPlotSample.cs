//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// LabelPointPlotSample.cs
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
using System.IO;
using System.Reflection;

using Xwt;
using Xwt.Drawing;
using XwPlot;

namespace Samples
{
	public class LabelPointPlotSample : PlotSample
	{
		private bool qeExampleTimerEnabled;
		private double[] PlotQEExampleValues;
		private string[] PlotQEExampleTextValues;

		public LabelPointPlotSample ()
		{
			infoText = "";
			infoText += "Cs2Te Photocathode QE evolution Example. Demonstrates - \n";
			infoText += "  * LabelPointPlot (allows text to be associated with points) \n";
			infoText += "  * PointPlot droplines \n";
			infoText += "  * LabelAxis \n";
			infoText += "  * PhysicalSpacingMin property of LabelAxis \n";

			qeExampleTimerEnabled = true;
			plotCanvas.Clear ();
			
			int len = 24;
			string[] s = new string[len];
			PlotQEExampleValues = new double[len];
			PlotQEExampleTextValues = new string[len];

			Random r = new Random ();

			for (int i=0; i<len;i++) {
				PlotQEExampleValues[i] = 8.0 + 12.0 * (double)r.Next(10000) / 10000.0;
				if (PlotQEExampleValues[i] > 18.0) {
					PlotQEExampleTextValues[i] = "KCsTe";
				}
				else {
					PlotQEExampleTextValues[i] = "";
				}
				s[i] = i.ToString("00") + ".1";
			}

			PointPlot pp = new PointPlot ();
			pp.DataSource = PlotQEExampleValues;
			pp.Marker = new Marker (Marker.MarkerType.Square, 10);
			pp.Marker.DropLine = true;
			pp.Marker.LineColor = Colors.CornflowerBlue;
			pp.Marker.Filled = false;
			plotCanvas.Add (pp);

			LabelPointPlot tp1 = new LabelPointPlot ();
			tp1.DataSource = PlotQEExampleValues;
			tp1.TextData = PlotQEExampleTextValues;
			tp1.LabelTextPosition = LabelPointPlot.LabelPositions.Above;
			tp1.Marker = new Marker (Marker.MarkerType.None, 10);
			plotCanvas.Add (tp1);

			LabelAxis la = new LabelAxis (plotCanvas.XAxis1);
			for (int i=0; i<len; ++i) {
				la.AddLabel (s[i], i);
			}
			Font ff = Font.FromName ("Verdana");
			la.TickTextFont = ff.WithSize (7);
			la.PhysicalSpacingMin = 25;
			plotCanvas.XAxis1 = la;

			plotCanvas.Title = "Cs2Te Photocathode QE evolution";
			plotCanvas.TitleFont = ff.WithSize (15);
			plotCanvas.XAxis1.WorldMin = -1.0;
			plotCanvas.XAxis1.WorldMax = len;
			plotCanvas.XAxis1.LabelFont = ff.WithSize (10);
			plotCanvas.XAxis1.Label = "Cathode ID";
			plotCanvas.YAxis1.Label = "QE [%]";
			plotCanvas.YAxis1.LabelFont = ff.WithSize (10);
			plotCanvas.YAxis1.TickTextFont = ff.WithSize (10);

			plotCanvas.YAxis1.WorldMin = 0.0;
			plotCanvas.YAxis1.WorldMax= 25.0;
			plotCanvas.XAxis1.TickTextAngle = 60.0;

			// Add timer into Xwt loop for data updates
			Application.TimeoutInvoke (750, qeExampleTimer_Tick);

			PackStart (plotCanvas.Canvas, true);
			Label info = new Label (infoText);
			PackStart (info);
						
		}

		protected override void Shutdown ()
		{
			// need to call this from somewhere
			qeExampleTimerEnabled = false;
		}

		/// <summary>
		/// Callback for QE example timer tick.
		/// </summary>
		private bool qeExampleTimer_Tick()
		{
			if (!qeExampleTimerEnabled)
				return false;
			
			Random r = new Random ();

			for (int i=0; i<PlotQEExampleValues.Length; ++i) {
				PlotQEExampleValues[i] = 8.0 + 12.0 * (double)r.Next(10000) / 10000.0;
				if ( PlotQEExampleValues[i] > 18.0 ) {
					PlotQEExampleTextValues[i] = "KCsTe";
				}
				else {
					PlotQEExampleTextValues[i] = "";
				}
			}
			plotCanvas.Refresh ();
			//returning true means that the timeout routine should be invoked
			//again after the timeout period expires.  Returning false will 
			//terminate the timeout ie when it has been disabled.
			return qeExampleTimerEnabled;
		}

	}
}

