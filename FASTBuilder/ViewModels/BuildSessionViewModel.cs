using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Caliburn.Micro;
using FastBuilder.Communication.Events;

namespace FastBuilder.ViewModels
{
	internal class BuildSessionViewModel : Screen
	{
		private bool _isRunning;
		private double _progress;
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

		public bool IsRestoringHistory
		{
			get => _isRestoringHistory;
			set
			{
				if (value == _isRestoringHistory) return;
				_isRestoringHistory = value;
				this.NotifyOfPropertyChange();
				this.NotifyOfPropertyChange(nameof(this.IsSessionViewVisible));
				this.NotifyOfPropertyChange(nameof(this.StatusText));
			}
		}

		public bool IsSessionViewVisible => !this.IsRestoringHistory;

		public bool IsRunning
		{
			get => _isRunning;
			private set
			{
				if (value == _isRunning) return;
				_isRunning = value;
				this.NotifyOfPropertyChange();
				this.NotifyOfPropertyChange(nameof(this.StatusText));
			}
		}

		public double Progress
		{
			get => _progress;
			private set
			{
				if (value.Equals(_progress)) return;
				_progress = value;
				this.NotifyOfPropertyChange();
			}
		}

		public string StatusText
		{
			get
			{
				if (_isRestoringHistory)
					return "Loading";

				if (this.IsRunning)
					return "Building";

				return "Ended";
			}
		}


		private readonly Dictionary<string, BuildWorkerViewModel> _workerMap = new Dictionary<string, BuildWorkerViewModel>();
		private bool _isRestoringHistory;
		private DateTime _currentTime;
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
				worker = new BuildWorkerViewModel(hostName);
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
		}

		public void OnJobStarted(StartJobEventArgs e)
		{
			this.EnsureWorker(e.HostName).OnJobStarted(e, this.StartTime);
		}

		public void Tick(DateTime now)
		{
			if (!this.IsRunning)
				return;

			this.CurrentTime = now;

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
