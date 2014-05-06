//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// OverlayCanvas.cs
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
	/// Extends Canvas by implementing an off-screen cached drawing surface
	/// from which standard display updates are made. Overlays can also be
	/// drawn over this standard background, to handle any dynamic content.
	/// </summary>
	/// <remarks>
	/// Separate Draw routines are defined for the cached and overlay content
	/// </remarks>
	public class OverlayCanvas : Canvas
	{
		Size startSize = new Size (400, 300);
		Size lastSize;
		//Size cacheSize;
		bool cacheDirty = true;

		ImageBuilder ib;
		BitmapImage cache;

		/// <summary>
		/// Default constructor
		/// </summary>
		public OverlayCanvas () : base ()
		{
			// Create initial ImageBuilder
			lastSize = startSize;
			ib = new ImageBuilder (startSize.Width, startSize.Height);
		}

		/// <summary>
		/// Called when the off-screen cache needs to be redrawn
		/// </summary>
		protected virtual void OnDrawCache (Context ctx, Rectangle dirtyArea)
		{
		}

		/// <summary>
		/// Called when the Overlay content needs to be drawn
		/// </summary>
		protected virtual void OnDrawOverlay (Context ctx, Rectangle dirtyArea)
		{
		}

		protected override void OnBoundsChanged ()
		{
			base.OnBoundsChanged ();
			cacheDirty = true;	// Mark cache as invalid
			UpdateCache ();		// update it
			QueueDraw ();		// and request redraw
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			// OnDraw checks whether the cache needs to be updated, and if so,
			// calls OnDrawCache to perform this using the off-screen Context.
			// Any Overlay content is then added by calling OnDrawOverlay.
			if (cacheDirty || lastSize != Bounds.Size) {
				UpdateCache ();
				OnDrawCache (ib.Context, Bounds);
			}
			// Update screen display from cache
			ctx.DrawImage (cache, dirtyRect, dirtyRect);
			// then add any overlay content direct to screen
			OnDrawOverlay (ctx, dirtyRect);
		}

		private void UpdateCache ()
		{
			if (Bounds.Size == Size.Zero)
				return;
			if (ib != null)
				ib.Dispose ();
			if (cache != null)
				cache.Dispose ();
			// TODO: change cache size only if greater, and then use larger value?
			lastSize = Bounds.Size;
			ib = new ImageBuilder (Bounds.Width, Bounds.Height);
			OnDrawCache (ib.Context, Bounds);
			cache = ib.ToBitmap ();
			cacheDirty = false;
		}

	} // OverlayCanvas

} 



