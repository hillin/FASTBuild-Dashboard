using System;

namespace FastBuild.Dashboard.Services.Worker
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
