namespace FastBuild.Dashboard.Communication.Events
{
	internal class StopBuildEventArgs : BuildEventArgs
	{
		public const string StopBuildEventName = "STOP_BUILD";
		public static StopBuildEventArgs Parse(string[] tokens)
		{
			var args = new StopBuildEventArgs();
			BuildEventArgs.ParseBase(tokens, args);
			return args;
		}

	}
}
