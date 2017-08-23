using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FastBuilder.ViewModels.Build
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

		private readonly Dictionary<BuildCoreViewModel, SortedSet<BuildJobViewModel>> _coreMappedStartTimeSortedJobs
			= new Dictionary<BuildCoreViewModel, SortedSet<BuildJobViewModel>>();

		private double _currentTimeOffset;

		public event EventHandler<BuildJobViewModel> OnJobStarted;
		public event EventHandler<BuildJobViewModel> OnJobFinished;

		public void Add(BuildJobViewModel job)
		{
			if (!_coreMappedStartTimeSortedJobs.TryGetValue(job.OwnerCore, out var jobList))
			{
				jobList = new SortedSet<BuildJobViewModel>(JobStartTimeComparer.Instance);
				_coreMappedStartTimeSortedJobs.Add(job.OwnerCore, jobList);
			}

			jobList.Add(job);

			this.OnJobStarted?.Invoke(this, job);
		}

		public void NotifyJobFinished(BuildJobViewModel job)
		{
			this.OnJobFinished?.Invoke(this, job);
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
	}
}
