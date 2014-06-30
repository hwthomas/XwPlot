using System;
using Xwt;
using Xwt.Drawing;
using XwPlot;

namespace Samples
{
	public class MainWindow: Window
	{
		TreeView samplesTree;
		TreeStore store;
		Image icon;
		VBox sampleBox;
		TreePosition currentCategory;
		Widget currentSample;
		Interaction currentInteraction;

		DataField<string> nameCol = new DataField<string> ();
		DataField<Sample> widgetCol = new DataField<Sample> ();
		DataField<Image> iconCol = new DataField<Image> ();

		TreePosition interactionCategory;
		TreePosition sampleCategory;
		TreePosition testCategory;
		
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
			
			sampleCategory = AddSample (null, "Sample Plots", null);
			interactionCategory = AddSample (null, "Interactions", null);
			testCategory = AddSample (null, "Tests", null);

			AddSample (sampleCategory, "Plot Markers", typeof (PlotMarkerSample));
			AddSample (sampleCategory, "Waveform Step Plot", typeof (StepPlotSample));
			AddSample (sampleCategory, "Point Plot", typeof (PointPlotSample));
			AddSample (sampleCategory, "LabelPoint Plot", typeof (LabelPointPlotSample));
			AddSample (sampleCategory, "GradientPlot", typeof (GradientPlotSample));
			AddSample (sampleCategory, "Histogram Plot", typeof (HistogramSample));
			AddSample (sampleCategory, "Stacked Histogram Plot", typeof (StackedHistogram));
			AddSample (sampleCategory, "Candle Plot", typeof (CandlePlotSample));
			AddSample (sampleCategory, "Trading Plot", typeof (TradingSample));
			AddSample (sampleCategory, "Plot Particles", typeof (PlotParticles));
			AddSample (sampleCategory, "Plot Logo", typeof (PlotLogo));

			AddInteraction (interactionCategory, "PlotDrag", new PlotDrag (true,false));

			AddSample (testCategory, "Linear Axis", typeof (LinearAxisTest));
			AddSample (testCategory, "Log Axis", typeof (LogAxisTest));
			AddSample (testCategory, "DateTime Axis", typeof (DateTimeAxisTest));
			AddSample (testCategory, "TradingDateTime Axis", typeof (TradingDateTimeAxisTest));
			AddSample (testCategory, "Rendering Performance", typeof (RenderingTest));
			AddSample (testCategory, "Overlay Canvas Test", typeof (OverlayTest));

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
					// cleanup if required - how?
					sampleBox.Remove (currentSample);
				}
				TreePosition selectedRow = samplesTree.SelectedRow;
				TreeNavigator navigator = store.GetNavigatorAt (selectedRow);
				Sample s = navigator.GetValue (widgetCol);
				TreePosition category = s.Category;
				if (category == interactionCategory) {
					if (currentCategory == sampleCategory) {
						int n = 1;	// can now add/remove interactions from currentSample
					}
				}
				System.Type currentType = s.Type;
				if (currentType != null) {
					if (s.Widget == null) {
						s.Widget = (Widget)Activator.CreateInstance (currentType);
					}
					sampleBox.PackStart (s.Widget, true);
				}
				currentSample = s.Widget;
				currentCategory = s.Category;
//				string txt = System.Xaml.XamlServices.Save (s);
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
		
		TreePosition AddSample (TreePosition category, string name, Type sampleType)
		{
			Sample sample = new Sample (category, sampleType);

			TreeNavigator node = store.AddNode (category);
			TreeNavigator nameNavigator = node.SetValue (nameCol, name);
			TreeNavigator iconNavigator = nameNavigator.SetValue (iconCol, icon);
			TreeNavigator sampleNavigator = iconNavigator.SetValue (widgetCol, sample);
			TreePosition pos = sampleNavigator.CurrentPosition;

			return pos;

		}

		TreePosition AddInteraction (TreePosition category, string name, Interaction interaction)
		{
			Sample sample = new Sample (category, interaction);

			TreeNavigator node = store.AddNode (category);
			TreeNavigator nameNavigator = node.SetValue (nameCol, name);
			TreeNavigator iconNavigator = nameNavigator.SetValue (iconCol, icon);
			TreeNavigator sampleNavigator = iconNavigator.SetValue (widgetCol, sample);
			TreePosition pos = sampleNavigator.CurrentPosition;

			return pos;

		}

	}
	
	class Sample
	{
		public Sample (TreePosition category, Type type)
		{
			Category = category;
			Type = type;	// for Plot samples and tests
		}

		public Sample (TreePosition category, Interaction interaction)
		{
			Category = category;
			Interaction = interaction;	// for Plot interactions
		}

		public TreePosition Category;
		public Type Type;
		public Widget Widget = null;
		public Interaction Interaction = null;

	}
}

