//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// Modifier.cs
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

namespace XwPlot
{
	/// <summary>
	/// Common set of button/key flags that NPlot will respond to for Interactions
	/// </summary>
	[System.Flags]
	public enum Modifier
	{
		None	= 0x00000,	// no keys
		Alt		= 0x00001,	// the Alt key
		Control = 0x00002,	// the Control key
		Shift	= 0x00004,	// the Shift key
		Command = 0x00008,	// the Command key
		Button1 = 0x00010,	// the first (left) mouse button
		Button2 = 0x00020,	// the second (middle) mouse button
		Button3 = 0x00040,	// the third (right) mouse button
		Spare1	= 0x00080,
		Home	= 0x00100,	// a restricted set of keyboard keys
		End		= 0x00200,	// that NPlot will respond to
		Left	= 0x00400,
		Up		= 0x00800,
		Right	= 0x01000,
		Down	= 0x02000,
		PageUp	= 0x02000,
		PageDn	= 0x04000,
		Plus	= 0x08000,
		Minus	= 0x10000
	}
}

