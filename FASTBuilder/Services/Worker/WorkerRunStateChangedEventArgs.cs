using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuilder.Services.Worker
{
	internal class WorkerRunStateChangedEventArgs : EventArgs
	{
		public bool IsRunning { get; }
		public string ErrorMessage { get; }
		public WorkerRunStateChangedEventArgs(bool isRunning, string errorMessage)
		{
			this.IsRunning = isRunning;
			this.ErrorMessage = errorMessage;
		}

	}
}
