using System;
using System.Collections.Generic;
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
	/// <summary>
	/// Interaction logic for BuildSessionView.xaml
	/// </summary>
	public partial class BuildSessionView : UserControl
	{
		public BuildSessionView()
		{
			InitializeComponent();
		}

		private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			var workerScrollViewer = (ScrollViewer) sender;
			if (workerScrollViewer.ScrollableWidth <= 0)
				return;

			if (this.HeaderScrollViewer.ScrollableHeight > 0)
			{
				this.HeaderScrollViewer.ScrollToVerticalOffset(
					e.VerticalOffset * this.HeaderScrollViewer.ScrollableHeight /workerScrollViewer.ScrollableHeight);
			}

			if (this.TimeRulerScrollViewer.ScrollableWidth > 0 )
			{
				this.TimeRulerScrollViewer.ScrollToHorizontalOffset(
					e.HorizontalOffset * this.TimeRulerScrollViewer.ScrollableWidth / workerScrollViewer.ScrollableWidth);
			}
		}

		private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var scaleService = IoC.Get<IScaleService>();
			scaleService.Scaling = scaleService.Scaling * (1 + e.Delta / 1200.0);
		}
	}
}
