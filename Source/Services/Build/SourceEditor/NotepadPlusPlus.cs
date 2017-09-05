using System;
using System.Diagnostics;
using System.Text;
using FastBuild.Dashboard.Configuration;

namespace FastBuild.Dashboard.Services.Build.SourceEditor
{
	[ExternalSourceEditor(
		"notepad-plus-plus",
		"Notepad++",
		"Open source file with Notepad++",
		AllowOverridePath = true,
		AllowSpecifyAdditionalArgs = true,
		AllowSpecifyArgs = false)]
	internal class NotepadPlusPlus : ExternalSourceEditorBase
	{
		private const string ExecutablePath = @"Notepad++\notepad++.exe";

		public override bool IsAvailable 
			=> !string.IsNullOrEmpty(ExternalSourceEditorBase.GetEditorExecutable(ExecutablePath));

		public override bool OpenFile(string file, int lineNumber, int initiatorProcessId)
		{
			var executable = ExternalSourceEditorBase.GetEditorExecutable(ExecutablePath);
			if (executable == null)
			{
				return false;
			}

			var argsBuilder = new StringBuilder();

			if (lineNumber > 0)
			{
				argsBuilder.Append("-n").Append(lineNumber);
			}

			if (!string.IsNullOrWhiteSpace(AppSettings.Default.ExternalSourceEditorAdditionalArgs))
			{
				argsBuilder.Append(' ').Append(AppSettings.Default.ExternalSourceEditorAdditionalArgs);
			}

			argsBuilder.Append(' ').Append('"').Append(file).Append('"');

			try
			{
				return Process.Start(executable, argsBuilder.ToString()) != null;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
