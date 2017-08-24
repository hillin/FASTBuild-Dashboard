using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FastBuilder.Services;
using FastBuilder.ViewModels.Build;

namespace FastBuilder.Views.Build
{
	partial class BuildJobsView
	{
		private double _coreRowHeight;
		private double _coreRowTopMargin;
		private double _coreRowBottomMargin;
		private double _workerRowTopMargin;
		private double _workerRowBottomMargin;
		private double _jobViewHeight;
		private BuildJobDisplayMode _jobDisplayMode;

		// maps a core row to the top position of its jobs
		private readonly Dictionary<BuildCoreViewModel, double> _coreTopMap
			= new Dictionary<BuildCoreViewModel, double>();

		private void InitializeLayoutPart()
		{
			this.UpdateLayoutParameters();
			_buildViewportService.BuildJobDisplayModeChanged += this.OnBuildJobDisplayModeChanged;
			_buildViewportService.ScalingChanged += this.OnScalingChanged;
			_buildViewportService.ViewTimeRangeChanged += this.OnViewTimeRangeChanged;
			_buildViewportService.VerticalViewRangeChanged += this.OnVerticalViewRangeChanged;
		}

		private void OnBuildJobDisplayModeChanged(object sender, EventArgs e)
			=> this.UpdateLayoutParameters();

		private void UpdateLayoutParameters()
		{
			_jobDisplayMode = _buildViewportService.BuildJobDisplayMode;
			var postFix = _jobDisplayMode.ToString();

			// queried resources are defined in /Theme/Layout.xaml

			_coreRowHeight = (double)this.FindResource($"BuildCoreRowHeight{postFix}");
			var buildCoreRowMargin = (Thickness)this.FindResource($"BuildCoreRowMargin{postFix}");
			_coreRowTopMargin = buildCoreRowMargin.Top;
			_coreRowBottomMargin = buildCoreRowMargin.Bottom;

			var workerRowMargin = (Thickness)this.FindResource($"BuildWorkerRowMargin{postFix}");
			var workerRowPadding = (Thickness)this.FindResource($"BuildWorkerRowPadding{postFix}");
			_workerRowTopMargin = workerRowMargin.Top + workerRowPadding.Top;
			_workerRowBottomMargin = workerRowMargin.Bottom + workerRowPadding.Bottom;

			_jobViewHeight = (double)this.FindResource($"JobViewHeight{postFix}");

			if (_sessionViewModel != null)
			{
				this.UpdateCoreTopMap();
				this.UpdateVisibleCores();
				this.UpdateJobs();
			}
		}

		private void UpdateCoreTopMap()
		{
			var top = 0.0;

			_coreTopMap.Clear();

			foreach (var worker in _sessionViewModel.Workers)
			{
				top += _workerRowTopMargin;

				foreach (var core in worker.Cores)
				{
					top += _coreRowTopMargin;
					_coreTopMap[core] = top;
					top += _coreRowHeight;
					top += _coreRowBottomMargin;
				}

				top += _workerRowBottomMargin;
			}
		}

		private void OnVerticalViewRangeChanged(object sender, EventArgs e)
		{
			if (_sessionViewModel == null)
			{
				// this could happen even before view model is assigned
				return;
			}

			this.UpdateVisibleCores();
			this.UpdateJobs();
		}

		private void UpdateVisibleCores()
		{
			_visibleCores.Clear();

			foreach (var pair in _coreTopMap)
			{
				var top = pair.Value;
				var bottom = top + _coreRowHeight;

				if (top <= _buildViewportService.ViewBottom && bottom >= _buildViewportService.ViewTop)
				{
					_visibleCores.Add(pair.Key);
				}
			}
		}

		private void UpdateCanvasSize() => this.Canvas.Width = _sessionViewModel.ElapsedTime.TotalSeconds * _buildViewportService.Scaling;

		private void OnViewTimeRangeChanged(object sender, EventArgs e) => this.UpdateJobs();

		private void OnScalingChanged(object sender, EventArgs e)
		{
			this.UpdateJobs();
			this.UpdateCanvasSize();
		}
	}
}
