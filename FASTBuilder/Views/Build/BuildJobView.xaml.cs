using System;
using System.Windows.Controls;
using FastBuilder.Support;

namespace FastBuilder.Views.Build
{
	public partial class BuildJobView
	{
		public BuildJobView() => InitializeComponent();

		public void Update(double left, double top, double width, string text, bool performanceMode)
		{
			Canvas.SetLeft(this, left);
			Canvas.SetTop(this, top);
			this.Width = width;

			if (width < 36)
			{
				this.DisplayName.Visibility = System.Windows.Visibility.Hidden;
			}
			else
			{
				this.DisplayName.Visibility = System.Windows.Visibility.Visible;
				this.DisplayName.Opacity = Math.Min(1, Math.Max(0, (width - 36) / 48));

				var textWidth = width
								- this.Border.Padding.Left
								- this.Border.Padding.Right
								- this.Border.Margin.Left
								- this.Border.Margin.Right;
				this.DisplayName.SetTrimmedText(text, textWidth);

			}
		}
	}
}
