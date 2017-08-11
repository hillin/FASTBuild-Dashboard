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

namespace FastBuilder.Views
{

	public partial class BuildSessionView
	{
		private bool _isAutoScrollingContent;
		private double _previousHorizontalScrollOffset;

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

			if (this.ContentScrollViewer.ScrollableWidth <= 0)
				return;

			if (this.HeaderScrollViewer.ScrollableHeight > 0)
			{
				this.HeaderScrollViewer.ScrollToVerticalOffset(
					e.VerticalOffset * this.HeaderScrollViewer.ScrollableHeight / this.ContentScrollViewer.ScrollableHeight);
			}

			if (this.TimeRulerScrollViewer.ScrollableWidth > 0)
			{
				this.TimeRulerScrollViewer.ScrollToHorizontalOffset(
					horizontalOffset * this.TimeRulerScrollViewer.ScrollableWidth / this.ContentScrollViewer.ScrollableWidth);
			}
		}

		private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			Debug.WriteLine("mousewheel");
			var scaleService = IoC.Get<IScaleService>();
			scaleService.Scaling = scaleService.Scaling * (1 + e.Delta / 1200.0);

			e.Handled = true;
		}

	}
}
