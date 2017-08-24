using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Caliburn.Micro;
using FastBuilder.Services;
using FastBuilder.ViewModels.Build;
using FastBuilder.Support;

namespace FastBuilder.Views.Build
{
	public partial class BuildJobsView
	{

		private double _startTimeOffset;
		private double _endTimeOffset;
		private double _currentTimeOffset;
		private bool _wasNowInTimeFrame;

		private BuildSessionJobManager _jobManager;

		// a set that stores all the cores visible to current viewport
		private readonly HashSet<BuildCoreViewModel> _visibleCores
			= new HashSet<BuildCoreViewModel>();

		private readonly IBuildViewportService _buildViewportService;
		private BuildSessionViewModel _sessionViewModel;

		public BuildJobsView()
		{
			InitializeComponent();
			_buildViewportService = IoC.Get<IBuildViewportService>();


			this.DataContextChanged += this.FastBuildJobsView_DataContextChanged;
			this.InitializeJobManagementPart();

			this.InitializeLayoutPart();
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


		private void JobManager_OnJobStarted(object sender, BuildJobViewModel job)
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

	}
}
