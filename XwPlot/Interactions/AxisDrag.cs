using System;
using System.Collections;

using Xwt;
using Xwt.Drawing;

namespace XwPlot
{

	/// <summary>
	/// Dragging the axes increases or decreases the axes scaling factors
	/// with the expansion being about the point where the drag is started
	/// Only the axis in which the mouse is clicked will be modified
	/// </summary>
	public class AxisDrag : Interaction
	{
		Axis axis = null;
		bool dragging = false;
		PhysicalAxis physicalAxis = null;
		Point lastPoint;
		Point startPoint;
		double focusRatio = 0.5;

		/// <summary>
		/// Default constructor
		/// </summary>
		public AxisDrag ()
		{
			Sensitivity = 1.0;
		}

		/// <summary>
		/// Sensitivity factor for axis scaling
		/// </summary>
		public double Sensitivity { get; set; }

		/// <summary>
		/// OnButtonPressed method for AxisDrag interaction
		/// </summary>
		public override bool OnButtonPressed (ButtonEventArgs args, PlotCanvas pc)
		{
			// if the mouse is inside the plot area (the tick marks may be here,
			// and are counted as part of the axis), then *don't* invoke drag. 
			if (pc.PlotAreaBoundingBoxCache.Contains(args.X, args.Y)) {
				return false;
			}
				
			if (args.Button == PointerButton.Left) {
				// see if hit with axis. NB Only one axis object will be returned
				ArrayList objects = pc.HitTest (new Point(args.X, args.Y));

				foreach (object o in objects) {
					if (o is Axis) {
						dragging = true;
						axis = (Axis)o;
						if (pc.PhysicalXAxis1Cache.Axis == axis) {
							physicalAxis = pc.PhysicalXAxis1Cache;
							//pc.plotCursor = CursorType.LeftRight;
						}
						else if (pc.PhysicalXAxis2Cache.Axis == axis) {
							physicalAxis = pc.PhysicalXAxis2Cache;
							//ps.plotCursor = CursorType.LeftRight;
						}
						else if (pc.PhysicalYAxis1Cache.Axis == axis) {
							physicalAxis = pc.PhysicalYAxis1Cache;
							//pc.plotCursor = CursorType.UpDown;
						}
						else if (pc.PhysicalYAxis2Cache.Axis == axis) {
							physicalAxis = pc.PhysicalYAxis2Cache;
							//pc.plotCursor = CursorType.UpDown;
						}

						startPoint = new Point (args.X, args.Y);
						lastPoint = startPoint;

						// evaluate focusRatio about which axis is expanded
						double  x = startPoint.X - physicalAxis.PhysicalMin.X;
						double  y = startPoint.Y - physicalAxis.PhysicalMin.Y;
						double r = Math.Sqrt(x*x + y*y);
						focusRatio = r/physicalAxis.PhysicalLength;
						return false;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// OnButtonReleased method for AxisDrag interaction
		/// </summary>
		public override bool OnButtonReleased (ButtonEventArgs args, PlotCanvas pc)
		{
			if (dragging) {
				dragging = false;
				axis = null;
				physicalAxis = null;
				lastPoint = new Point();
				//pc.plotCursor = CursorType.LeftPointer;
			}
			return false;
		}

		/// <summary>
		/// OnMouseMoved method for AxisDrag interaction
		/// </summary>
		public override bool OnMouseMoved (MouseMovedEventArgs args, PlotCanvas pc)
		{
			if (dragging && physicalAxis != null) {
				pc.CacheAxes();

				double dX = (args.X - lastPoint.X);
				double dY = (args.Y - lastPoint.Y);
				lastPoint = new Point (args.X, args.Y);

				// In case the physical axis is not horizontal/vertical, combine dX and dY
				// in a way which preserves their sign and intuitive axis zoom sense, ie
				// because the physical origin is top-left, expand with +ve dX, but -ve dY 
				double distance = dX - dY;
				double proportion = distance*Sensitivity /physicalAxis.PhysicalLength;
				axis.IncreaseRange (proportion, focusRatio);
				return true;
			}
			return false;
		}

	} // AxisDrag (Zoom)

}
