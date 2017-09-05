using System;
using System.IO;
using FastBuild.Dashboard.Configuration;

namespace FastBuild.Dashboard.Services.Build.SourceEditor
{
	internal abstract class ExternalSourceEditorBase : IExternalSourceEditor
	{
		protected static string GetEditorExecutable(string pathInProgramFiles)
		{
			if (!string.IsNullOrWhiteSpace(AppSettings.Default.ExternalSourceEditorPath)
				&& File.Exists(AppSettings.Default.ExternalSourceEditorPath))
			{
				return AppSettings.Default.ExternalSourceEditorPath;
			}

			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), pathInProgramFiles);

			if (File.Exists(path))
			{
				return path;
			}

			path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), pathInProgramFiles);

			if (File.Exists(path))
			{
				return path;
			}

			return null;
		}

		public abstract bool IsAvailable { get; }
		public abstract bool OpenFile(string file, int lineNumber, int initiatorProcessId);
	}
}
