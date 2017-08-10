using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuilder.Communication.Events
{
	internal class ReportProgressEventArgs : BuildEventArgs
	{
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
