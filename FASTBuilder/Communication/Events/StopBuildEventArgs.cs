using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuilder.Communication.Events
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
