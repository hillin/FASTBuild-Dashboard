using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FastBuild.Dashboard.Communication.Events;

namespace FastBuild.Dashboard.Communication
{
	internal class BuildWatcher
	{
		private readonly LogWatcher _logWatcher;

		private static string[] Tokenize(string message)
		{
			return Regex.Matches(message, @"[\""].+?[\""]|[^ ]+")
				.Cast<Match>()
				.Select(m => m.Value.Trim('"'))
				.ToArray();
		}

		private static T ParseEventArgs<T>(string[] tokens)
			where T : BuildEventArgs
		{
			try
			{
				return (T)typeof(T).GetMethod("Parse", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { tokens });
			}
			catch (Exception e)
			{
				throw new ParseException(string.Empty, e);
			}
		}

		public event EventHandler HistoryRestorationStarted;
		public event EventHandler HistoryRestorationEnded;
		public event EventHandler<StartBuildEventArgs> SessionStarted;
		public event EventHandler<StopBuildEventArgs> SessionStopped;
		public event EventHandler<ReportCounterEventArgs> ReportCounter;
		public event EventHandler<ReportProgressEventArgs> ReportProgress;
		public event EventHandler<StartJobEventArgs> JobStarted;
		public event EventHandler<FinishJobEventArgs> JobFinished;

		public bool IsRestoringHistory => _logWatcher.IsRestoringHistory;

		public DateTime LastMessageTime { get; private set; }

		public BuildWatcher()
		{
			_logWatcher = new LogWatcher();
			_logWatcher.HistoryRestorationStarted += this.LogWatcher_HistoryRestorationStarted;
			_logWatcher.HistoryRestorationEnded += this.LogWatcher_HistoryRestorationEnded;
			_logWatcher.LogReceived += this.LogWatcher_LogReceived;
			_logWatcher.LogReset += this.LogWatcher_LogReset;
		}

		private void LogWatcher_LogReset(object sender, EventArgs e)
		{

		}

		private void LogWatcher_HistoryRestorationStarted(object sender, EventArgs e)
		{
			this.HistoryRestorationStarted?.Invoke(this, EventArgs.Empty);
		}

		private void LogWatcher_HistoryRestorationEnded(object sender, EventArgs e)
		{
			this.HistoryRestorationEnded?.Invoke(this, EventArgs.Empty);
		}

		public void Start()
		{
			_logWatcher.Start();
		}

		private void LogWatcher_LogReceived(object sender, string e)
		{
			this.ProcessLog(e);
		}

		private void IgnoreLog(string message)
		{
			// todo: log this message
		}

		private T ReceiveEvent<T>(string[] tokens)
			where T : BuildEventArgs
		{
			var args = BuildWatcher.ParseEventArgs<T>(tokens);
			this.LastMessageTime = args.Time;
			return args;
		}

		private void ProcessLog(string message)
		{
			var tokens = BuildWatcher.Tokenize(message);

			if (tokens.Length < 2)
			{
				this.IgnoreLog(message);
				return;
			}

			try
			{
				switch (tokens[BuildEventArgs.EventTypeArgIndex])
				{
					case StartBuildEventArgs.StartBuildEventName:
						this.SessionStarted?.Invoke(this, this.ReceiveEvent<StartBuildEventArgs>(tokens));
						break;
					case StopBuildEventArgs.StopBuildEventName:
						this.SessionStopped?.Invoke(this, this.ReceiveEvent<StopBuildEventArgs>(tokens));
						break;
					case StartJobEventArgs.StartJobEventName:
						this.JobStarted?.Invoke(this, this.ReceiveEvent<StartJobEventArgs>(tokens));
						break;
					case FinishJobEventArgs.FinishJobEventName:
						this.JobFinished?.Invoke(this, this.ReceiveEvent<FinishJobEventArgs>(tokens));
						break;
					case ReportProgressEventArgs.ReportProgressEventName:
						this.ReportProgress?.Invoke(this, this.ReceiveEvent<ReportProgressEventArgs>(tokens));
						break;
					case ReportCounterEventArgs.ReportCounterEventName:
						this.ReportCounter?.Invoke(this, this.ReceiveEvent<ReportCounterEventArgs>(tokens));
						break;
					default:
						this.IgnoreLog(message);
						break;
				}
			}
			catch (ParseException)
			{
				this.IgnoreLog(message);
			}
		}

	}
}
