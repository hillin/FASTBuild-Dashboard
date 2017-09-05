using System.Collections.Generic;

namespace FastBuild.Dashboard.Services.Build.SourceEditor
{
	internal interface IExternalSourceEditorService
	{
		IEnumerable<ExternalSourceEditorMetadata> ExternalSourceEditors { get; }
		ExternalSourceEditorMetadata SelectedEditor { get; set; }
		bool IsSelectedEditorAvailable { get; }
		bool OpenFile(string file, int lineNumber, int initiatorProcessId);
	}
}
