using System;
using System.Drawing;

namespace NPlot
{

	/// <summary>
	/// Allows Plot to be dragged without rescaling in both X and Y
	/// </summary>
	public class PlotDrag : Interaction
	{
		private bool vertical_ = true;
		private bool horizontal_ = true;
		private bool dragInitiated_ = false;
		private Point lastPoint_ = new Point(-1, -1);
		private Point unset_ = new Point(-1, -1);
		private double focusX = 0.5, focusY = 0.5;
		private float sensitivity_ = 2.0f;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="horizontal">enable horizontal drag/param>
		/// <param name="vertical">enable vertical drag/param>
		public PlotDrag(bool horizontal, bool vertical)
		{
			Vertical = vertical;
			Horizontal = horizontal;
		}

		/// <summary>
		/// Horizontal Drag enable/disable
		/// </summary>
		public bool Horizontal
		{
			get { return horizontal_; }
			set { horizontal_ = value; }
		}

		/// <summary>
		/// Vertical Drag enable/disable
		/// </summary>
		public bool Vertical
		{
			get { return vertical_; }
			set { vertical_ = value; }
		}

		/// <summary>
		/// MouseDown method for PlotDrag interaction
		/// </summary>
		/// <param name="X">mouse X position</param>
		/// <param name="Y"> mouse Y position</param>
		/// <param name="keys"> mouse and keyboard modifiers</param>
		/// <param name="ps">the InteractivePlotSurface2D</param>
		public override bool DoMouseDown (int X, int Y, Modifier keys, InteractivePlotSurface2D ps)
		{
			// Only start drag if mouse is inside plot area (excluding axes)
			Rectangle area = ps.PlotAreaBoundingBoxCache;
			if (area.Contains(X,Y)) {
				dragInitiated_ = true;
				lastPoint_ = new Point(X,Y);
				if (((keys & Modifier.Button1) != 0)) {		   // Drag
					if (horizontal_ || vertical_) {
						ps.plotCursor = CursorType.Hand;
					}
					if (((keys & Modifier.Control) != 0)) {	   // Zoom
						if (horizontal_)
							ps.plotCursor = CursorType.LeftRight;
						if (vertical_)
							ps.plotCursor = CursorType.UpDown;
						if (horizontal_ && vertical_)
							ps.plotCursor = CursorType.Zoom;
					}
				}
				// evaluate focusPoint about which axis is expanded
				focusX = (double)(X - area.Left)/(double)area.Width;
				focusY = (double)(area.Bottom - Y)/(double)area.Height;
			}
			return false;
		}


		/// <summary>
		/// MouseMove method for PlotDrag interaction
		/// </summary>
		/// <param name="X">mouse X position</param>
		/// <param name="Y"> mouse Y position</param>
		/// <param name="keys"> mouse and keyboard modifiers</param>
		/// <param name="ps">the InteractivePlotSurface2D</param>
		public override bool DoMouseMove (int X, int Y, Modifier keys, InteractivePlotSurface2D ps)
		{
			Rectangle area = ps.PlotAreaBoundingBoxCache;

			// Mouse Left-Button gives Plot Drag, Ctrl.Left-Button Zooms
			if (((keys & Modifier.Button1) != 0) && dragInitiated_) {
				ps.CacheAxes();

				double dX = X - lastPoint_.X;		// distance mouse has moved
				double dY = Y - lastPoint_.Y;
				lastPoint_ = new Point(X, Y);

				if ((keys & Modifier.Control) != 0) {
					// Axis re-ranging required
					double factor = Sensitivity;
					if ((keys & Modifier.Alt) != 0) {
						factor *= 0.25;	   // arbitrary change 
					}
					double xProportion = +dX*factor/area.Width;
					double yProportion = -dY*factor/area.Height;
						
					if (horizontal_) {
						ps.ZoomXAxes (xProportion, focusX);
					}
					if (vertical_) {
						ps.ZoomYAxes (yProportion, focusY);
					}
				}
				else {
					// Axis translation required
					double xShift = -dX / area.Width;
					double yShift = +dY / area.Height;

					if (horizontal_) {
						ps.TranslateXAxes (xShift);
					}
					if (vertical_) {
						ps.TranslateYAxes (yShift);
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// MouseUp method for PlotDrag interaction
		/// </summary>
		/// <param name="X">mouse X position</param>
		/// <param name="Y"> mouse Y position</param>
		/// <param name="keys"> mouse and keyboard modifiers</param>
		/// <param name="ps">the InteractivePlotSurface2D</param>
		public override bool DoMouseUp(int X, int Y, Modifier keys, InteractivePlotSurface2D ps)
		{
			if (dragInitiated_) {
				lastPoint_ = unset_;
				dragInitiated_ = false;
				ps.plotCursor = CursorType.LeftPointer;
			}
			return false;
		}
			
		/// <summary>
		/// Sensitivity factor for axis scaling
		/// </summary>
		/// <value></value>
		public float Sensitivity
		{
			get { return sensitivity_; }
			set { sensitivity_ = value; }
		}
		 
	} // PlotDrag/Zoom
	
}