using System;
using Xwt;
using Xwt.Drawing;


namespace XwPlot
{
	/// <summary>
	/// PlotDrag allows Plot to be dragged without rescaling in both X and Y
	/// </summary>
	public class PlotDrag : Interaction
	{
		bool vertical = true;
		bool horizontal = true;
		Point lastPoint = new Point (-1, -1);
		Point unset = new Point (-1, -1);
		private bool dragInitiated = false;

		double focusX = 0.5, focusY = 0.5;

		Key key; 
		ModifierKeys modifiers;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="horizontal">enable horizontal drag/param>
		/// <param name="vertical">enable vertical drag/param>
		public PlotDrag (bool horizontal, bool vertical)
		{
			Vertical = vertical;
			Horizontal = horizontal;
			Sensitivity = 2.0;
		}

		/// <summary>
		/// Horizontal Drag enable/disable
		/// </summary>
		public bool Horizontal { get; set; }

		/// <summary>
		/// Vertical Drag enable/disable
		/// </summary>
		public bool Vertical { get; set; }

		/// <summary>
		/// Sensitivity factor for axis scaling
		/// </summary>
		public double Sensitivity { get; set; }


		public override bool OnButtonPressed (ButtonEventArgs args, PlotCanvas pc)
		{
			// Only start drag if mouse is inside plot area (excluding axes)
			Rectangle area = pc.PlotAreaBoundingBoxCache;
			if (area.Contains (args.Position)) {
				dragInitiated = true;
				lastPoint = new Point (args.X, args.Y);
				if (args.Button == PointerButton.Left) {				// Drag
					if (horizontal || vertical) {
						//pc.plotCursor = CursorType.Hand;
					}
					if (((modifiers & ModifierKeys.Control) != 0)) {	// Zoom
						if (horizontal) {
							;//pc.plotCursor = CursorType.LeftRight;
						}
						if (vertical) {
							;//pc.plotCursor = CursorType.UpDown;
						}
						if (horizontal && vertical) {
							;//pc.plotCursor = CursorType.Zoom;
						}
					}
				}
				// evaluate focusPoint about which axis is expanded
				focusX = (double)(args.X - area.Left)/(double)area.Width;
				focusY = (double)(area.Bottom - args.Y)/(double)area.Height;
			}
			return false;
		}

		public override bool OnButtonReleased (ButtonEventArgs args, PlotCanvas pc)
		{
			if (dragInitiated) {
				lastPoint = unset;
				dragInitiated = false;
				//pc.plotCursor = CursorType.LeftPointer;
			}
			return false;
		}

		public override bool OnMouseMoved (MouseMovedEventArgs args, PlotCanvas pc)
		{
			Rectangle area = pc.PlotAreaBoundingBoxCache;

			// Mouse Left-Button gives Plot Drag, Ctrl.Left-Button Zooms
			if (dragInitiated) {
				pc.CacheAxes();

				double dX = args.X - lastPoint.X;		// distance mouse has moved
				double dY = args.Y - lastPoint.Y;
				lastPoint = new Point (args.X, args.Y);

				if ((modifiers & ModifierKeys.Control) != 0) {
					// Axis re-ranging required - Alt key reduces sensitivity
					double factor = Sensitivity;
					if ((modifiers & ModifierKeys.Alt) != 0) {
						factor *= 0.25;	   // arbitrary change 
					}
					double xProportion = +dX*factor/area.Width;
					double yProportion = -dY*factor/area.Height;
						
					if (horizontal) {
						pc.ZoomXAxes (xProportion, focusX);
					}
					if (vertical) {
						pc.ZoomYAxes (yProportion, focusY);
					}
				}
				else {
					// Axis translation required
					double xShift = -dX / area.Width;
					double yShift = +dY / area.Height;

					if (horizontal) {
						pc.TranslateXAxes (xShift);
					}
					if (vertical) {
						pc.TranslateYAxes (yShift);
					}
				}
				return true;
			}
			return false;
		}

		public override bool OnKeyPressed (KeyEventArgs args, PlotCanvas pc)
		{
			key = args.Key;
			modifiers = args.Modifiers;
			return false;
		}

		public override bool OnKeyReleased (KeyEventArgs args, PlotCanvas pc)
		{
			key = args.Key;
			modifiers = args.Modifiers;
			return false;
		}

	} // PlotDrag/Zoom
	
}