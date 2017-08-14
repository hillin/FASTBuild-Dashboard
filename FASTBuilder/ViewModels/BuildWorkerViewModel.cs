using System;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.Linq;
using FastBuilder.Communication.Events;

namespace FastBuilder.ViewModels
{
	internal class BuildWorkerViewModel : PropertyChangedBase
	{
		public string HostName { get; }
		public BuildSessionViewModel OwnerSession { get; }
		public BindableCollection <BuildCoreViewModel> Cores { get; } = new BindableCollection <BuildCoreViewModel>();

		public int ActiveCoreCount { get; private set; }

		public BuildWorkerViewModel(string hostName, BuildSessionViewModel ownerSession)
		{
			this.HostName = hostName;
			this.OwnerSession = ownerSession;
		}

		public void OnJobFinished(FinishJobEventArgs e)
		{
			var core = this.Cores.FirstOrDefault(c => c.CurrentJob != null && c.CurrentJob.EventName == e.EventName);
			core?.OnJobFinished(e);

			this.UpdateActiveCoreCount();
		}

		public void OnJobStarted(StartJobEventArgs e, DateTime sessionStartTime)
		{
			var core = this.Cores.FirstOrDefault(c => !c.IsBusy);
			if (core == null)
			{
				core = new BuildCoreViewModel(this.Cores.Count, this);

				// called from log watcher thread
				lock (this.Cores)
				{
					this.Cores.Add(core);
				}
			}

			core.OnJobStarted(e, sessionStartTime);

			this.UpdateActiveCoreCount();
		}

		private void UpdateActiveCoreCount()
		{
			this.ActiveCoreCount = this.Cores.Count(c => c.IsBusy);
		}

		public void OnSessionStopped(DateTime time)
		{
			foreach (var core in this.Cores)
			{
				core.OnSessionStopped(time);
			}

			this.ActiveCoreCount = 0;
		}

		public void Tick(DateTime now)
		{
			// called from tick thread
			lock (this.Cores)
			{
				foreach (var core in this.Cores)
				{
					core.Tick(now);
				}
			}
		}
	}
}
