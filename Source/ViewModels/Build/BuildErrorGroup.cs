using System.Collections.Generic;

namespace FastBuild.Dashboard.ViewModels.Build
{
	internal class BuildErrorGroup
	{
		public string FilePath { get; }
		public IEnumerable<BuildErrorInfo> Errors { get; }

		public BuildErrorGroup(string fileName, IEnumerable<BuildErrorInfo> errors)
		{
			this.FilePath = fileName;
			this.Errors = errors;
		}
	}
}
