//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// PlotCanvas.cs
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
// 3. Neither the name of NPlot nor the names of its contributors may
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
using System.Diagnostics;
using System.Collections;

using Xwt;
using Xwt.Drawing;

namespace XwPlot
{
	/// <summary>
	/// Extends PlotSurface by implementing a drawing surface (Xwt.Canvas)
	/// to obtain a Drawing Context with which the PlotSurface is drawn.
	/// </summary>
	/// <remarks>
	/// The Canvas is exposed so that it may be added to any Xwt Window
	/// </remarks>
	public class PlotCanvas : PlotSurface
	{
		private DrawingSurface surface;	// The Xwt Drawing Surface
		public ArrayList interactions = new ArrayList();

		public PlotCanvas () : base ()
		{
			// Create Drawing Surface with a reference to this PlotCanvas
			surface = new DrawingSurface (this);
			// Create empty InteractionOccurred and PreRefresh Event handlers
			InteractionOccurred += new InteractionHandler (OnInteractionOccurred);
			PreRefresh += new PreRefreshHandler (OnPreRefresh);
		}

		/// <summary>
		/// Expose the local DrawingSurface (Canvas)
		/// </summary>
		public Canvas Canvas
		{
			get { return (Canvas)surface; }
		}

		/// <summary>
		/// Force a complete redraw and display of the plot area on the Canvas.
		/// </summary>
		public void Refresh ()
		{
			PreRefresh (this);			// Raise the PreRefresh event
			surface.UpdateCache ();		// First update the cache
			surface.QueueDraw ();		// then redraw the display
		}

		/// <summary>
		/// Clear the plot and reset to default values.
		/// </summary>
		public new void Clear ()
		{
			ClearAxisCache ();
			interactions.Clear ();
			base.Clear ();
		}

		#region Add/Remove Interaction
		/// <summary>
		/// Adds a specific interaction to the PlotSurface
		/// </summary>
		/// <param name="i">the interaction to add.</param>
		public void AddInteraction (Interaction i)
		{
			interactions.Add (i);
		}

		/// <summary>
		/// Remove a previously added interaction
		/// </summary>
		/// <param name="i">interaction to remove</param>
		public void RemoveInteraction (Interaction i)			 
		{
			interactions.Remove (i);
		}
		#endregion // Add/Remove Interaction

		#region Axis Cache
		private bool cached = false;	// at least 1 axis has been cached
		private Axis xAxis1Cache;		// copies of current axes,
		private Axis yAxis1Cache;		// saved for restoring the
		private Axis xAxis2Cache;		// original dimensions after
		private Axis yAxis2Cache;		// zooming, etc

		/// <summary>
		/// Caches the current axes
		/// </summary>
		public void CacheAxes ()
		{
			if (!cached) {
				if (XAxis1 != null) {
					xAxis1Cache = (Axis)XAxis1.Clone();
					cached = true;
				}
				if (XAxis2 != null) {
					xAxis2Cache = (Axis)XAxis2.Clone();
					cached = true;
				}
				if (YAxis1 != null) {
					yAxis1Cache = (Axis)YAxis1.Clone();
					cached = true;
				}
				if (YAxis2 != null) {
					yAxis2Cache = (Axis)YAxis2.Clone();
					cached = true;
				}
			}
		}

		/// <summary>
		/// Sets axes to be those saved in the cache.
		/// </summary>
		public void SetOriginalDimensions ()
		{
			if (cached) {
				XAxis1 = xAxis1Cache;
				XAxis2 = xAxis2Cache;
				YAxis1 = yAxis1Cache;
				YAxis2 = yAxis2Cache;
				ClearAxisCache ();
			}
		}

		protected void ClearAxisCache ()
		{
			xAxis2Cache = xAxis1Cache = null;
			yAxis2Cache = yAxis1Cache = null;
			cached = false;
		}
		#endregion	// Axis Cache

		#region Axis Range utilities
		/// <summary>
		/// Translate all PlotSurface X-Axes by shiftProportion
		/// </summary>
		public void TranslateXAxes (double shiftProportion )
		{
			if (XAxis1 != null) {
				XAxis1.TranslateRange (shiftProportion);
			}
			if (XAxis2 != null) {
				XAxis2.TranslateRange (shiftProportion);
			}
		}

