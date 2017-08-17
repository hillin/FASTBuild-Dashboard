using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuilder.Services.Worker
{
	internal interface IWorkerAgent
	{
		event EventHandler<WorkerRunStateChangedEventArgs> WorkerRunStateChanged;
		bool IsRunning { get; }
		void SetCoreCount(int coreCount);
		void SetWorkerMode(WorkerMode mode);
		void Initialize();
		WorkerCoreStatus[] GetStatus();
	}
}
