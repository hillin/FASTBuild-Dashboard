using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using FastBuilder.Services;
using FastBuilder.Support;
using FastBuilder.ViewModels.Build;

namespace FastBuilder.Views.Build
{
	partial class BuildJobsView
	{
		// recoreds active (visible) jobs and their corresponding view
		private readonly Dictionary<BuildJobViewModel, BuildJobView> _activeJobViewMap
			= new Dictionary<BuildJobViewModel, BuildJobView>();

		// a queue that stores recycled job views (hidden and no job assigned)
		private readonly Queue<BuildJobView> _jobViewPool
			= new Queue<BuildJobView>();


		private void InitializeJobManagementPart()
		{
			// this timer will slowly fill job pool when program is idle
			var poolFillerTimer = new DispatcherTimer(DispatcherPriority.Background);
			poolFillerTimer.Tick += this.PoolFillerTimer_Tick;
		}

		private void PoolFillerTimer_Tick(object sender, EventArgs e)
		{
			if (_activeJobViewMap.Count + _jobViewPool.Count < _jobManager.JobCount)
			{
				var view = this.CreateJobView();
				view.Visibility = Visibility.Hidden;
				_jobViewPool.Enqueue(view);
			}
		}

		private void ClearJobs()
		{
			foreach (var view in _activeJobViewMap.Values)
			{
				this.RecycleView(view);
			}

			_activeJobViewMap.Clear();
			_visibleCores.Clear();
			_coreTopMap.Clear();
		}

		private bool IsShortJob(BuildJobViewModel job)
		{
			return job.ElapsedSeconds * _buildViewportService.Scaling <= BuildJobView.ShortJobWidthThreshold;
		}

		private void AddJob(BuildJobViewModel job)
		{

			BuildJobView view;
			if (_jobViewPool.Count == 0)
			{
				view = this.CreateJobView();
			}
			else
			{
				view = _jobViewPool.Dequeue();
				view.Visibility = Visibility.Visible;
			}

			view.DataContext = job;
			_activeJobViewMap[job] = view;

		}

		private BuildJobView CreateJobView()
		{
			var view = new BuildJobView();
			this.Canvas.Children.Add(view);
			return view;
		}


		private void UpdateJobs()
		{
			var buildViewportService = IoC.Get<IBuildViewportService>();

			var headerViewWidth = (double)this.FindResource("HeaderViewWidth");

			_startTimeOffset = buildViewportService.ViewStartTimeOffsetSeconds
							   + (headerViewWidth - 8) / buildViewportService.Scaling; // minus 8px to make the jobs looks like being covered under the header panel

			_endTimeOffset = buildViewportService.ViewEndTimeOffsetSeconds;
			_wasNowInTimeFrame = _endTimeOffset >= _currentTimeOffset && _startTimeOffset <= _currentTimeOffset;

			var jobs = new HashSet<BuildJobViewModel>(_jobManager.EnumerateJobs(_startTimeOffset, _endTimeOffset, _visibleCores));

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
			var maxWidth = 24 * 60 * 60 * scaling;
			var performanceMode = _activeJobViewMap.Count > 8;


			foreach (var pair in _activeJobViewMap)
			{
				var job = pair.Key;
				var view = pair.Value;

				var left = Math.Max(minimumLeft, job.StartTimeOffset * scaling);
				var acceptedStartTimeOffset = Math.Max(_startTimeOffset, job.StartTimeOffset);
				var width = MathEx.Clamp((job.EndTimeOffset - acceptedStartTimeOffset) * scaling, 0, maxWidth);

				if (width < BuildJobView.ShortJobWidthThreshold)
				{
					// try to use space before next job
					width = job.NextJob != null
						? MathEx.Clamp((job.NextJob.StartTimeOffset - acceptedStartTimeOffset) * scaling, 0, BuildJobView.ShortJobWidthThreshold)
						: BuildJobView.ShortJobWidthThreshold;
				}

				if (width < 1   // job too short to display
					|| left + width < 1)    // left could be negative
				{
					// we don't recycle this view because it might be shown again after a zoom
					view.Visibility = Visibility.Hidden;
				}
				else
				{
					var top = _coreTopMap[job.OwnerCore];

					view.Visibility = Visibility.Visible;
					view.Update(left, top, width, _jobViewHeight, _jobDisplayMode == BuildJobDisplayMode.Compact ? null : job.DisplayName, performanceMode);
				}
			}
		}

		private void RecycleView(BuildJobView view)
		{
			view.Visibility = Visibility.Hidden;
			view.DataContext = null;
			_jobViewPool.Enqueue(view);
		}
	}
}
