using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Timers;
using Caliburn.Micro;
using FastBuild.Dashboard.Communication;
using FastBuild.Dashboard.Communication.Events;
using FastBuild.Dashboard.Services;

namespace FastBuild.Dashboard.ViewModels.Build
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
				if (value.Equals(_currentTime))
				{
					return;
				}

				_currentTime = value;
				this.NotifyOfPropertyChange();

				this.NotifyOfPropertyChange(nameof(this.ElapsedTime));
				this.NotifyOfPropertyChange(nameof(this.DisplayElapsedTime));
			}
		}

		public int? ProcessId { get; }
		private Process _fbuildProcess;
		public int? LogVersion { get; }

		private readonly Dictionary<string, BuildWorkerViewModel> _workerMap = new Dictionary<string, BuildWorkerViewModel>();
		private DateTime _currentTime;

		public TimeSpan ElapsedTime => _currentTime - this.StartTime;
		public string DisplayElapsedTime => this.ElapsedTime.ToString(@"hh\:mm\:ss\.f");

		public event EventHandler<double> Ticked;

		public BindableCollection<BuildWorkerViewModel> Workers { get; } = new BindableCollection<BuildWorkerViewModel>();

		public BuildSessionJobManager JobManager { get; } = new BuildSessionJobManager();
		public BuildInitiatorProcessViewModel InitiatorProcess { get; }

		private BuildSessionViewModel(DateTime startTime, int? processId, int? logVersion)
		{
			this.StartTime = startTime;
			this.CurrentTime = startTime;
			this.ProcessId = processId;
			this.LogVersion = logVersion;
			this.IsRunning = true;

			// ReSharper disable once VirtualMemberCallInConstructor
			this.DisplayName = startTime.ToString(CultureInfo.CurrentCulture);

			var brokerageService = IoC.Get<IBrokerageService>();
			this.PoolWorkerNames = brokerageService.WorkerNames;
			brokerageService.WorkerCountChanged += this.BrokerageService_WorkerCountChanged;

			this.InitiatorProcess = new BuildInitiatorProcessViewModel(processId);

			this.WatchBuildProcess();
		}

		public BuildSessionViewModel()
			: this(DateTime.Now, null, null)
		{
		}

		public BuildSessionViewModel(StartBuildEventArgs e)
			: this(e.Time, e.ProcessId, e.LogVersion)
		{
		}


		private void WatchBuildProcess()
		{
			if (this.ProcessId == null)
			{
				return;
			}

			if (!BuildInitiatorProcessViewModel.GetIsProcessAccessible(this.ProcessId.Value))
			{
				// process not accessible, it's either exited (historical build)
				// or running by an account with higher privilege
				return;
			}

			try
			{
				var process = Process.GetProcessById(this.ProcessId.Value);

				if (process.StartTime > this.StartTime)
				{
					// fbuild process already terminated, this is a fake one with its ID reused
					return;
				}

				_fbuildProcess = process;
			}
			catch (ArgumentException)
			{
				// process already terminated, may be a historical build
				return;
			}

			var timer = new Timer(100);

			void TimerTick(object sender, ElapsedEventArgs e)
			{
				if (_fbuildProcess.HasExited)
				{
					_fbuildProcess = null;
					this.OnStopped(DateTime.Now);
					// ReSharper disable AccessToDisposedClosure
					timer.Elapsed -= TimerTick;
					timer.Stop();
					timer.Dispose();
					// ReSharper restore AccessToDisposedClosure
				}
			}

			timer.Elapsed += TimerTick;
			timer.Start();

		}


		public void OnStopped(StopBuildEventArgs e)
		{
			this.OnStopped(e.Time);
		}

		public void OnStopped(DateTime time)
		{
			// give components a last chance to tick
			// we don't do UpdateTimeFromEvent here because Tick will do the same thing
			this.Tick(time);
			this.IsRunning = false;

			var currentTimeOffset = this.ElapsedTime.TotalSeconds;

			// do this before notifying workers, so give JobManager a chance to raise
			// job finish events
			this.JobManager.NotifySessionStopped();

			foreach (var worker in this.Workers)
			{
				worker.OnSessionStopped(currentTimeOffset);
			}

			this.InProgressJobCount = 0;

			this.UpdateActiveWorkerAndCoreCount();
		}

		public void ReportProgress(ReportProgressEventArgs e)
		{
			this.UpdateTimeFromEvent(e);

			this.Progress = e.Progress;
		}

		public void ReportCounter(ReportCounterEventArgs e)
		{
			this.UpdateTimeFromEvent(e);
		}

		private BuildWorkerViewModel EnsureWorker(string hostName)
		{
			if (!_workerMap.TryGetValue(hostName, out var worker))
			{
				worker = new BuildWorkerViewModel(hostName, _workerMap.Count == 0, this);
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
			this.UpdateTimeFromEvent(e);

			var job = this.EnsureWorker(e.HostName).OnJobFinished(e);

			if (job != null)
			{
				var racedJob = this.JobManager.GetJobPotentiallyWonByLocalRace(job);
				if (racedJob != null)
				{
					this.OnJobFinished(FinishJobEventArgs.MakeRacedOut(e.Time, racedJob.OwnerCore.OwnerWorker.HostName, racedJob.EventName));
				}

				this.JobManager.NotifyJobFinished(job);
			}

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
			this.UpdateTimeFromEvent(e);

			var job = this.EnsureWorker(e.HostName).OnJobStarted(e, this.StartTime);
			this.JobManager.Add(job);
			++this.InProgressJobCount;

			if (!this.IsRunning)
			{
				// because of the async nature, this could happen even after the build process 
				// is terminated
				var finishJobEventArgs = FinishJobEventArgs.MakeStopped(_currentTime, e.HostName, e.EventName);
				this.OnJobFinished(finishJobEventArgs);
			}

			this.UpdateActiveWorkerAndCoreCount();
		}


		private void UpdateTimeFromEvent(BuildEventArgs e)
		{
			// this is important for history restoring to keep track of time
			this.CurrentTime = e.Time;
		}

		public void Tick(DateTime now)
		{
			if (!this.IsRunning || this.IsRestoringHistory)
			{
				return;
			}

			this.CurrentTime = now;

			var timeOffset = this.ElapsedTime.TotalSeconds;

			this.JobManager.Tick(timeOffset);

			// called from tick thread
			lock (this.Workers)
			{
				foreach (var worker in this.Workers)
				{
					worker.Tick(timeOffset);
				}
			}

			this.Ticked?.Invoke(this, timeOffset);
		}

		private void DetectDebris()
		{
			// historical build could be interrupted (due to unexpected termination of fbuild process)
			// so no StopBuild event will be triggered. we detect this kind of 'debris' here.

			// because fbuild will output build state routinely (500ms IIRC), so we won't need to worry
			// about long jobs being mishandled in this situation.

			if ((DateTime.Now - this.CurrentTime).TotalSeconds > 10)
			{
				this.OnStopped(this.CurrentTime);
			}
		}
	}
}
