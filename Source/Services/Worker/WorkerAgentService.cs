using System;
using FastBuild.Dashboard.Configuration;

namespace FastBuild.Dashboard.Services.Worker
{
	internal class WorkerAgentService : IWorkerAgentService
	{
		public int WorkerCores
		{
			get
			{
				var cores = AppSettings.Default.WorkerCores;
				if (cores <= 0)
				{
					cores = Environment.ProcessorCount / 2;
				}

				return cores;
			}
			set
			{
				AppSettings.Default.WorkerCores = value;
				AppSettings.Default.Save();

				if (_workerAgent.IsRunning)
				{
					_workerAgent.SetCoreCount(this.WorkerCores);
				}
			}
		}

		public WorkerMode WorkerMode
		{
			get => (WorkerMode)AppSettings.Default.WorkerMode;
			set
			{
				AppSettings.Default.WorkerMode = (int)value;
				AppSettings.Default.Save();

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