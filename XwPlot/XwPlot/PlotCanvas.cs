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

		public PlotCanvas () : base ()
		{
			// Create Drawing Surface with reference to this PlotSurface
			surface = new DrawingSurface (this);
		}

		/// <summary>
		/// Expose the local DrawingSurface (Canvas)
		/// </summary>
		public Canvas Canvas
		{
			get { return (Canvas)surface; }
		}

		/// <summary>
		/// Invalidate the entire plot area on the Canvas.
		/// OnDraw then gets called to draw the contents
		/// </summary>
		public void Refresh ()
		{
			surface.QueueDraw ();
		}

		/// <summary>
		/// Cached (overlay) Canvas with (saved) reference to PlotSurface ps.
		/// Extends Canvas by implementing an off-screen cached drawing surface
		/// from which standard display updates are made. Overlays can also be
		/// drawn over this standard background, to handle any dynamic content.
		/// </summary>
		internal class DrawingSurface : Canvas
		{
			PlotSurface plotSurface;	// To allow access to parent PlotSurface
			Size cacheSize;
			bool cacheDirty = true;

			ImageBuilder ib;
			BitmapImage cache;

			/// <summary>
			/// Creates a new DrawingSurface and saves a reference to the PlotSurface
			/// </summary>
			internal DrawingSurface (PlotSurface ps) : base ()
			{
				plotSurface = ps;
				cacheSize = Size.Zero;
				ib = new ImageBuilder (cacheSize.Width, cacheSize.Height);
			}

			protected override void OnBoundsChanged ()
			{
				base.OnBoundsChanged ();
				UpdateCache ();
				QueueDraw ();		// request full redraw
			}

			protected override void OnDraw (Context ctx, Rectangle dirtyRect)
			{
				// OnDraw checks whether the cache needs to be updated, and if so,
				// calls OnDrawCache to perform this using the off-screen Context.
				// Any Overlay content is then added by calling OnDrawOverlay.
				Matrix ctm = ctx.GetCTM ();
				if (cacheDirty || cacheSize != Bounds.Size) {
					UpdateCache ();
					ctx.DrawImage (cache, Bounds, Bounds);	// Update complete display
				} else {
					ctx.DrawImage (cache, dirtyRect, dirtyRect);	// Update dirtyRect from cache
				}
				//OnDrawCache (ctx, Bounds);		// This shouldn't be necessary - but cache not copying
				OnDrawOverlay (ctx, dirtyRect);		// add overlay content
			}

			/// <summary>
			/// Called when the off-screen cache needs to be redrawn
			/// </summary>
			protected virtual void OnDrawCache (Context ctx, Rectangle dirtyArea)
			{
				// PlotSurface draws itself into the rectangle specified when Draw is called.
				// Consequently, always specify the entire area of the plot cache, since a
				// smaller part of the plot cannot (at present) be drawn.
				plotSurface.Draw (ctx, Bounds);
			}

			/// <summary>
			/// Called when the Overlay content needs to be drawn
			/// </summary>
			protected virtual void OnDrawOverlay (Context ctx, Rectangle dirtyArea)
			{
				// All Overlay content is added by Interactions
			}

			private void UpdateCache ()
			{
				if (Bounds.Size == Size.Zero)
					return;
				if (ib != null)
					ib.Dispose ();
				if (cache != null)
					cache.Dispose ();

				cacheSize = Bounds.Size;
				ib = new ImageBuilder (Bounds.Width, Bounds.Height);
				OnDrawCache (ib.Context, Bounds);
				cache = ib.ToBitmap ();
				cacheDirty = false;
			}
		}
	} 

} 


