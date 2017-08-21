using System;

namespace FastBuilder.Services.Worker
{
	internal class WorkerAgentService : IWorkerAgentService
	{
		private AppSettings CurrentAppSettings => AppSettings.Default;

		public int WorkerCores
		{
			get
			{
				var cores = this.CurrentAppSettings.WorkerCores;
				if (cores <= 0)
				{
					cores = Environment.ProcessorCount / 2;
				}

				return cores;
			}
			set
			{
				this.CurrentAppSettings.WorkerCores = value;
				this.CurrentAppSettings.Save();

				if (_workerAgent.IsRunning)
				{
					_workerAgent.SetCoreCount(this.WorkerCores);
				}
			}
		}

		public WorkerMode WorkerMode
		{
			get => (WorkerMode)this.CurrentAppSettings.WorkerMode;
			set
			{
				this.CurrentAppSettings.WorkerMode = (int)value;
				this.CurrentAppSettings.Save();

				if (_workerAgent.IsRunning)
				{
					_workerAgent.SetWorkerMode(this.WorkerMode);
				}
			}
		}

		public bool IsRunning => _workerAgent.IsRunning;
		public event EventHandler<WorkerRunStateChangedEventArgs> WorkerRunStateChanged;

		private readonly IWorkerAgent _workerAgent;

		public WorkerAgentService()
		{
			_workerAgent = new ExternalWorkerAgent();
			_workerAgent.WorkerRunStateChanged += this.WorkerAgent_WorkerRunStateChanged;
		}

		private void WorkerAgent_WorkerRunStateChanged(object sender, WorkerRunStateChangedEventArgs e)
			=> this.WorkerRunStateChanged?.Invoke(this, e);

		public void Initialize()
		{
			_workerAgent.Initialize();
			if (_workerAgent.IsRunning)
			{
				_workerAgent.SetCoreCount(this.WorkerCores);
				_workerAgent.SetWorkerMode(this.WorkerMode);
			}
		}

		public WorkerCoreStatus[] GetStatus() => _workerAgent.GetStatus();
	}
}