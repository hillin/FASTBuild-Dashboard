using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using FastBuild.Dashboard.Services.Build;
using FastBuild.Dashboard.ViewModels.Build;
using Action = System.Action;

namespace FastBuild.Dashboard.Views.Build
{
	public partial class BuildJobsView : Control
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
			_buildViewportService = IoC.Get<IBuildViewportService>();

			this.DataContextChanged += this.FastBuildJobsView_DataContextChanged;

			this.Background = Brushes.Transparent;
			
			this.InitializeLayoutPart();
			this.InitializeRenderPart();
			this.InitializeTooltipPart();
		}

		private void FastBuildJobsView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (_sessionViewModel != null)
			{
				_sessionViewModel.Ticked -= this.OnTicked;
				_sessionViewModel = null;

				_jobManager.OnJobFinished -= this.JobManager_OnJobFinished;
				_jobManager.OnJobStarted -= this.JobManager_OnJobStarted;
				_jobManager = null;

				this.Clear();
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
			_jobManager.OnJobFinished += this.JobManager_OnJobFinished;

			this.UpdateTimeFrame();

			this.InvalidateCores();
			this.InvalidateJobs();
		}

		private void JobManager_OnJobFinished(object sender, BuildJobViewModel e) 
			=> this.Dispatcher.BeginInvoke(new Action(this.InvalidateVisual));

		private void JobManager_OnJobStarted(object sender, BuildJobViewModel job)
		{
			this.Dispatcher.BeginInvoke(new System.Action(() =>
			{
				this.InvalidateCores();

				if (job.StartTimeOffset <= _endTimeOffset && job.EndTimeOffset >= _startTimeOffset)
				{
					this.TryAddJob(job);
				}

				this.InvalidateVisual();
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
							this.TryAddJob(job);
						}
					}
				}

				this.InvalidateMeasure();

				_wasNowInTimeFrame = isNowInTimeFrame;
			}));
		}

	}
}
