using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Caliburn.Micro;
using FastBuilder.Services;
using FastBuilder.ViewModels;

namespace FastBuilder.Views
{

	public partial class BuildSessionView
	{
		private bool _isAutoScrollingContent;
		private double _previousHorizontalScrollOffset;

		private BuildSessionViewModel ViewModel => (BuildSessionViewModel)this.DataContext;

		public BuildSessionView()
		{
			InitializeComponent();
			_isAutoScrollingContent = true;
			_previousHorizontalScrollOffset = this.ContentScrollViewer.ScrollableWidth;
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

			if (this.ContentScrollViewer.ScrollableHeight > 0)
			{
				this.HeaderScrollViewer.ScrollToVerticalOffset(
					e.VerticalOffset * this.HeaderScrollViewer.ScrollableHeight / this.ContentScrollViewer.ScrollableHeight);
			}

			this.UpdateViewTimeRange(ViewTimeRangeChangeReason.Scroll);
		}

		private void UpdateViewTimeRange(ViewTimeRangeChangeReason reason)
		{
			var viewTransformService = IoC.Get<IViewTransformService>();

			if (this.ContentScrollViewer.ScrollableWidth > 0)
			{
				var headerViewWidth = (double)this.FindResource("HeaderViewWidth");
				var startTime = (this.ContentScrollViewer.HorizontalOffset - headerViewWidth) / viewTransformService.Scaling;
				var duration = this.ContentScrollViewer.ActualWidth / viewTransformService.Scaling;
				var endTime = startTime + duration;
				viewTransformService.SetViewTimeRange(startTime, endTime, reason);
			}
			else
			{
				viewTransformService.SetViewTimeRange(0, this.ViewModel.ElapsedTime.TotalSeconds, reason);
			}
		}

		private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var scaleService = IoC.Get<IViewTransformService>();
			scaleService.Scaling = scaleService.Scaling * (1 + e.Delta / 1200.0);

			this.UpdateViewTimeRange(ViewTimeRangeChangeReason.Zoom);

			e.Handled = true;
		}

	}
}
