using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuilder.Communication.Events
{
	internal class StartBuildEventArgs : BuildEventArgs
	{
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
