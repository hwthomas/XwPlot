//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// Interaction.cs
// 
// Derived originally from NPlot (Copyright (C) 2003-2006 Matt Howlett and others)
// Updated and ported to Xwt 2012-2014 : Hywel Thomas <hywel.w.thomas@gmail.com>
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
using Xwt;
using Xwt.Drawing;

namespace XwPlot
{
	/// <summary>
	/// Defines the base class for plot "Interactions". An interaction comprises a range
	/// of handlers for mouse and keyboard events that work in a specific way, eg rescaling
	/// the axes, scrolling the PlotSurface, measuring distances, etc. Each handler should
	/// return true if the underlying (cached) plot requires redrawing.
	/// </summary>
	/// <remarks>
	/// This is a virtual base class, rather than abstract, since each Interaction 
	/// will only need to override a limited number of the possible default handlers.
	/// The default handlers below do nothing, and return false (no redraw required).
	/// </remarks>
	public class Interaction
	{
		public Interaction ()
		{
		}

		public virtual bool OnMouseEntered (EventArgs args, PlotSurface2D ps)
		{
			return false;
		}

		public virtual bool OnMouseExited (EventArgs args, PlotSurface2D ps)
		{
			return false;
		}

		public virtual bool OnButtonPressed (ButtonEventArgs args, PlotSurface2D ps)
		{
			return false;
		}
				
		public virtual bool OnButtonReleased (ButtonEventArgs args, PlotSurface2D ps)
		{
			return false;
		}
				
		public virtual bool OnMouseMoved (MouseMovedEventArgs args, PlotSurface2D ps)
		{
			return false;
		}
			
		public virtual bool OnMouseScrolled (MouseScrolledEventArgs args, PlotSurface2D ps)
		{
			return false;
		}
			
		public virtual bool OnKeyPressed (KeyEventArgs args, PlotSurface2D ps)
		{
			return false;
		}

		public virtual bool OnKeyReleased (KeyEventArgs args, PlotSurface2D ps)
		{
			return false;
		}

		/// <summary>
		/// Draw Overlay content over the cached background plot
		/// </summary>
		public virtual void OnDraw (Context ctx, Rectangle dirtyRect)
		{
		}

	}

}

