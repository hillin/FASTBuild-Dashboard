using System;

namespace FastBuild.Dashboard.Communication.Events
{
	internal abstract class BuildEventArgs : EventArgs
	{
		public const int TimeArgIndex = 0;
		public const int EventTypeArgIndex = 1;
		public const int EventArgStartIndex = 2;

		protected static DateTime ParseTime(string timeStamp)
		{
			return DateTime.FromFileTime(long.Parse(timeStamp));
		}

		protected static void ParseBase(string[] tokens, BuildEventArgs args)
		{
			args.Time = BuildEventArgs.ParseTime(tokens[TimeArgIndex]);
		}

		public DateTime Time { get; protected set; }
	}
}
