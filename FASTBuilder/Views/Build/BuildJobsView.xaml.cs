using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using FastBuilder.Services;
using FastBuilder.ViewModels.Build;
using FastBuilder.Support;

namespace FastBuilder.Views.Build
{
	public partial class BuildJobsView
	{
		private const double CoreRowHeight = 28;
		private const double CoreVerticalMargin = 0;
		private const double WorkerVerticalMargin = 12;


		private double _startTimeOffset;
		private double _endTimeOffset;
		private double _currentTimeOffset;
		private bool _wasNowInTimeFrame;

		private BuildSessionJobManager _jobManager;

		// recoreds active (visible) jobs and their corresponding view
		private readonly Dictionary<IBuildJobViewModel, BuildJobView> _activeJobViewMap
			= new Dictionary<IBuildJobViewModel, BuildJobView>();

		// a queue that stores recycled job views (hidden and no job assigned)
		private readonly Queue<BuildJobView> _jobViewPool
			= new Queue<BuildJobView>();

		// maps a core row to the top position of its jobs
		private readonly Dictionary<BuildCoreViewModel, double> _coreTopMap
			= new Dictionary<BuildCoreViewModel, double>();

		private readonly HashSet<BuildCoreViewModel> _visibleCores
			= new HashSet<BuildCoreViewModel>();

		private readonly IBuildViewportService _buildViewportService;
		private BuildSessionViewModel _sessionViewModel;

		public BuildJobsView()
		{
			InitializeComponent();
			_buildViewportService = IoC.Get<IBuildViewportService>();
			_buildViewportService.ScalingChanging += this.OnPreScalingChanging;
			_buildViewportService.ViewTimeRangeChanged += this.OnViewTimeRangeChanged;
			_buildViewportService.VerticalViewRangeChanged += this._buildViewportService_VerticalViewRangeChanged;

			this.DataContextChanged += this.FastBuildJobsView_DataContextChanged;
		}

		private void _buildViewportService_VerticalViewRangeChanged(object sender, EventArgs e)
		{
			this.UpdateVisibleCores();
			this.UpdateJobs();
		}

		private void UpdateVisibleCores()
		{
			_visibleCores.Clear();

			foreach (var pair in _coreTopMap)
			{
				var top = pair.Value;
				var bottom = top + CoreRowHeight;

				if (top <= _buildViewportService.ViewBottom && bottom >= _buildViewportService.ViewTop)
				{
					_visibleCores.Add(pair.Key);
				}
			}
		}

		private void FastBuildJobsView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (_sessionViewModel != null)
			{
				_sessionViewModel.Ticked -= this.OnTicked;
				_sessionViewModel = null;

				_jobManager.OnJobStarted -= this.JobManager_OnJobStarted;
				_jobManager = null;

				this.ClearJobs();
			}

			var vm = this.DataContext as BuildSessionViewModel;
			if (vm == null)
			{
				return;
			}

			_sessionViewModel = vm;
			_sessionViewModel.Ticked += this.OnTicked;

			_jobManager = vm.JobManager;
			_jobManager.OnJobStarted += this.JobManager_OnJobStarted;

			this.UpdateCoreTopMap();
			this.UpdateVisibleCores();
			this.UpdateJobs();
		}

		private void ClearJobs()
		{
			foreach (var view in _activeJobViewMap.Values)
			{
				this.RecycleView(view);
			}

			_activeJobViewMap.Clear();
			_coreTopMap.Clear();
			_visibleCores.Clear();
			_coreTopMap.Clear();
		}

