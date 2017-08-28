using System;
using System.Diagnostics;
using System.Linq;
using Caliburn.Micro;
using FastBuild.Dashboard.Communication.Events;

namespace FastBuild.Dashboard.ViewModels.Build
{
	[DebuggerDisplay("Worker:{" + nameof(BuildWorkerViewModel.HostName) + "}")]
	internal class BuildWorkerViewModel : PropertyChangedBase
	{
		public string HostName { get; }
		public bool IsLocal { get; }
		public BuildSessionViewModel OwnerSession { get; }
		public BindableCollection <BuildCoreViewModel> Cores { get; } = new BindableCollection <BuildCoreViewModel>();

		public int ActiveCoreCount { get; private set; }

		public BuildWorkerViewModel(string hostName, bool isLocal, BuildSessionViewModel ownerSession)
		{
			this.HostName = hostName;
			this.IsLocal = isLocal;
			this.OwnerSession = ownerSession;
		}

		public BuildJobViewModel OnJobFinished(FinishJobEventArgs e)
		{
			var core = this.Cores.FirstOrDefault(c => c.CurrentJob != null && c.CurrentJob.EventName == e.EventName);
			var job = core?.OnJobFinished(e);

			this.UpdateActiveCoreCount();

			return job;
		}

		public BuildJobViewModel OnJobStarted(StartJobEventArgs e, DateTime sessionStartTime)
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

			var job = core.OnJobStarted(e, sessionStartTime);

			this.UpdateActiveCoreCount();

			return job;
		}

		private void UpdateActiveCoreCount()
		{
			this.ActiveCoreCount = this.Cores.Count(c => c.IsBusy);
		}

		public void OnSessionStopped(double currentTimeOffset)
		{
			foreach (var core in this.Cores)
			{
				core.OnSessionStopped(currentTimeOffset);
			}

			this.ActiveCoreCount = 0;
		}

		public void Tick(double currentTimeOffset)
		{
			// called from tick thread
			lock (this.Cores)
			{
				foreach (var core in this.Cores)
				{
					core.Tick(currentTimeOffset);
				}
			}
		}
	}
}
