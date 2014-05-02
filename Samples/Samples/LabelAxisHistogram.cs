using System;
using System.IO;
using System.Reflection;

using Xwt;
using Xwt.Drawing;
using XwPlot;

namespace Samples
{
	public class LabelAxisHistogram : PlotSample
	{
		public LabelAxisHistogram ()
		{
			infoText = "";
			infoText += "Internet Usage Example. Demonstrates - \n";
			infoText += " * Label Axis with angled text. \n";
			infoText += " * ColorGradient Bars fill";
			
			plotCanvas.Clear();

			Grid myGrid = new Grid();
			myGrid.VerticalGridType = Grid.GridType.Coarse;
			double[] pattern = {1.0, 2.0};
			myGrid.MajorGridDash = pattern;

			myGrid.MajorGridColor = Colors.LightGray;
			plotCanvas.Add (myGrid);

			// set up Histogram dataSets manually
			double[] xs = {20.0, 31.0, 27.0, 38.0, 24.0, 3.0, 2.0};
			double[] xs2 = {7.0, 10.0, 42.0, 9.0, 2.0, 79.0, 70.0};
			double[] xs3 = {1.0, 20.0, 20.0, 25.0, 10.0, 30.0, 30.0};

			HistogramPlot hp1 = new HistogramPlot();
			hp1.DataSource = xs;
			hp1.BaseWidth = 0.6;
			hp1.FillGradient = new ColorGradient (Colors.DarkGray, Colors.White);
			hp1.Filled = true;
			hp1.Label = "Developer Work";
			
			HistogramPlot hp2 = new HistogramPlot();
			hp2.DataSource = xs2;
			hp2.Label = "Web Browsing";
			hp2.FillGradient = new ColorGradient (Colors.LightGreen, Colors.White);
			hp2.Filled = true;
			hp2.StackedTo (hp1);
			
			HistogramPlot hp3 = new HistogramPlot();
			hp3.DataSource = xs3;
			hp3.Label = "P2P Downloads";
			hp2.FillGradient = new ColorGradient (Colors.LightBlue, Colors.White);
			hp3.Filled = true;
			hp3.StackedTo (hp2);
			
			plotCanvas.Add (hp1);
			plotCanvas.Add (hp2);
			plotCanvas.Add (hp3);
			
			plotCanvas.Legend = new Legend();

			LabelAxis la = new LabelAxis (plotCanvas.XAxis1);
			la.AddLabel ("Monday", 0.0);
			la.AddLabel ("Tuesday", 1.0);
			la.AddLabel ("Wednesday", 2.0);
			la.AddLabel ("Thursday", 3.0);
			la.AddLabel ("Friday", 4.0);
			la.AddLabel ("Saturday", 5.0);
			la.AddLabel ("Sunday", 6.0);
			la.Label = "Days";
			la.TickTextFont = Font.FromName ("Courier New").WithSize (8);
			la.TicksBetweenText = true;

			plotCanvas.XAxis1 = la;
			plotCanvas.YAxis1.WorldMin = 0.0;
			plotCanvas.YAxis1.Label = "MBytes";
			((LinearAxis)plotCanvas.YAxis1).NumberOfSmallTicks = 1;

			plotCanvas.Title = "Internet useage for user:\n johnc 09/01/03 - 09/07/03";

			plotCanvas.XAxis1.TickTextAngle = 30.0;

			PackStart (plotCanvas.Canvas, true);
			Label l = new Label (infoText);
			PackStart (l);
		}
	}
}

