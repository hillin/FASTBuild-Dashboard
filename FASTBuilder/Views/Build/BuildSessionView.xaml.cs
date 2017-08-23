using System;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using FastBuilder.Services;

namespace FastBuilder.Views.Build
{

	public partial class BuildSessionView
	{
		private bool _isAutoScrollingContent;
		private double _previousHorizontalScrollOffset;

		private static IBuildViewportService BuildViewportService => IoC.Get<IBuildViewportService>();
		private double HeaderViewWidth => (double)this.FindResource("HeaderViewWidth");

		public BuildSessionView()
		{
			InitializeComponent();
			_isAutoScrollingContent = true;
			_previousHorizontalScrollOffset = this.ContentScrollViewer.ScrollableWidth;
			BuildSessionView.BuildViewportService.SetVerticalViewRange(0, this.ContentScrollViewer.ViewportHeight);
		}

		private void ContentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			var horizontalOffset = e.HorizontalOffset;
			if (_isAutoScrollingContent)
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (horizontalOffset == _previousHorizontalScrollOffset)    // which means the scroll is not actually changed, but content size is changed
				{
					this.ContentScrollViewer.ScrollToHorizontalOffset(this.ContentScrollViewer.ScrollableWidth);
					horizontalOffset = this.ContentScrollViewer.ScrollableWidth;
				}
			}

			// if we are scrolled to the right end, turn on auto scrolling
			_isAutoScrollingContent = Math.Abs(horizontalOffset - this.ContentScrollViewer.ScrollableWidth) < 1;

			_previousHorizontalScrollOffset = horizontalOffset;


			var buildViewportService = BuildSessionView.BuildViewportService;

			if (this.ContentScrollViewer.ScrollableHeight > 0)
			{
				var offset = e.VerticalOffset * this.HeaderScrollViewer.ScrollableHeight /
							 this.ContentScrollViewer.ScrollableHeight;
				this.HeaderScrollViewer.ScrollToVerticalOffset(offset);
				this.BackgroundScrollViewer.ScrollToVerticalOffset(offset);
			}

			BuildSessionView.BuildViewportService.SetVerticalViewRange(e.VerticalOffset,
				e.VerticalOffset + this.ContentScrollViewer.ViewportHeight);


			var startTime = (this.ContentScrollViewer.HorizontalOffset - this.HeaderViewWidth) / buildViewportService.Scaling;
			var duration = this.ContentScrollViewer.ActualWidth / buildViewportService.Scaling;
			var endTime = startTime + duration;
			buildViewportService.SetViewTimeRange(startTime, endTime);
		}



		private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var buildViewportService = IoC.Get<IBuildViewportService>();
			var middleTime = (buildViewportService.ViewEndTimeOffsetSeconds + buildViewportService.ViewStartTimeOffsetSeconds) / 2;

			buildViewportService.Scaling = buildViewportService.Scaling * (1 + e.Delta / 1200.0);

			var duration = this.ContentScrollViewer.ActualWidth / buildViewportService.Scaling;
			var startTime = middleTime - duration / 2;
			this.ContentScrollViewer.ScrollToHorizontalOffset(startTime * buildViewportService.Scaling + this.HeaderViewWidth);

			e.Handled = true;
		}

	}
}
