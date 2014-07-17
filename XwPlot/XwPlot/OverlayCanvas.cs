﻿//
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
	public class OverlayCanvas : Canvas
	{
		ImageBuilder ib;
		BitmapImage cache;
		Size cacheSize;

		/// <summary>
		/// Default constructor
		/// </summary>
		public OverlayCanvas () : base ()
		{
			CanGetFocus = true;
			if (Bounds.Size == Size.Zero)
				return;
			ib = new ImageBuilder (Bounds.Width, Bounds.Height);
			cacheSize = Bounds.Size;
			cache = ib.ToBitmap ();
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

		/// <summary>
		/// Update the cache contents, reallocating the cache if necessary
		/// </summary>
		internal void UpdateCache ()
		{
			if (Bounds.Size == Size.Zero)
				return;
			if (cache != null)
				cache.Dispose ();
			if (ib != null)
				ib.Dispose ();

			ib = new ImageBuilder (Bounds.Width, Bounds.Height);
			// Clear cache to Canvas Background colour
			ib.Context.SetColor (BackgroundColor);
			ib.Context.Rectangle (Bounds);
			ib.Context.Fill ();
			// Draw into cache
			OnDrawCache (ib.Context, Bounds);
			cacheSize = Bounds.Size;
			cache = ib.ToBitmap ();
		}

		#region Canvas (base) overrides
		protected override void OnBoundsChanged ()
		{
			base.OnBoundsChanged ();
			UpdateCache ();			// cache must be redrawn
			QueueDraw ();			// and display updated
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			// OnDraw updates the display from the off-screen cache,
			// then adds Overlay content by calling OnDrawOverlay.
			ctx.DrawImage (cache, dirtyRect, dirtyRect);
			OnDrawOverlay (ctx, dirtyRect);

			//OnDrawCache (ctx, Bounds);	// Test only
		}

		protected override void OnMouseEntered (EventArgs args)
		{
			CanGetFocus = true;
			SetFocus ();		// ensure keypresses are received
		}

		protected override void OnMouseExited (EventArgs args)
		{
			CanGetFocus = false;
		}

		#endregion // overrides

	}

}

