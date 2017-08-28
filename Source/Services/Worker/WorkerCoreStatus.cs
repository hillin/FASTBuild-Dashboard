namespace FastBuild.Dashboard.Services.Worker
{
	internal class WorkerCoreStatus
	{
		public WorkerCoreState State { get; }
		public string HostHelping { get; }
		public string WorkingItem { get; }
		public WorkerCoreStatus(WorkerCoreState state, string hostHelping = null, string workingItem = null)
		{
			this.State = state;
			this.HostHelping = hostHelping;
			this.WorkingItem = workingItem;
		}
	}
}
