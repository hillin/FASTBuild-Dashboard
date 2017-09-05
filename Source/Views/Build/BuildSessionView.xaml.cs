using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using FastBuild.Dashboard.Services.Build;

namespace FastBuild.Dashboard.Views.Build
{

	public partial class BuildSessionView
	{
		private bool _isAutoScrollingContent;
		private double _previousHorizontalScrollOffset;

		private static IBuildViewportService BuildViewportService => IoC.Get<IBuildViewportService>();
		private double HeaderViewWidth => App.CachedResource<double>.GetResource("HeaderViewWidth");

		public BuildSessionView()
		{
			InitializeComponent();
			_isAutoScrollingContent = true;
			_previousHorizontalScrollOffset = this.ContentScrollViewer.ScrollableWidth;
			BuildSessionView.BuildViewportService.BuildJobDisplayModeChanged += this.BuildViewportService_BuildJobDisplayModeChanged;
			this.NotifyVerticalViewRangeChanged();
		}

		private void BuildViewportService_BuildJobDisplayModeChanged(object sender, EventArgs e)
		{
			// when this event is triggered, the layout of header/background is not updated immediately.
			// we use a one-shot timer to delay a synchronization among scroll viewers

			var t = new DispatcherTimer();

			void Callback(object callbackSender, EventArgs callbackArgs)
			{
				this.SynchronizeScrollViewers();
				t.Tick -= Callback;
				t.IsEnabled = false;
			}

			t.Tick += Callback;

			t.IsEnabled = true;
		}

		private void ContentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			var horizontalOffset = this.ContentScrollViewer.HorizontalOffset;
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

			this.SynchronizeScrollViewers();

			this.NotifyVerticalViewRangeChanged();

			var startTime = (this.ContentScrollViewer.HorizontalOffset - this.HeaderViewWidth) / BuildSessionView.BuildViewportService.Scaling;
			var duration = this.ContentScrollViewer.ActualWidth / BuildSessionView.BuildViewportService.Scaling;
			var endTime = startTime + duration;
			BuildSessionView.BuildViewportService.SetViewTimeRange(startTime, endTime);
		}

		private void SynchronizeScrollViewers()
		{
			if (this.ContentScrollViewer.ScrollableHeight > 0)
			{
				var offset = this.ContentScrollViewer.VerticalOffset * this.HeaderScrollViewer.ScrollableHeight /
							 this.ContentScrollViewer.ScrollableHeight;
				this.HeaderScrollViewer.ScrollToVerticalOffset(offset);
				this.BackgroundScrollViewer.ScrollToVerticalOffset(offset);
			}
			else
			{
				this.HeaderScrollViewer.ScrollToVerticalOffset(0);
				this.BackgroundScrollViewer.ScrollToVerticalOffset(0);
			}
		}

		private void NotifyVerticalViewRangeChanged()
		{
			BuildSessionView.BuildViewportService.SetVerticalViewRange(this.ContentScrollViewer.VerticalOffset,
				this.ContentScrollViewer.VerticalOffset + this.ContentScrollViewer.ViewportHeight);
		}

		private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var buildViewportService = IoC.Get<IBuildViewportService>();

			var relativePosition = e.GetPosition(this);
			var proportion = relativePosition.X / this.ActualWidth;

			var fixedTime = buildViewportService.ViewStartTimeOffsetSeconds
			                + (buildViewportService.ViewEndTimeOffsetSeconds - buildViewportService.ViewStartTimeOffsetSeconds)
			                * proportion;

			buildViewportService.Scaling = buildViewportService.Scaling * (1 + e.Delta / 1200.0);

			var startTime = fixedTime - this.ContentScrollViewer.ActualWidth / buildViewportService.Scaling * proportion;

			this.ContentScrollViewer.ScrollToHorizontalOffset(startTime * buildViewportService.Scaling + this.HeaderViewWidth);

			e.Handled = true;
		}

		private void BuildJobsView_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e) 
			=> this.NotifyVerticalViewRangeChanged();
	}
}
