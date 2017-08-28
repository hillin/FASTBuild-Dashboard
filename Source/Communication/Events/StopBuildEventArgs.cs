namespace FastBuild.Dashboard.Communication.Events
{
	internal class StopBuildEventArgs : BuildEventArgs
	{
		public static StopBuildEventArgs Parse(string[] tokens)
		{
			var args = new StopBuildEventArgs();
			BuildEventArgs.ParseBase(tokens, args);
			return args;
		}

	}
}
