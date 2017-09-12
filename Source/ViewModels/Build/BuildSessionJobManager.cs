using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FastBuild.Dashboard.ViewModels.Build
{
	internal class BuildSessionJobManager
	{
		private class JobStartTimeComparer : IComparer<BuildJobViewModel>
		{
			public static readonly JobStartTimeComparer Instance = new JobStartTimeComparer();
			public int Compare(BuildJobViewModel x, BuildJobViewModel y)
			{
				Debug.Assert(x != null, "x != null");
				Debug.Assert(y != null, "y != null");
				return x.StartTime.CompareTo(y.StartTime);
			}
		}

		// all jobs that are categorized by core, then sorted by start time
		private readonly Dictionary<BuildCoreViewModel, SortedSet<BuildJobViewModel>> _coreMappedStartTimeSortedJobs
			= new Dictionary<BuildCoreViewModel, SortedSet<BuildJobViewModel>>();

		// all active jobs that are assigned to remote workers, used to detect local race
		private readonly Dictionary<string, BuildJobViewModel> _activeRemoteJobMap
			= new Dictionary<string, BuildJobViewModel>();

		// local race detection should be disabled if two or more jobs of the same name are assigned to remote workers
		// this may imply that the job are not well named, maybe many different jobs share a same name
		private bool _isLocalRaceDetectionAvailable = true;

		private double _currentTimeOffset;

		public event EventHandler<BuildJobViewModel> OnJobStarted;
		public event EventHandler<BuildJobViewModel> OnJobFinished;

		public int JobCount { get; private set; }

		public void Add(BuildJobViewModel job)
		{
			if (!_coreMappedStartTimeSortedJobs.TryGetValue(job.OwnerCore, out var jobList))
			{
				jobList = new SortedSet<BuildJobViewModel>(JobStartTimeComparer.Instance);
				_coreMappedStartTimeSortedJobs.Add(job.OwnerCore, jobList);
			}

			jobList.Add(job);

			if (_isLocalRaceDetectionAvailable && !job.OwnerCore.OwnerWorker.IsLocal)
			{
				if (_activeRemoteJobMap.ContainsKey(job.EventName))
				{
					_isLocalRaceDetectionAvailable = false;
				}
				else
				{
					_activeRemoteJobMap.Add(job.EventName, job);
				}
			}

			++this.JobCount;

			this.OnJobStarted?.Invoke(this, job);
		}

		public void NotifyJobFinished(BuildJobViewModel job)
		{
			this.OnJobFinished?.Invoke(this, job);
			
			_activeRemoteJobMap.Remove(job.EventName);	// simply remove, doesn't matter if not existed
		}

		public BuildJobViewModel GetJobPotentiallyWonByLocalRace(BuildJobViewModel job)
		{
			if (!job.OwnerCore.OwnerWorker.IsLocal)
			{
				return null;
			}

			_activeRemoteJobMap.TryGetValue(job.EventName, out var racedJob);
			return racedJob;
		}

		public void Tick(double currentTimeOffset)
		{
			_currentTimeOffset = currentTimeOffset;
		}

		public IEnumerable<BuildJobViewModel> GetAllJobs()
		{
			return _coreMappedStartTimeSortedJobs.Values.SelectMany(j => j);
		}

		public IEnumerable<BuildJobViewModel> EnumerateJobs(double startTimeOffset, double endTimeOffset, HashSet<BuildCoreViewModel> cores)
		{
			var isTimeFrameAfterNow = startTimeOffset > _currentTimeOffset;

			foreach (var pair in _coreMappedStartTimeSortedJobs)
			{
				if (!cores.Contains(pair.Key))
				{
					continue;
				}

				foreach (var job in pair.Value)
				{
					if (job.StartTimeOffset > endTimeOffset)
					{
						break;  // after time frame; because we are sorted by StartTimeOffset in ascending order, the following jobs can all be skipped
					}

					if (job.IsFinished && job.EndTimeOffset < startTimeOffset)
					{
						continue;   // before time frame
					}

					if (!job.IsFinished && isTimeFrameAfterNow)
					{
						continue;   // before time frame
					}

					yield return job;
				}
			}
		}

		public void NotifySessionStopped()
		{
			foreach (var job in this.GetAllJobs().Where(j => !j.IsFinished))
			{
				this.NotifyJobFinished(job);
			}
		}
	}
}
