using System;
using System.Windows.Controls;

namespace FastBuilder.Views.Build
{
	public partial class BuildJobView 
	{
		public BuildJobView() => InitializeComponent();

		public void SetDimensions(double left, double top, double width)
		{
			Canvas.SetLeft(this, left);
			Canvas.SetTop(this, top);
			this.Width = width;

			this.DisplayName.Opacity = Math.Min(1, Math.Max(0, (width - 24) / 24));
		}
	}
}
