using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using FastBuild.Dashboard.ViewModels.Build;

namespace FastBuild.Dashboard.Views.Build
{
	partial class BuildJobsView
	{

		private Popup _tooltipPopup;
		private BuildJobTooltipView _tooltip;
		private BuildJobViewModel _tooltipJob;
		private DispatcherTimer _tooltipTimer;

		private void InitializeTooltipPart()
		{
			this.CreateTooltipPopup();
			Application.Current.Deactivated += this.Application_Deactivated;

			_tooltipTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(ToolTipService.GetInitialShowDelay(this))
			};

			_tooltipTimer.Tick += this.TooltipTimer_Tick;
		}

		private void TooltipTimer_Tick(object sender, EventArgs e)
		{
			_tooltipTimer.Stop();

			if (_tooltipJob == null || _tooltipJob != this.JobHitTest())
			{
				return;
			}

			_tooltip.DataContext = _tooltipJob;
			_tooltipPopup.Placement = PlacementMode.RelativePoint;
			_tooltipPopup.PlacementTarget = this;
			var rect = _jobBounds[_tooltipJob];
			rect.Inflate(15, 15);
			_tooltip.MinWidth = Math.Min(rect.Width, this.ActualWidth - _headerViewWidth);
			_tooltipPopup.PlacementRectangle = rect;
			_tooltipPopup.IsOpen = true;
		}

		private void Application_Deactivated(object sender, EventArgs e)
		{
			this.CloseTooltip();
		}

		private void CreateTooltipPopup()
		{
			_tooltipPopup = new Popup
			{
				AllowsTransparency = true
			};

			_tooltip = new BuildJobTooltipView();
			_tooltipPopup.Child = _tooltip;
		}

		private BuildJobViewModel JobHitTest()
		{
			var mousePosition = Mouse.GetPosition(this);

			return _jobBounds.Where(pair => pair.Value.Contains(mousePosition))
				.Select(pair => pair.Key)
				.FirstOrDefault();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (!_tooltipPopup.IsOpen)
			{
				_tooltipJob = this.JobHitTest();
				if (_tooltipJob != null)
				{
					_tooltipTimer.Stop();
					_tooltipTimer.Start();
				}

				return;
			}

			if (_tooltipJob != null)
			{
				var mousePosition = e.GetPosition(this);
				if (_jobBounds[_tooltipJob].Contains(mousePosition))
				{
					return;
				}
			}

			this.CloseTooltip();
		}

		private void CloseTooltip()
		{
			_tooltipJob = null;
			_tooltip.DataContext = null;
			_tooltipPopup.IsOpen = false;
		}
	}
}
