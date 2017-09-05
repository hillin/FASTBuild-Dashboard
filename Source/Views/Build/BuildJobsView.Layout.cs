using System;
using System.Collections.Generic;
using System.Windows;
using FastBuild.Dashboard.Services.Build;
using FastBuild.Dashboard.ViewModels.Build;

namespace FastBuild.Dashboard.Views.Build
{
	partial class BuildJobsView
	{
		private double _coreRowHeight;
		private double _coreRowTopMargin;
		private double _coreRowBottomMargin;
		private double _workerRowTopMargin;
		private double _workerRowBottomMargin;
		private double _jobViewHeight;

		private double _headerViewWidth;

		private BuildJobDisplayMode _jobDisplayMode;

		private bool _coresInvalidated = true;
		private bool _visibleCoresInvalidated = true;
		private bool _jobsInvalidated = true;

		// maps a core row to the top position of its jobs
		private readonly Dictionary<BuildCoreViewModel, double> _coreTopMap
			= new Dictionary<BuildCoreViewModel, double>();

		// maps a job to its bounds, used for hit testing
		// we may convert this to a quad-tree solution in the future
		private readonly Dictionary<BuildJobViewModel, Rect> _jobBounds
			= new Dictionary<BuildJobViewModel, Rect>();

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

			_coreRowHeight = App.CachedResource<double>.GetResource($"BuildCoreRowHeight{postFix}");
			var buildCoreRowMargin = App.CachedResource<Thickness>.GetResource($"BuildCoreRowMargin{postFix}");
			_coreRowTopMargin = buildCoreRowMargin.Top;
			_coreRowBottomMargin = buildCoreRowMargin.Bottom;

			var workerRowMargin = App.CachedResource<Thickness>.GetResource($"BuildWorkerRowMargin{postFix}");
			var workerRowPadding = App.CachedResource<Thickness>.GetResource($"BuildWorkerRowPadding{postFix}");
			_workerRowTopMargin = workerRowMargin.Top + workerRowPadding.Top;
			_workerRowBottomMargin = workerRowMargin.Bottom + workerRowPadding.Bottom;

			_jobViewHeight = App.CachedResource<double>.GetResource($"JobViewHeight{postFix}");

			_headerViewWidth = App.CachedResource<double>.GetResource("HeaderViewWidth");

			this.InvalidateCores();
			this.InvalidateJobs();
		}

		private void UpdateCores()
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

			this.InvalidateVisibleCores();
		}

		private void OnVerticalViewRangeChanged(object sender, EventArgs e)
		{
			this.InvalidateVisibleCores();
			this.InvalidateJobs();
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

		private void OnViewTimeRangeChanged(object sender, EventArgs e) => this.InvalidateJobs();

		private void OnScalingChanged(object sender, EventArgs e)
		{
			this.InvalidateMeasure();
			this.InvalidateJobs();
		}

		private void InvalidateCores()
		{
			if (!_coresInvalidated)
			{
				_coresInvalidated = true;
				this.InvalidateArrange();
			}
		}

		private void InvalidateVisibleCores()
		{
			if (!_visibleCoresInvalidated)
			{
				_visibleCoresInvalidated = true;
				this.InvalidateArrange();
			}
		}

		private void InvalidateJobs()
		{
			if (!_jobsInvalidated)
			{
				_jobsInvalidated = true;
				this.InvalidateArrange();
			}
		}

		protected override Size MeasureOverride(Size constraint)
			=> _sessionViewModel == null
				? new Size(0, 0)
				: new Size(_sessionViewModel.ElapsedTime.TotalSeconds * _buildViewportService.Scaling + _headerViewWidth * 2,
					constraint.Height);

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			if (_sessionViewModel != null)
			{
				if (_coresInvalidated)
				{
					this.UpdateCores();
					_coresInvalidated = false;
				}

				if (_visibleCoresInvalidated)
				{
					this.UpdateVisibleCores();
					_visibleCoresInvalidated = false;
				}

				if (_jobsInvalidated)
				{
					this.UpdateJobs();
					_jobsInvalidated = false;
				}
			}

			return base.ArrangeOverride(arrangeBounds);
		}
	}
}
