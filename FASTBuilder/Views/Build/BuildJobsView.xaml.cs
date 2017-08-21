using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using FastBuilder.Services;
using FastBuilder.ViewModels.Build;

namespace FastBuilder.Views.Build
{
	public partial class BuildJobsView
	{
		private double _startTimeOffset;
		private double _endTimeOffset;
		private double _currentTimeOffset;
		private bool _wasNowInTimeFrame;

		private BuildSessionJobManager _jobManager;

		private readonly Dictionary<BuildJobViewModel, BuildJobView> _activeJobViewMap
			= new Dictionary<BuildJobViewModel, BuildJobView>();

		private readonly Queue<BuildJobView> _jobViewPool
			= new Queue<BuildJobView>();

		private readonly IViewTransformService _viewTransformService;
		private BuildSessionViewModel _sessionViewModel;

		public BuildJobsView()
		{
			InitializeComponent();
			_viewTransformService = IoC.Get<IViewTransformService>();
			_viewTransformService.PreScalingChanging += this.OnPreScalingChanging;
			_viewTransformService.ViewTimeRangeChanged += this.OnViewTimeRangeChanged;

			this.DataContextChanged += this.FastBuildJobsView_DataContextChanged;
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
		}

		private void ClearJobs()
		{
			foreach (var view in _activeJobViewMap.Values)
			{
				this.RecycleView(view);
			}

			_activeJobViewMap.Clear();
		}

		private void JobManager_OnJobStarted(object sender, BuildJobViewModel job)
		{
			this.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				if (job.StartTimeOffset <= _endTimeOffset && job.EndTimeOffset >= _startTimeOffset)
				{
					this.AddJob(job);
				}

				this.UpdateAllViewPositions();
			}));
		}

		private void AddJob(BuildJobViewModel job)
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
				if (!_wasNowInTimeFrame && isNowInTimeFrame)
				{
					// "now" has come into current time frame, add all active jobs
					foreach (var job in _jobManager.GetAllJobs().Where(j => !j.IsFinished))
					{
						if (!_activeJobViewMap.ContainsKey(job))
						{
							this.AddJob(job);
						}
					}

					this.UpdateAllViewPositions();
				}

				_wasNowInTimeFrame = isNowInTimeFrame;

				this.UpdateCanvasSize();

			}));
		}

		private void UpdateCanvasSize()
		{
			this.Canvas.Width = _sessionViewModel.ElapsedTime.TotalSeconds * _viewTransformService.Scaling;
		}

		private void OnViewTimeRangeChanged(object sender, ViewTimeRangeChangeReason e)
		{
			this.UpdateTimeFrame();
		}

		private void UpdateTimeFrame()
		{
			var viewTransformService = IoC.Get<IViewTransformService>();

			var headerViewWidth = (double)this.FindResource("HeaderViewWidth");

			_startTimeOffset = viewTransformService.ViewStartTimeOffsetSeconds 
				+ (headerViewWidth - 8) / viewTransformService.Scaling;	// minus 8px to make the jobs looks like being covered under the header panel

			_endTimeOffset = viewTransformService.ViewEndTimeOffsetSeconds;
			_wasNowInTimeFrame = _endTimeOffset >= _currentTimeOffset && _startTimeOffset <= _currentTimeOffset;

			var jobs = new HashSet<BuildJobViewModel>(_jobManager.EnumerateJobs(_startTimeOffset, _endTimeOffset));

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

			this.UpdateAllViewPositions();
		}

		private void UpdateAllViewPositions()
		{
			var scaling = _viewTransformService.Scaling;

			var minimumLeft = scaling * _startTimeOffset ;

			foreach (var pair in _activeJobViewMap)
			{
				var job = pair.Key;
				var view = pair.Value;

				const double coreRowHeight = 28;
				const double coreVerticalMargin = 0;
				const double workerVerticalMargin = 12;

				var top = 0.0;

				var session = job.OwnerCore.OwnerWorker.OwnerSession;
				var workerIndex = session.Workers.IndexOf(job.OwnerCore.OwnerWorker);

				for (var i = 0; i < workerIndex; ++i)
				{
					var worker = session.Workers[i];
					top += workerVerticalMargin * 2 + worker.Cores.Count * (coreRowHeight + coreVerticalMargin * 2);
				}

				top += workerVerticalMargin;

				top += job.OwnerCore.Id * (coreRowHeight + coreVerticalMargin * 2);
				top += coreVerticalMargin;

				Canvas.SetTop(view, top);

				Canvas.SetLeft(view, Math.Max(minimumLeft, job.StartTimeOffset * scaling));
				view.Width = Math.Min(job.EndTimeOffset - Math.Max(_startTimeOffset, job.StartTimeOffset), 24 * 60 * 60) * scaling;
				job.ShouldShowText = view.Width >= 48;
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
			this.UpdateTimeFrame();
			this.UpdateCanvasSize();
		}
	}
}
