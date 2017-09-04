using System;
using System.Timers;
using System.Windows.Shell;
using Caliburn.Micro;
using FastBuild.Dashboard.Communication;
using FastBuild.Dashboard.Communication.Events;

namespace FastBuild.Dashboard.ViewModels.Build
{
	internal sealed class BuildWatcherViewModel : Conductor<BuildSessionViewModel>.Collection.OneActive, IMainPage
	{
		private BuildSessionViewModel _currentSession;
		private readonly BuildWatcher _watcher;
		private TaskbarItemProgressState _taskbarProgressState;
		private double _taskbarProgressValue;

		public event EventHandler<bool> WorkingStateChanged;

		public BuildSessionViewModel CurrentSession
		{
			get => _currentSession;
			private set
			{
				if (object.Equals(value, _currentSession))
				{
					return;
				}

				_currentSession = value;
				this.NotifyOfPropertyChange();
			}
		}

		public TaskbarItemProgressState TaskbarProgressState
		{
			get => _taskbarProgressState;
			private set
			{
				if (value == _taskbarProgressState)
				{
					return;
				}

				_taskbarProgressState = value;
				this.NotifyOfPropertyChange();
			}
		}

		public double TaskbarProgressValue
		{
			get => _taskbarProgressValue;
			private set
			{
				if (value.Equals(_taskbarProgressValue))
				{
					return;
				}

				_taskbarProgressValue = value;
				this.NotifyOfPropertyChange();
			}
		}

		public string Icon => "HeartPulse";

		public BuildWatcherViewModel()
		{
			this.DisplayName = "Build";

			_watcher = new BuildWatcher();
			_watcher.HistoryRestorationStarted += this.Watcher_HistoryRestorationStarted;
			_watcher.HistoryRestorationEnded += this.Watcher_HistoryRestorationEnded;
			_watcher.JobStarted += this.Watcher_JobStarted;
			_watcher.JobFinished += this.Watcher_JobFinished;
			_watcher.ReportCounter += this.Watcher_ReportCounter;
			_watcher.ReportProgress += this.Watcher_ReportProgress;
			_watcher.SessionStopped += this.Watcher_SessionStopped;
			_watcher.SessionStarted += this.Watcher_SessionStarted;
			_watcher.Start();

			var tickTimer = new Timer(100);
			tickTimer.Elapsed += this.TickTimer_Elapsed;
			tickTimer.Start();
		}

		private void TickTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			// ReSharper disable once UseNullPropagation
			if (this.CurrentSession == null)
			{
				return;
			}

			// note we only need to tick current session

			// for sessions which are restoring history, don't use the real time so very long job graphs can be prevented
			this.CurrentSession.Tick(this.CurrentSession.IsRestoringHistory ? _watcher.LastMessageTime : DateTime.Now);
		}

		private void Watcher_HistoryRestorationEnded(object sender, EventArgs e)
		{
			if (this.CurrentSession != null)
			{
				this.CurrentSession.IsRestoringHistory = false;
			}
		}

		private void Watcher_HistoryRestorationStarted(object sender, EventArgs e)
		{
			if (this.CurrentSession != null)
			{
				this.CurrentSession.IsRestoringHistory = true;
			}
		}

		private void EnsureCurrentSession()
		{
			if (this.CurrentSession != null)
			{
				return;
			}

			this.CurrentSession = new BuildSessionViewModel
			{
				IsRestoringHistory = _watcher.IsRestoringHistory
			};

			// called from log watcher thread
			lock (this.Items)
			{
				this.Items.Add(this.CurrentSession);
			}
		}

		private void Watcher_SessionStarted(object sender, StartBuildEventArgs e)
		{
			this.CurrentSession?.OnStopped(DateTime.Now);

			this.CurrentSession = new BuildSessionViewModel(e)
			{
				IsRestoringHistory = _watcher.IsRestoringHistory
			};

			this.Items.Add(this.CurrentSession);
			this.ActivateItem(this.CurrentSession);

			this.TaskbarProgressState = TaskbarItemProgressState.Indeterminate;
			this.WorkingStateChanged?.Invoke(this, true);
		}

		private void Watcher_SessionStopped(object sender, StopBuildEventArgs e)
		{
			this.CurrentSession?.OnStopped(e);
			this.TaskbarProgressState = TaskbarItemProgressState.None;
			this.WorkingStateChanged?.Invoke(this, false);
		}

		private void Watcher_ReportProgress(object sender, ReportProgressEventArgs e)
		{
			this.EnsureCurrentSession();
			this.CurrentSession.ReportProgress(e);
			this.TaskbarProgressState = TaskbarItemProgressState.Normal;
			this.TaskbarProgressValue = e.Progress / 100.0;
		}

		private void Watcher_ReportCounter(object sender, ReportCounterEventArgs e)
		{
			this.EnsureCurrentSession();
			this.CurrentSession.ReportCounter(e);
		}

		private void Watcher_JobFinished(object sender, FinishJobEventArgs e)
		{
			this.EnsureCurrentSession();
			this.CurrentSession.OnJobFinished(e);
		}

		private void Watcher_JobStarted(object sender, StartJobEventArgs e)
		{
			this.EnsureCurrentSession();
			this.CurrentSession.OnJobStarted(e);
		}

	}
}