		/// <summary>
		/// Translate all PlotSurface Y-Axes by shiftProportion
		/// </summary>
		public void TranslateYAxes (double shiftProportion )
		{
			if (YAxis1 != null) {
				YAxis1.TranslateRange (shiftProportion);
			}
			if (YAxis2 != null) {
				YAxis2.TranslateRange (shiftProportion);
			}
		}

		/// <summary>
		/// Zoom all PlotSurface X-Axes about focusPoint by zoomProportion 
		/// </summary>
		public void ZoomXAxes (double zoomProportion, double focusRatio)
		{
			if (XAxis1 != null) {
				XAxis1.IncreaseRange (zoomProportion,focusRatio);
			}
			if (XAxis2 != null) {
				XAxis2.IncreaseRange (zoomProportion,focusRatio);
			}
		}

		/// <summary>
		/// Zoom all PlotSurface Y-Axes about focusPoint by zoomProportion 
		/// </summary>
		public void ZoomYAxes (double zoomProportion, double focusRatio)
		{
			if (YAxis1 != null) {
				YAxis1.IncreaseRange (zoomProportion,focusRatio);
			}
			if (YAxis2 != null) {
				YAxis2.IncreaseRange (zoomProportion,focusRatio);
			}
		}

		/// <summary>
		/// Define all PlotSurface X-Axes to minProportion, maxProportion
		/// </summary>
		public void DefineXAxes (double minProportion, double maxProportion)
		{
			if (XAxis1 != null) {
				XAxis1.DefineRange (minProportion, maxProportion, true);
			}
			if (XAxis2 != null) {
				XAxis2.DefineRange (minProportion, maxProportion, true);
			}
		}

		/// <summary>
		/// Define all PlotSurface Y-Axes to minProportion, maxProportion
		/// </summary>
		public void DefineYAxes (double minProportion, double maxProportion)
		{
			if (YAxis1 != null) {
				YAxis1.DefineRange (minProportion, maxProportion, true);
			}
			if (YAxis2 != null) {
				YAxis2.DefineRange(minProportion, maxProportion, true);
			}
		}
		#endregion	// Axis Range utilities

		#region PlotCanvas Events
		/// An Event is raised to notify clients that an Interaction has modified
		/// the PlotSurface, and a separate Event is also raised prior to a call
		/// to refresh the PlotSurface.	 Currently, the conditions for raising
		/// both Events are the same (ie the PlotSurface has been modified)

		/// <summary>
		/// InteractionOccurred event signature
		/// </summary>
		public delegate void InteractionHandler (object sender);

		/// <summary>
		/// Event raised when an interaction modifies the PlotSurface
		/// </summary>
		public event InteractionHandler InteractionOccurred;

		/// <summary>
		/// Default handler called when Interaction modifies PlotSurface
		/// Override this, or add handler to InteractionOccurred event.
		/// </summary>
		protected void OnInteractionOccurred (object sender)
		{
		}

		/// <summary>
		/// PreRefresh event handler signature
		/// </summary>
		public delegate void PreRefreshHandler (object sender);

		/// <summary>
		/// Event raised by Refresh () call, but prior to actual Plot Refresh
		/// </summary>
		public event PreRefreshHandler PreRefresh;

		/// <summary>
		/// Default handler for PreRefresh
		/// Override this, or add handler to PreRefresh event.
		/// </summary>
		protected void OnPreRefresh (object sender)
		{
		}
		#endregion	// PlotCanvas Events

		#region DrawingSurface
		/// <summary>
		/// Cached (overlay) Canvas with (saved) reference to PlotSurface ps.
		/// Extends Canvas by implementing an off-screen cached drawing surface
		/// from which standard display updates are made. Overlays can also be
		/// drawn over this standard background, to handle any dynamic content.
		/// </summary>
		internal class DrawingSurface : Canvas
		{
			PlotCanvas plotCanvas;	// To allow access to parent PlotCanvas
			Size cacheSize;

			ImageBuilder ib;
			BitmapImage cache;

			/// <summary>
			/// Creates a new DrawingSurface and saves a reference to the PlotSurface
			/// </summary>
			internal DrawingSurface (PlotCanvas pc) : base ()
			{
				CanGetFocus = true;
				plotCanvas = pc;
				cacheSize = Bounds.Size;
				ib = new ImageBuilder (cacheSize.Width, cacheSize.Height);
				cache = ib.ToBitmap ();
			}

