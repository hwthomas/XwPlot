//
// XwPlot - A cross-platform charting library using the Xwt toolkit
// 
// LabelPointPlot.cs
//
// Derived originally from NPlot (Copyright (C) 2003-2006 Matt Howlett and others)
// Updated and ported to Xwt 2012-2014 : Hywel Thomas <hywel.w.thomas@gmail.com>
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
using System.Data;
using Xwt;
using Xwt.Drawing;

namespace XwPlot
{
	/// <summary>
	/// Encapsulates functionality for a PointPlot with Labels added
	/// </summary>
	public class LabelPointPlot : PointPlot, ISequencePlot
	{
		/// <summary>
		/// Enumeration of all label positions relative to a point.
		/// </summary>
		public enum LabelPositions
		{
			Above,
			Below,
			Left,
			Right
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		public LabelPointPlot ()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="marker">The marker type to use for this plot.</param>
		public LabelPointPlot (Marker marker) : base (marker)
		{
		}

		/// <summary>
		/// The position of the text label in relation to the point.
		/// </summary>
		public LabelPositions LabelTextPosition
		{
			get {
				return labelTextPosition;
			}
			set {
				labelTextPosition = value;
			}
		}
		private LabelPositions labelTextPosition = LabelPositions.Above;

		/// <summary>
		/// The text datasource to attach to each point.
		/// </summary>
		public object TextData
		{
			get {
				return textData;
			}
			set {
				textData = value;
			}
		}
		object textData;

		/// <summary>
		/// The Font used to write text.
		/// </summary>
		public Font Font
		{
			get {
				return font;
			}
			set {
				font = value;
			}
		}
		private Font font = Font.SystemSansSerifFont.WithSize (8);

		/// <summary>
		/// Draws the plot using the Drawing Context and X, Y axes supplied.
		/// </summary>
		public override void Draw (Context ctx, PhysicalAxis xAxis, PhysicalAxis yAxis)
		{
			SequenceAdapter data = 
				new SequenceAdapter (this.DataSource, this.DataMember, this.OrdinateData, this.AbscissaData);

			TextDataAdapter textData =
				new TextDataAdapter (this.DataSource, this.DataMember, this.TextData);

			TextLayout layout = new TextLayout ();
			layout.Font = Font;

			ctx.Save ();
			ctx.SetColor (Colors.Black);

			for (int i=0; i<data.Count; ++i) {
				try {
					Point p = data[i];
					if (!Double.IsNaN(p.X) && !Double.IsNaN(p.Y)) {
						Point xPos = xAxis.WorldToPhysical (p.X, false);
						Point yPos = yAxis.WorldToPhysical (p.Y, false);
						// first plot the marker
						Marker.Draw (ctx, xPos.X, yPos.Y);
						// then the label
						if (textData[i] != "") {
							layout.Text = textData[i];
							Size size = layout.GetSize ();
							switch (labelTextPosition) {
							case LabelPositions.Above:
								p.X = xPos.X-size.Width/2;
								p.Y = yPos.Y-size.Height-Marker.Size*2/3;
								break;
							case LabelPositions.Below:
								p.X = xPos.X-size.Width/2;
								p.Y = yPos.Y+Marker.Size*2/3;
								break;
							case LabelPositions.Left:
								p.X = xPos.X-size.Width-Marker.Size*2/3;
								p.Y = yPos.Y-size.Height/2;
								break;
							case LabelPositions.Right:
								p.X = xPos.X+Marker.Size*2/3;
								p.Y = yPos.Y-size.Height/2;
								break;
							}
							ctx.DrawTextLayout (layout, p);
						}
					}
				}
				catch {
					throw new XwPlotException ("Error in TextPlot.Draw");
				}
			}
			ctx.Restore ();
		}


		/// <summary>
		/// This class is used in conjunction with SequenceAdapter
		/// to interpret data specified to the TextPlot class.
		/// </summary>
		private class TextDataAdapter
		{
			private object data;
			private object dataSource;
			private string dataMember;

			public TextDataAdapter (object dataSource, string dataMember, object data)
			{
				this.data = data;
				this.dataSource = dataSource;
				this.dataMember = dataMember;
			}

			public string this[int i]
			{
				get {
					// this is inefficient [could set up delegates in constructor].
					if (data is string[]) {
						return ((string[])data)[i];
					}

					if (data is string) {
						if (dataSource == null) {
							throw new XwPlotException ("Error: DataSource null");
						}

						System.Data.DataRowCollection rows;

						if (dataSource is System.Data.DataSet) {
							if (dataMember != null) {
								// TODO error check
								rows = ((DataTable)((DataSet)dataSource).Tables[dataMember]).Rows;
							}
							else {
								// TODO error check
								rows = ((DataTable)((DataSet)dataSource).Tables[0]).Rows;
							}
						}
						else if (dataSource is System.Data.DataTable) {
							rows = ((DataTable)dataSource).Rows;
						}
						else {
							throw new XwPlotException ("Data conversion not implemented");
						}

						return (string)((System.Data.DataRow)(rows[i]))[(string)data];
					}

					if (data is System.Collections.ArrayList) {
						object dataPoint = ((System.Collections.ArrayList)data)[i];
						if (dataPoint is string) {
							return (string)dataPoint;
						}
						throw new XwPlotException( "TextDataAdapter: data not in recognised format" );
					}

					if (data == null) {
						return "text";
					}

					throw new XwPlotException ("Text data type not recognised");
				}
			}

			public int Count
			{
				get {
					// this is inefficient [could set up delegates in constructor].
					if (data == null) {
						return 0;
					}
					if (data is string[]) {
						return ((string[])data).Length;
					}
					if (data is System.Collections.ArrayList) {
						return ((System.Collections.ArrayList)data).Count;
					}
					throw new XwPlotException ("Text data not in correct format");
				}
			}

		}	// TextDataAdapter

	}	// LabelPointPlot

}
