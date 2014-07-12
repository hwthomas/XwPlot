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
		Widget currentWidget;
		Interaction currentInteraction;

		DataField<string> nameCol = new DataField<string> ();
		DataField<Sample> sampleCol = new DataField<Sample> ();
		DataField<Image> iconCol = new DataField<Image> ();

		TreePosition interactionCategory;
		TreePosition plotCategory;
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
			
			store = new TreeStore (nameCol, iconCol, sampleCol);
			samplesTree = new TreeView ();
			samplesTree.Columns.Add ("Name", iconCol, nameCol);
			
			plotCategory = AddSample (null, "Sample Plots", null);
			interactionCategory = AddSample (null, "Interactions", null);
			testCategory = AddSample (null, "Tests", null);

			AddSample (plotCategory, "Plot Markers", typeof (PlotMarkerSample));
			AddSample (plotCategory, "Waveform Step Plot", typeof (StepPlotSample));
			AddSample (plotCategory, "Point Plot", typeof (PointPlotSample));
			AddSample (plotCategory, "LabelPoint Plot", typeof (LabelPointPlotSample));
			AddSample (plotCategory, "GradientPlot", typeof (GradientPlotSample));
			AddSample (plotCategory, "Histogram Plot", typeof (HistogramSample));
			AddSample (plotCategory, "Stacked Histogram Plot", typeof (StackedHistogram));
			AddSample (plotCategory, "Candle Plot", typeof (CandlePlotSample));
			AddSample (plotCategory, "Trading Plot", typeof (TradingSample));
			AddSample (plotCategory, "Plot Particles", typeof (PlotParticles));
			AddSample (plotCategory, "Plot Logo", typeof (PlotLogo));

			AddInteraction (interactionCategory, "AxisDrag", new AxisDrag ());
			AddInteraction (interactionCategory, "AxisScale", new AxisScale ());
			AddInteraction (interactionCategory, "PlotDrag (horizontal)", new PlotDrag (true,false));
			AddInteraction (interactionCategory, "PlotDrag (vertical)", new PlotDrag (false, true));
			AddInteraction (interactionCategory, "PlotScale", new PlotScale (true, true));
			AddInteraction (interactionCategory, "PlotZoom", new PlotZoom ());
			AddInteraction (interactionCategory, "PlotSelection", new PlotSelection ());

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

				// Remove currentInteraction if there is one
				if (currentInteraction != null) {
					// must already have a valid plot sample with the interaction added to it
					PlotSample ps = (PlotSample)currentWidget;
					PlotCanvas pc = ps.PlotCanvas;
					// Remove current interaction from PlotCanvas
					pc.RemoveInteraction (currentInteraction);
					currentInteraction = null;
				}

				// get newSample from selected row
				TreePosition viewRow = samplesTree.SelectedRow;
				TreeNavigator storeRow = store.GetNavigatorAt (viewRow);
				Sample newSample = storeRow.GetValue (sampleCol);

				TreePosition newCategory = newSample.Category;
				if (newCategory == interactionCategory) {
					// only allow interaction if there is already a plotSample
					if (currentCategory == plotCategory) {
						PlotSample ps = (PlotSample)currentWidget;
						PlotCanvas pc = ps.PlotCanvas;
						// Add new interaction to existing PlotCanvas
						currentInteraction = newSample.Interaction;
						pc.AddInteraction (currentInteraction);
					}

				} else {
					// plotCategory or testCategory
					currentCategory = newCategory;
					if (currentWidget != null) {
						sampleBox.Remove (currentWidget);
					}
					if (newSample.Type != null) {
						currentWidget = (Widget)Activator.CreateInstance (newSample.Type);
						sampleBox.PackStart (currentWidget, true);
						Dump (currentWidget, 0);
					}
				}
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
			Sample sample = new Sample (category, name, sampleType);

			TreeNavigator node = store.AddNode (category);
			TreeNavigator nameNavigator = node.SetValue (nameCol, name);
			TreeNavigator iconNavigator = nameNavigator.SetValue (iconCol, icon);
			TreeNavigator sampleNavigator = iconNavigator.SetValue (sampleCol, sample);
			return sampleNavigator.CurrentPosition;
		}

		TreePosition AddInteraction (TreePosition category, string name, Interaction interaction)
		{
			Sample sample = new Sample (category, name, interaction);

			TreeNavigator node = store.AddNode (category);
			TreeNavigator nameNavigator = node.SetValue (nameCol, name);
			TreeNavigator iconNavigator = nameNavigator.SetValue (iconCol, icon);
			TreeNavigator sampleNavigator = iconNavigator.SetValue (sampleCol, sample);
			return sampleNavigator.CurrentPosition;
		}

	}
	
	class Sample
	{
		public Sample (TreePosition category, string name, Type type)
		{
			Category = category;
			Name = name;
			Type = type;
		}

		public Sample (TreePosition category, string name, Interaction interaction)
		{
			Category = category;
			Name = name;
			Interaction = interaction;
		}

		public TreePosition Category;
		public string Name;
		public Type Type;						// for Plot samples and tests only
		public Interaction Interaction = null;	// for Plot interactions only

	}
}

