using System;
using System.Windows.Controls;
using FastBuilder.Support;

namespace FastBuilder.Views.Build
{
	public partial class BuildJobView 
	{
		public BuildJobView() => InitializeComponent();

		public void Update(double left, double top, double width, string text)
		{
			Canvas.SetLeft(this, left);
			Canvas.SetTop(this, top);
			this.Width = width;

			this.DisplayName.Opacity = Math.Min(1, Math.Max(0, (width - 48) / 48));
			var textWidth = width
			                - this.Border.Padding.Left
			                - this.Border.Padding.Right
			                - this.Border.Margin.Left
			                - this.Border.Margin.Right;
			this.DisplayName.SetTrimmedText(text, textWidth);
		}
	}
}
