using System.Linq;
using System.Timers;
using Caliburn.Micro;
using FastBuild.Dashboard.Services.Worker;

namespace FastBuild.Dashboard.ViewModels.Worker
{
	internal class WorkerViewModel : PropertyChangedBase, IMainPage
	{
		private readonly IWorkerAgentService _workerAgentService;
		private string _workerErrorMessage;
		private string _statusTitle;
		public string DisplayName => "Worker";

		public bool IsWorkerRunning => _workerAgentService.IsRunning;

		public string Icon => "Worker";

		public string WorkerErrorMessage
		{
			get => _workerErrorMessage;
			private set
			{
				if (value == _workerErrorMessage)
				{
					return;
				}

				_workerErrorMessage = value;
				this.NotifyOfPropertyChange();
			}
		}

		public string StatusTitle
		{
			get => _statusTitle;
			private set
			{
				if (value == _statusTitle)
				{
					return;
				}

				_statusTitle = value;
				this.NotifyOfPropertyChange();
			}
		}

		public BindableCollection<WorkerCoreStatusViewModel> CoreStatuses { get; }
			= new BindableCollection<WorkerCoreStatusViewModel>();

		private bool _isTicking;

		public WorkerViewModel()
		{
			this.StatusTitle = "Preparing...";

			_workerAgentService = IoC.Get<IWorkerAgentService>();
			_workerAgentService.WorkerRunStateChanged += this.WorkerAgentService_WorkerRunStateChanged;
			_workerAgentService.Initialize();

			var tickTimer = new Timer(500)
			{
				AutoReset = true
			};
			tickTimer.Elapsed += this.Tick;
			tickTimer.Start();
		}

		private void Tick(object sender, ElapsedEventArgs e)
		{
			if (!this.IsWorkerRunning)
			{
				return;
			}

			if (_isTicking)
			{
				return; 
			}

			_isTicking = true;

			var statuses = _workerAgentService.GetStatus();

			for (var i = this.CoreStatuses.Count - 1; i > statuses.Length; --i)
			{
				this.CoreStatuses.RemoveAt(i);
			}

			for (var i = this.CoreStatuses.Count; i < statuses.Length; ++i)
			{
				this.CoreStatuses.Add(new WorkerCoreStatusViewModel(i));
			}

			for (var i = 0; i < this.CoreStatuses.Count; ++i)
			{
				this.CoreStatuses[i].UpdateStatus(statuses[i]);
			}

			if (statuses.All(s => s.State == WorkerCoreState.Disabled))
			{
				this.StatusTitle = "Disabled";
			}
			else if (statuses.Any(s => s.State == WorkerCoreState.Working))
			{
				this.StatusTitle = "Working";
			}
			else
			{
				this.StatusTitle = "Idle";
			}

			_isTicking = false;
		}

		private void WorkerAgentService_WorkerRunStateChanged(object sender, WorkerRunStateChangedEventArgs e)
		{
			this.NotifyOfPropertyChange(nameof(this.IsWorkerRunning));
			this.WorkerErrorMessage = e.ErrorMessage;

			if (!e.IsRunning)
			{
				this.StatusTitle = "Worker Error";
			}
		}
	}
}
