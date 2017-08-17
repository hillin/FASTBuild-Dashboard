using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FastBuilder.Services.Worker;
using FASTBuilder;

namespace FastBuilder.ViewModels.Worker
{
	internal class WorkerViewModel : PropertyChangedBase, IMainPage
	{
		private readonly IWorkerAgentService _workerAgentService;
		private string _workerErrorMessage;
		public string DisplayName => "Worker";

		public bool IsWorkerRunning => _workerAgentService.IsRunning;

		public string WorkerErrorMessage
		{
			get => _workerErrorMessage;
			private set
			{
				if (value == _workerErrorMessage)
					return;
				_workerErrorMessage = value;
				this.NotifyOfPropertyChange();
			}
		}

		public WorkerViewModel()
		{
			_workerAgentService = IoC.Get<IWorkerAgentService>();
			_workerAgentService.WorkerRunStateChanged += WorkerAgentService_WorkerRunStateChanged;
			_workerAgentService.Initialize();

			var status = _workerAgentService.GetStatus();
		}

		private void WorkerAgentService_WorkerRunStateChanged(object sender, WorkerRunStateChangedEventArgs e)
		{
			this.NotifyOfPropertyChange(nameof(this.IsWorkerRunning));
			this.WorkerErrorMessage = e.ErrorMessage;
		}
	}
}
