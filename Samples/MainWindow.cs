using System;
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class MainWindow: Window
	{
		TreeView samplesTree;
		TreeStore store;
		Image icon;
		VBox sampleBox;
		Widget currentSample;
		
		DataField<string> nameCol = new DataField<string> ();
		DataField<Sample> widgetCol = new DataField<Sample> ();
		DataField<Image> iconCol = new DataField<Image> ();
		
		public MainWindow ()
		{
			Title = "XwPlot Samples Demonstration";
			Width = 800;
			Height = 600;

			Menu menu = new Menu ();
			
			var file = new MenuItem ("File");
			file.SubMenu = new Menu ();
			file.SubMenu.Items.Add (new MenuItem ("Open"));
			file.SubMenu.Items.Add (new MenuItem ("New"));
			MenuItem mi = new MenuItem ("Close");
			mi.Clicked += delegate {
				Application.Exit();
			};
			file.SubMenu.Items.Add (mi);
			menu.Items.Add (file);
			
			var edit = new MenuItem ("Edit");
			edit.SubMenu = new Menu ();
			edit.SubMenu.Items.Add (new MenuItem ("Copy"));
			edit.SubMenu.Items.Add (new MenuItem ("Cut"));
			edit.SubMenu.Items.Add (new MenuItem ("Paste"));
			menu.Items.Add (edit);
			
			MainMenu = menu;
			
			HPaned box = new HPaned ();
			
			icon = Image.FromResource (typeof(App), "class.png");
			
			store = new TreeStore (nameCol, iconCol, widgetCol);
			samplesTree = new TreeView ();
			samplesTree.Columns.Add ("Name", iconCol, nameCol);
			
			var staticPlots = AddSample (null, "Static Plots", null);
			AddSample (staticPlots, "Plot Markers", typeof (PlotMarkerSample));
			AddSample (staticPlots, "Waveform Step Plot", typeof (StepPlotSample));
			AddSample (staticPlots, "Point Plot", typeof (PointPlotSample));
			AddSample (staticPlots, "LabelPoint Plot", typeof (LabelPointPlotSample));
			AddSample (staticPlots, "GradientPlot", typeof (GradientPlotSample));
			AddSample (staticPlots, "Histogram Plot", typeof (HistogramSample));
			AddSample (staticPlots, "Stacked Histogram Plot", typeof (StackedHistogram));
			AddSample (staticPlots, "Candle Plot", typeof (CandlePlotSample));
			AddSample (staticPlots, "Trading Plot", typeof (TradingSample));
			AddSample (staticPlots, "Plot Particles", typeof (PlotParticles));
			AddSample (staticPlots, "Plot Logo", typeof (PlotLogo));

			var interactivePlots = AddSample (null, "Interactions", null);
			AddSample (interactivePlots, "KeyActions", typeof (StepPlotSample));

			var tests = AddSample (null, "Tests", null);
			AddSample (tests, "Linear Axis", typeof (LinearAxisTest));
			AddSample (tests, "Log Axis", typeof (LogAxisTest));
			AddSample (tests, "DateTime Axis", typeof (DateTimeAxisTest));
			AddSample (tests, "TradingDateTime Axis", typeof (TradingDateTimeAxisTest));
			AddSample (tests, "Rendering Performance", typeof (RenderingTest));
			AddSample (tests, "Overlay Canvas Test", typeof (OverlayTest));

			samplesTree.DataSource = store;
			
			box.Panel1.Content = samplesTree;
			
			sampleBox = new VBox ();

			box.Panel2.Content = sampleBox;
			box.Panel2.Resize = true;
			box.Position = 150;
			
			Content = box;
			
			samplesTree.SelectionChanged += HandleSamplesTreeSelectionChanged;

			CloseRequested += HandleCloseRequested;
		}

		void HandleCloseRequested (object sender, CloseRequestedEventArgs args)
		{
			bool allowClose = MessageDialog.Confirm ("Samples will be closed", Command.Ok);
			args.AllowClose = allowClose;
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		void HandleSamplesTreeSelectionChanged (object sender, EventArgs e)
		{
			if (samplesTree.SelectedRow != null) {
				if (currentSample != null) {
					sampleBox.Remove (currentSample);	// possibly shutdown sample here?
				}
				Sample s = store.GetNavigatorAt (samplesTree.SelectedRow).GetValue (widgetCol);
				if (s.Type != null) {
					if (s.Widget == null) {
						s.Widget = (Widget)Activator.CreateInstance (s.Type);
					}
					var sample = s.Widget;	// possibly extract plotCanvas, etc? for Interactions?
					sampleBox.PackStart (sample, true);
				}
				
//				string txt = System.Xaml.XamlServices.Save (s);
				currentSample = s.Widget;
				Dump (currentSample, 0);
			}
		}
		
		void Dump (IWidgetSurface w, int ind)
		{
			if (w == null)
				return;
			var s = w.GetPreferredSize ();
			Console.WriteLine (new string (' ', ind * 2) + " " + w.GetType ().Name + " " + s.Width + " " + s.Height);
			foreach (var c in w.Children)
				Dump (c, ind + 1);
		}
		
		TreePosition AddSample (TreePosition pos, string name, Type sampleType)
		{
			//if (page != null)
			//	page.Margin.SetAll (5);
			return store.AddNode (pos).SetValue (nameCol, name).SetValue (iconCol, icon).SetValue (widgetCol, new Sample (sampleType)).CurrentPosition;
		}
	}
	
	class Sample
	{
		public Sample (Type type)
		{
			Type = type;
		}

		public Type Type;
		public Widget Widget;
	}
}

