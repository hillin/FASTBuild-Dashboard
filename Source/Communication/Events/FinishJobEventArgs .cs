using System;

namespace FastBuild.Dashboard.Communication.Events
{
	internal class FinishJobEventArgs : BuildEventArgs
	{
		public const string FinishJobEventName = "FINISH_JOB";

		public static FinishJobEventArgs Parse(string[] tokens)
		{
			var args = new FinishJobEventArgs();
			BuildEventArgs.ParseBase(tokens, args);
			args.Result = FinishJobEventArgs.ParseBuildJobResult(tokens[EventArgStartIndex]);
			args.HostName = tokens[EventArgStartIndex + 1];
			args.EventName = tokens[EventArgStartIndex + 2];
			if (tokens.Length > EventArgStartIndex + 3)
			{
				args.Message = tokens[EventArgStartIndex + 3];
			}
			return args;
		}

		public static FinishJobEventArgs MakeRacedOut(DateTime time, string hostName, string eventName)
		{
			return new FinishJobEventArgs
			{
				Time = time,
				EventName = eventName,
				HostName = hostName,
				Result = BuildJobStatus.RacedOut
			};
		}

		public static FinishJobEventArgs MakeStopped(DateTime time, string hostName, string eventName)
		{
			return new FinishJobEventArgs
			{
				Time = time,
				EventName = eventName,
				HostName = hostName,
				Result = BuildJobStatus.Stopped
			};
		}

		private static BuildJobStatus ParseBuildJobResult(string result)
		{
			switch (result)
			{
				case "FAILED":
					return BuildJobStatus.Failed;
				case "ERROR":
					return BuildJobStatus.Error;
				case "SUCCESS":
				case "SUCCESS_COMPLETE":
					return BuildJobStatus.Success;
				case "SUCCESS_CACHED":
					return BuildJobStatus.SuccessCached;
				case "SUCCESS_PREPROCESSED":
					return BuildJobStatus.SuccessPreprocessed;
				case "TIMEOUT":
					return BuildJobStatus.Timeout;
				default:
					throw new ArgumentException("unknown build job result", nameof(result));
			}
		}

		public BuildJobStatus Result { get; private set; }
		public string HostName { get; private set; }
		public string EventName { get; private set; }
		public string Message { get; private set; }
	}
}
