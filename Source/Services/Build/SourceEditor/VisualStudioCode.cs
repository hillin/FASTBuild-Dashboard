using System;
using System.Diagnostics;
using System.Text;
using FastBuild.Dashboard.Configuration;

namespace FastBuild.Dashboard.Services.Build.SourceEditor
{
	[ExternalSourceEditor(
		"visual-studio-code", 
		"Visual Studio Code",
		"Open source file with Visual Studio Code",
		AllowOverridePath = true, 
		AllowSpecifyAdditionalArgs = true, 
		AllowSpecifyArgs = false)]
	internal class VisualStudioCode : ExternalSourceEditorBase
	{
		private const string ExecutablePath = @"Microsoft VS Code\Code.exe";

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
			argsBuilder.Append("-g ").Append('"').Append(file).Append('"');

			if (lineNumber > 0)
			{
				argsBuilder.Append(':').Append(lineNumber);
			}

			if (!string.IsNullOrWhiteSpace(AppSettings.Default.ExternalSourceEditorAdditionalArgs))
			{
				argsBuilder.Append(' ').Append(AppSettings.Default.ExternalSourceEditorAdditionalArgs);
			}

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
