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
		public BindableCollection <BuildCoreViewModel> Cores { get; } = new BindableCollection <BuildCoreViewModel>();

		public BuildWorkerViewModel(string hostName)
		{
			this.HostName = hostName;
		}

		public void OnJobFinished(FinishJobEventArgs e)
		{
			var core = this.Cores.FirstOrDefault(c => c.CurrentJob != null && c.CurrentJob.EventName == e.EventName);
			core?.OnJobFinished(e);
		}

		public void OnJobStarted(StartJobEventArgs e, DateTime sessionStartTime)
		{
			var core = this.Cores.FirstOrDefault(c => !c.IsBusy);
			if (core == null)
			{
				core = new BuildCoreViewModel(this.Cores.Count);

				// called from log watcher thread
				lock (this.Cores)
				{
					this.Cores.Add(core);
				}
			}

			core.OnJobStarted(e, sessionStartTime);
		}

		public void OnSessionStopped(DateTime time)
		{
			foreach (var core in this.Cores)
			{
				core.OnSessionStopped(time);
			}
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
