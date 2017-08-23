using System;
using System.Windows;
using System.Windows.Controls;
using FastBuilder.Support;

namespace FastBuilder.Views.Build
{
	public partial class BuildJobView
	{
		public const double ShortJobWidthThreshold = 36;

		public BuildJobView() => InitializeComponent();

		public void Update(double left, double top, double width, string text, bool performanceMode)
		{
			Canvas.SetLeft(this, left);
			Canvas.SetTop(this, top);
			this.Width = width;

			this.Border.CornerRadius = new CornerRadius(MathEx.Clamp((this.Width - 12) / 2, 0, 2));

			if (width < ShortJobWidthThreshold)
			{
				this.DisplayName.Visibility = Visibility.Hidden;
			}
			else
			{
				this.DisplayName.Visibility = System.Windows.Visibility.Visible;
				this.DisplayName.Opacity = Math.Min(1, Math.Max(0, (width - ShortJobWidthThreshold) / 48));

				if (performanceMode)
				{
					this.DisplayName.Text = text;
				}
				else
				{
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
}
