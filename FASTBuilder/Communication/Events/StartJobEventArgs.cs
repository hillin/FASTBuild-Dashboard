using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuilder.Communication.Events
{
	internal class StartJobEventArgs : BuildEventArgs
	{
		public static StartJobEventArgs Parse(string[] tokens)
		{
			var args = new StartJobEventArgs();
			BuildEventArgs.ParseBase(tokens, args);
			args.HostName = tokens[EventArgStartIndex ];
			args.EventName = tokens[EventArgStartIndex + 1];
			return args;
		}

		public string HostName { get; private set; }
		public string EventName { get; private set; }

	}
}
