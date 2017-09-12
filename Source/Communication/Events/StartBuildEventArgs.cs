namespace FastBuild.Dashboard.Communication.Events
{
	internal class StartBuildEventArgs : BuildEventArgs
	{
		public const string StartBuildEventName = "START_BUILD";
		public static StartBuildEventArgs Parse(string[] tokens)
		{
			var args = new StartBuildEventArgs();
			BuildEventArgs.ParseBase(tokens, args);
			args.LogVersion = int.Parse(tokens[EventArgStartIndex]);
			args.ProcessId = int.Parse(tokens[EventArgStartIndex + 1]);
			return args;
		}

		public int LogVersion { get; private set; }
		public int ProcessId { get; private set; }

	}
}
