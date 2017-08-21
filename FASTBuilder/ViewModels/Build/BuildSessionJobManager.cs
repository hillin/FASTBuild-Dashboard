using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FastBuilder.ViewModels.Build
{
	internal class BuildSessionJobManager
	{
		private class JobStartTimeComparer : IComparer<BuildJobViewModel>
		{
			public int Compare(BuildJobViewModel x, BuildJobViewModel y)
			{
				Debug.Assert(x != null, "x != null");
				Debug.Assert(y != null, "y != null");
				return x.StartTime.CompareTo(y.StartTime);
			}
		}

		private readonly SortedSet<BuildJobViewModel> _startTimeSortedJobs
			= new SortedSet<BuildJobViewModel>(new JobStartTimeComparer());

		private double _currentTimeOffset;

		public event EventHandler<BuildJobViewModel> OnJobStarted;
		public event EventHandler<BuildJobViewModel> OnJobFinished;

		public void Add(BuildJobViewModel job)
		{
			_startTimeSortedJobs.Add(job);

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
			return _startTimeSortedJobs;
		}

		public IEnumerable<BuildJobViewModel> EnumerateJobs(double startTimeOffset, double endTimeOffset)
		{
			var isTimeFrameAfterNow = startTimeOffset > _currentTimeOffset;

			foreach (var job in _startTimeSortedJobs)
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