			/// <summary>
			/// Called when the off-screen cache needs to be redrawn
			/// </summary>
			protected virtual void OnDrawCache (Context ctx, Rectangle dirtyArea)
			{
				// First clear cache to Canvas Background colour
				// should be no need to Save () and Restore ()
				ctx.Save ();
				ctx.SetColor (BackgroundColor);
				ctx.Rectangle (Bounds);
				ctx.Fill ();
				ctx.Restore ();
				// PlotSurface draws itself into the rectangle specified when Draw is called.
				// Consequently, always specify the entire area of the plot cache, since a
				// smaller part of the plot cannot (at present) be drawn.
				plotCanvas.Draw (ctx, Bounds);
			}

			protected virtual void OnDrawOverlay (Context ctx, Rectangle dirtyArea)
			{
				// All Overlay content is added by PlotSurface Interactions
				foreach (Interaction interaction in plotCanvas.interactions) {
					interaction.OnDraw (ctx, dirtyArea);
				}
			}

			/// <summary>
			/// Update the cache contents, reallocating the cache if necessary
			/// </summary>
			internal void UpdateCache ()
			{
				if (Bounds.Size == Size.Zero)
					return;
				if (cacheSize != Bounds.Size) {
					if (ib != null)
						//ib.Dispose ();
					if (cache != null)
						//cache.Dispose ();
					cacheSize = Bounds.Size;
					ib = new ImageBuilder (cacheSize.Width, cacheSize.Height);
				}
				OnDrawCache (ib.Context, Bounds);
				cache = ib.ToBitmap ();
			}

			#region Canvas (base) overrides
			protected override void OnBoundsChanged ()
			{
				base.OnBoundsChanged ();
				UpdateCache ();
				QueueDraw ();		// request full redraw
			}

			protected override void OnDraw (Context ctx, Rectangle dirtyRect)
			{
				// OnDraw updates the display from the off-screen cache,
				// then adds Overlay content by calling OnDrawOverlay.
				ctx.DrawImage (cache, dirtyRect, dirtyRect);
				OnDrawOverlay (ctx, dirtyRect);
			}

			protected override void OnMouseEntered (EventArgs args)
			{
				SetFocus ();		// ensure keypresses are received
				bool modified = false;
				foreach (Interaction interaction in plotCanvas.interactions) {
					modified |= interaction.OnMouseEntered (args, plotCanvas);
				}
				CheckForRedraw (modified);
			}

			protected override void OnMouseExited (EventArgs args)
			{
				bool modified = false;
				foreach (Interaction interaction in plotCanvas.interactions) {
					modified |= interaction.OnMouseExited (args, plotCanvas);
				}
				CheckForRedraw (modified);
			}

			protected override void OnButtonPressed (ButtonEventArgs args)
			{
				bool modified = false;
				foreach (Interaction interaction in plotCanvas.interactions) {
					modified |= interaction.OnButtonPressed (args, plotCanvas);
				}
				CheckForRedraw (modified);
			}

			protected override void OnButtonReleased (ButtonEventArgs args)
			{
				bool modified = false;
				foreach (Interaction interaction in plotCanvas.interactions) {
					modified |= interaction.OnButtonReleased (args, plotCanvas);
				}
				CheckForRedraw (modified);
			}

			protected override void OnMouseMoved (MouseMovedEventArgs args)
			{
			}

			protected override void OnMouseScrolled (MouseScrolledEventArgs args)
			{
			}

			protected override void OnKeyPressed (KeyEventArgs args)
			{
				bool modified = false;
				foreach (Interaction interaction in plotCanvas.interactions) {
					modified |= interaction.OnKeyPressed (args, plotCanvas);
				}
				CheckForRedraw (modified);
			}

			protected override void OnKeyReleased (KeyEventArgs args)
			{
				bool modified = false;
				foreach (Interaction interaction in plotCanvas.interactions) {
					modified |= interaction.OnKeyReleased (args, plotCanvas);
				}
				CheckForRedraw (modified);
			}

			private void CheckForRedraw (bool modified)
			{
				if (modified) {
					plotCanvas.InteractionOccurred (this);
					plotCanvas.Refresh ();
				}
			}

			#endregion // overrides

		}
		#endregion	// DrawingSurface class

	} 

} 


