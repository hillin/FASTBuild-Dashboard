using System;
using System.Diagnostics;

namespace FastBuild.Dashboard.Services.Build.SourceEditor
{
	[ExternalSourceEditor(
		"default",
		"Default",
		"Open source file with system default associated editor.",
		AllowOverridePath = false,
		AllowSpecifyAdditionalArgs = false,
		AllowSpecifyArgs = false)]
	internal class DefaultExternalSourceEditor : IExternalSourceEditor
	{
		public bool IsAvailable => true;

		public bool OpenFile(string file, int lineNumber, int initiatorProcessId)
		{
			try
			{
				return Process.Start(file) != null;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
