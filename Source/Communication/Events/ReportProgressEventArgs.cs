using System.Globalization;

namespace FastBuild.Dashboard.Communication.Events
{
	internal class ReportProgressEventArgs : BuildEventArgs
	{
		public const string ReportProgressEventName = "PROGRESS_STATUS";

		public static ReportProgressEventArgs Parse(string[] tokens)
		{
			var args = new ReportProgressEventArgs();
			BuildEventArgs.ParseBase(tokens, args);
			args.Progress = float.Parse(tokens[EventArgStartIndex], CultureInfo.InvariantCulture);
			return args;
		}

		public double Progress { get; private set; }
	}
}
