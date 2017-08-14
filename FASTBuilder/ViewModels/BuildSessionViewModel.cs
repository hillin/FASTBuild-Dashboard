using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Caliburn.Micro;
using FastBuilder.Communication;
using FastBuilder.Communication.Events;
using FastBuilder.Services;

namespace FastBuilder.ViewModels
{
	internal partial class BuildSessionViewModel : Screen
	{

		public DateTime StartTime { get; }

		// current time, this could be a historical time if we are restoring history, otherwise should be 
		// a time a bit before now (decided by Tick)
		public DateTime CurrentTime
		{
			get => _currentTime;
			private set
			{
				if (value.Equals(_currentTime)) return;
				_currentTime = value;
				this.NotifyOfPropertyChange();
			}
		}

		public int? ProcessId { get; }
		public int? LogVersion { get; }

		private readonly Dictionary<string, BuildWorkerViewModel> _workerMap = new Dictionary<string, BuildWorkerViewModel>();
		private DateTime _currentTime;

		public TimeSpan ElapsedTime => _currentTime - this.StartTime;
		public string DisplayElapsedTime => this.ElapsedTime.ToString(@"hh\:mm\:ss\.f");

		public BindableCollection<BuildWorkerViewModel> Workers { get; } = new BindableCollection<BuildWorkerViewModel>();
		public TimeRulerViewModel TimeRuler { get; }

		private BuildSessionViewModel(DateTime startTime, int? processId, int? logVersion)
		{
			this.StartTime = startTime;
			this.CurrentTime = startTime;
			this.ProcessId = processId;
			this.LogVersion = logVersion;
			this.IsRunning = true;

			// ReSharper disable once VirtualMemberCallInConstructor
			this.DisplayName = startTime.ToString(CultureInfo.CurrentCulture);

			this.TimeRuler = new TimeRulerViewModel(this);

			this.PoolWorkerNames = new string[0];
			IoC.Get<IWorkerPoolService>().WorkerCountChanged += this.IWorkerPoolService_WorkerCountChanged;
		}


		public BuildSessionViewModel()
			: this(DateTime.Now, null, null)
		{
		}

		public BuildSessionViewModel(StartBuildEventArgs e)
			: this(e.Time, e.ProcessId, e.LogVersion)
		{
		}


		public void OnStopped(StopBuildEventArgs e)
		{
			this.OnStopped(e.Time);
		}

		public void OnStopped(DateTime time)
		{
			// give components a last chance to tick
			this.Tick(time);
			this.IsRunning = false;
			foreach (var worker in this.Workers)
			{
				worker.OnSessionStopped(time);
			}

			this.UpdateActiveWorkerAndCoreCount();
		}

		public void ReportProgress(ReportProgressEventArgs e)
		{
			this.Progress = e.Progress;
		}

		public void ReportCounter(ReportCounterEventArgs e)
		{

		}

		private BuildWorkerViewModel EnsureWorker(string hostName)
		{
			if (!_workerMap.TryGetValue(hostName, out var worker))
			{
				worker = new BuildWorkerViewModel(hostName, this);
				_workerMap.Add(hostName, worker);

				// called from log watcher thread
				lock (this.Workers)
				{
					this.Workers.Add(worker);
				}
			}

			return worker;
		}

		public void OnJobFinished(FinishJobEventArgs e)
		{
			this.EnsureWorker(e.HostName).OnJobFinished(e);
			switch (e.Result)
			{
				case BuildJobStatus.Success:
					++this.SuccessfulJobCount;
					break;
				case BuildJobStatus.SuccessCached:
					++this.SuccessfulJobCount;
					++this.CacheHitCount;
					break;
				case BuildJobStatus.Failed:
				case BuildJobStatus.Error:
					++this.FailedJobCount;
					break;
			}

			--this.InProgressJobCount;

			this.UpdateActiveWorkerAndCoreCount();
		}

		public void OnJobStarted(StartJobEventArgs e)
		{
			this.EnsureWorker(e.HostName).OnJobStarted(e, this.StartTime);
			++this.InProgressJobCount;

			this.UpdateActiveWorkerAndCoreCount();
		}

		public void Tick(DateTime now)
		{
			if (!this.IsRunning)
				return;

			this.CurrentTime = now;
			this.NotifyOfPropertyChange(nameof(this.ElapsedTime));
			this.NotifyOfPropertyChange(nameof(this.DisplayElapsedTime));

			// called from tick thread
			lock (this.Workers)
			{
				foreach (var worker in this.Workers)
				{
					worker.Tick(now);
				}
			}

			this.TimeRuler.Tick(now);
		}
	}
}
