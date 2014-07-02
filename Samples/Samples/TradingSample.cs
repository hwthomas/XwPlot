//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// TradingSample.cs
//
// Derived originally from NPlot (Copyright (C) 2003-2006 Matt Howlett and others)
// Port to Xwt 2012-2014 : Hywel Thomas <hywel.w.thomas@gmail.com>
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
using System.Data;
using System.Collections;
using System.Xml;

using Xwt;
using Xwt.Drawing;
using XwPlot;

namespace Samples
{
	public class TradingSample : PlotSample
	{
		public TradingSample () : base ()
		{
			infoText = "";
			infoText += "Stock Dataset Sample. Demonstrates - \n";
			infoText += " * CandlePlot, FilledRegion, LinePlot and ArrowItem IDrawables \n";
			infoText += " * DateTime axes \n";
			infoText += " * Horizontal Drag Interaction - try dragging the plot surface \n";
			infoText += " * Axis Drag Interaction - try dragging in the horizontal and vertical Axis areas";

			plotCanvas.Clear ();
			// [NOTIMP] plotCanvas.DateTimeToolTip = true;

			// obtain stock information from xml file
			DataSet ds = new DataSet();
			System.IO.Stream file =
			    Assembly.GetExecutingAssembly ().GetManifestResourceStream ("Samples.Resources.asx_jbh.xml");
			ds.ReadXml (file, System.Data.XmlReadMode.ReadSchema);
			DataTable dt = ds.Tables[0];
			// DataView dv = new DataView( dt );

			// create CandlePlot.
			CandlePlot cp = new CandlePlot ();
			cp.DataSource = dt;
			cp.AbscissaData = "Date";
			cp.OpenData = "Open";
			cp.LowData = "Low";
			cp.HighData = "High";
			cp.CloseData = "Close";
			cp.BearishColor = Colors.Red;
			cp.BullishColor = Colors.Green;
			cp.Style = CandlePlot.Styles.Filled;

			// calculate 10 day moving average and 2*sd line
			ArrayList av10 = new ArrayList();
			ArrayList sd2_10 = new ArrayList();
			ArrayList sd_2_10 = new ArrayList();
			ArrayList dates = new ArrayList();
			for (int i=0; i<dt.Rows.Count-10; ++i) {
				float sum = 0.0f;
				for (int j=0; j<10; ++j) {
					sum += (float)dt.Rows[i+j]["Close"];
				}
				float average = sum / 10.0f;
				av10.Add (average);
				sum = 0.0f;
				for (int j=0; j<10; ++j) {
					sum += ((float)dt.Rows[i+j]["Close"]-average)*((float)dt.Rows[i+j]["Close"]-average);
				}
				sum /= 10.0f;
				sum = 2.0f * (float)Math.Sqrt (sum);
				sd2_10.Add (average + sum);
				sd_2_10.Add (average - sum);
				dates.Add ((DateTime)dt.Rows[i+10]["Date"]);
			}

			// and a line plot of close values.
			LinePlot av = new LinePlot ();
			av.OrdinateData = av10;
			av.AbscissaData = dates;
			av.LineColor = Colors.DarkGray;
			av.LineWidth = 2.0;

			LinePlot top = new LinePlot ();
			top.OrdinateData = sd2_10;
			top.AbscissaData = dates;
			top.LineColor = Colors.LightBlue;
			top.LineWidth = 2.0;

			LinePlot bottom = new LinePlot ();
			bottom.OrdinateData = sd_2_10;
			bottom.AbscissaData = dates;
			bottom.LineColor = Colors.LightBlue;
			bottom.LineWidth = 2.0;

			FilledRegion fr = new FilledRegion (top, bottom);
			fr.FillColor = Colors.LightGreen;

			// Note: order of adding FilledRegion, Plots, etc is important for visibility
			plotCanvas.Add (fr);
			plotCanvas.Add (new Grid());

			plotCanvas.Add (av);
			plotCanvas.Add (top);
			plotCanvas.Add (bottom);
			plotCanvas.Add (cp);

			// now make an arrow... 
			ArrowItem arrow = new ArrowItem (new Point (((DateTime)dt.Rows[60]["Date"]).Ticks, 2.28), -80, "An interesting flat bit");
			arrow.ArrowColor = Colors.DarkBlue;
			arrow.PhysicalLength = 50;

			plotCanvas.Add (arrow);

			plotCanvas.Title = "Stock Prices";
			plotCanvas.XAxis1.Label = "Date / Time";
			plotCanvas.XAxis1.WorldMin += plotCanvas.XAxis1.WorldLength / 4.0;
			plotCanvas.XAxis1.WorldMax -= plotCanvas.XAxis1.WorldLength / 2.0;
			plotCanvas.YAxis1.Label = "Price [$]";

			plotCanvas.XAxis1 = new TradingDateTimeAxis (plotCanvas.XAxis1);

			plotCanvas.PlotBackColor = Colors.White;
			plotCanvas.XAxis1.LineColor = Colors.Black;
			plotCanvas.YAxis1.LineColor = Colors.Black;

			PackStart (plotCanvas.Canvas, true);
			Label la = new Label (infoText);
			PackStart (la);
		}
	}
}