		private void JobManager_OnJobStarted(object sender, IBuildJobViewModel job)
		{
			this.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				this.UpdateCoreTopMap();
				this.UpdateVisibleCores();

				if (job.StartTimeOffset <= _endTimeOffset && job.EndTimeOffset >= _startTimeOffset)
				{
					this.AddJob(job);
				}

				this.UpdateJobViews();
			}));
		}

		private void UpdateCoreTopMap()
		{
			var top = 0.0;

			_coreTopMap.Clear();

			foreach (var worker in _sessionViewModel.Workers)
			{
				top += WorkerVerticalMargin;

				foreach (var core in worker.Cores)
				{
					top += CoreVerticalMargin;
					_coreTopMap[core] = top;
					top += CoreRowHeight;
					top += CoreVerticalMargin;
				}

				top += WorkerVerticalMargin;
			}
		}

		private bool IsShortJob(BuildJobViewModel job)
		{
			return job.ElapsedSeconds * _buildViewportService.Scaling <= BuildJobView.ShortJobThreshold;
		}

		private void AddJob(IBuildJobViewModel job)
		{

			BuildJobView view;
			if (_jobViewPool.Count == 0)
			{
				view = new BuildJobView();
				this.Canvas.Children.Add(view);
			}
			else
			{
				view = _jobViewPool.Dequeue();
				view.Visibility = Visibility.Visible;
			}

			view.DataContext = job;
			_activeJobViewMap[job] = view;

		}

		private void OnTicked(object sender, double timeOffset)
		{
			this.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				_currentTimeOffset = timeOffset;

				var isNowInTimeFrame = _endTimeOffset >= _currentTimeOffset && _startTimeOffset <= _currentTimeOffset;
				if (isNowInTimeFrame)
				{
					if (!_wasNowInTimeFrame)
					{
						// "now" has come into current time frame, add all active jobs
						foreach (var job in _jobManager.GetAllJobs().Where(j => !j.IsFinished))
						{
							if (!_activeJobViewMap.ContainsKey(job))
							{
								this.AddJob(job);
							}
						}
					}

					this.UpdateJobViews();
				}

				_wasNowInTimeFrame = isNowInTimeFrame;

				this.UpdateCanvasSize();
			}));
		}

		private void UpdateCanvasSize() => this.Canvas.Width = _sessionViewModel.ElapsedTime.TotalSeconds * _buildViewportService.Scaling;

		private void OnViewTimeRangeChanged(object sender, EventArgs e) => this.UpdateJobs();

		private void UpdateJobs()
		{
			var buildViewportService = IoC.Get<IBuildViewportService>();

			var headerViewWidth = (double)this.FindResource("HeaderViewWidth");

			_startTimeOffset = buildViewportService.ViewStartTimeOffsetSeconds
				+ (headerViewWidth - 8) / buildViewportService.Scaling; // minus 8px to make the jobs looks like being covered under the header panel

			_endTimeOffset = buildViewportService.ViewEndTimeOffsetSeconds;
			_wasNowInTimeFrame = _endTimeOffset >= _currentTimeOffset && _startTimeOffset <= _currentTimeOffset;

			var jobs = new HashSet<IBuildJobViewModel>(_jobManager.EnumerateJobs(_startTimeOffset, _endTimeOffset, _visibleCores));

			// remove (recycle) job views that are no longer existed in current time frame
			var keysToRemove = _activeJobViewMap.Keys.Where(key => !jobs.Contains(key)).ToList();

			foreach (var key in keysToRemove)
			{
				var view = _activeJobViewMap[key];
				this.RecycleView(view);
				_activeJobViewMap.Remove(key);
			}

			// create view for jobs which are new to current time frame
			foreach (var job in jobs)
			{
				if (!_activeJobViewMap.ContainsKey(job))
				{
					this.AddJob(job);
				}
			}

			this.UpdateJobViews();
		}

		private void UpdateJobViews()
		{
			var scaling = _buildViewportService.Scaling;

			var minimumLeft = scaling * _startTimeOffset;

			var performanceMode = _activeJobViewMap.Count > 8;

			foreach (var pair in _activeJobViewMap)
			{
				var job = pair.Key;
				var view = pair.Value;

				var left = Math.Max(minimumLeft, job.StartTimeOffset * scaling);
				var width = Math.Max(0, Math.Min(job.EndTimeOffset - Math.Max(_startTimeOffset, job.StartTimeOffset), 24 * 60 * 60) * scaling);
				if (width < 1	// job too short to display
					|| left + width < 1)	// left could be negative
				{
					// we don't recycle this view because it might be shown again after a zoom
					view.Visibility = Visibility.Hidden;
				}
				else
				{
					var top = _coreTopMap[job.OwnerCore];

					view.Visibility = Visibility.Visible;
					view.Update(left, top, width, job.DisplayName, performanceMode);
				}
			}
		}

		private void RecycleView(BuildJobView view)
		{
			view.Visibility = Visibility.Hidden;
			view.DataContext = null;
			_jobViewPool.Enqueue(view);
		}

		private void OnPreScalingChanging(object sender, EventArgs e)
		{
			this.UpdateJobs();
			this.UpdateCanvasSize();
		}
	}
}
