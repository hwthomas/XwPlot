//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// KeyActions.cs
// 
// Copyright (C) 2013 Hywel Thomas <hywel.w.thomas@gmail.com>
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
using Xwt;

namespace XwPlot
{

	/// <summary>
	/// Links some of the standard keyboard keys to plot scrolling and zooming.
	/// Since all key-interactions are applied to the complete PlotSurface, any
	/// translation or zooming is applied to all axes that have been defined
	/// 
	/// The following key actions are currently implemented :-
	/// Left	- scrolls the viewport to the left
	/// Right	- scrolls the viewport to the right
	/// Up		- scrolls the viewport up
	/// Down	- scrolls the viewport down
	/// +		- zooms in
	/// -		- zooms out
	/// Alt		- reduces the effect of the above actions
	/// Home	- restores original view and dimensions
	/// </summary>
	public class KeyActions : Interaction
	{
		const double left		= -0.25;
		const double right		= +0.25;
		const double up			= +0.25;
		const double down		= -0.25;
		const double zoomIn		= -0.50;	// Should give reversible
		const double zoomOut 	= +1.00;	// ZoomIn / ZoomOut actions

		const double symmetrical	= 0.5;
		const double altFactor		= 0.4;	// Alt key reduces sensitivity

		public KeyActions () : base ()
		{
			Sensitivity = 1.0;
		}

		/// <summary>
		/// Sensitivity factor for axis zoom
		/// </summary>
		public double Sensitivity { get; set; }
	
		/// <summary>
		/// Handler for KeyPressed events
		/// </summary>
		/// <param name="args">the Xwt.KeyEventArgs</param>
		/// <param name="pc">the PlotCanvas</param>
		/// <returns>
		/// true if the underlying (cached) plot requires redrawing, otherwise false
		/// </returns>
		public override bool OnKeyPressed (KeyEventArgs args, PlotCanvas pc)
		{
			double factor = Sensitivity;
			Key key = args.Key;
			ModifierKeys modifiers = args.Modifiers;

			if ((modifiers & ModifierKeys.Alt) != 0) {
				factor = Sensitivity * altFactor;
			} 

			if (key == Key.Home || key == Key.NumPadHome) {
				pc.SetOriginalDimensions ();
				return true;
			}
			if (key == Key.Left || key == Key.NumPadLeft) {
				pc.CacheAxes();
				pc.TranslateXAxes (factor * left);
				return true;
			}
			if (key == Key.Right || key == Key.NumPadRight) {
				pc.CacheAxes();
				pc.TranslateXAxes (factor*right);
				return true;
			}
			if (key == Key.Up || key == Key.NumPadUp) {
				pc.CacheAxes ();
				pc.TranslateYAxes (factor*up);
				return true;
			}
			if (key == Key.Down || key == Key.NumPadDown) {
				pc.CacheAxes ();
				pc.TranslateYAxes (factor*down);
				return true;
			}
			if (key == Key.Plus || key == Key.NumPadAdd) {
				pc.CacheAxes ();
				pc.ZoomXAxes (zoomIn*factor, symmetrical);
				pc.ZoomYAxes (zoomIn*factor, symmetrical);
				return true;
			}
			if (key == Key.Minus || key == Key.NumPadSubtract) {
				pc.CacheAxes ();
				pc.ZoomXAxes (zoomOut*factor, symmetrical);
				pc.ZoomYAxes (zoomOut*factor, symmetrical);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Handler for KeyRelease events
		/// </summary>
		/// <param name="key">the NPlot key enumeration</param>
		/// <param name="pc">the PlotCanvas</param>
		/// <returns></returns> 
		public override bool OnKeyReleased (KeyEventArgs args, PlotCanvas pc)
		{
			return false;
		}

	} // Key Actions

}