using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuild.Dashboard.Services.Build.SourceEditor
{
	internal interface IExternalSourceEditorService
	{
		IEnumerable<ExternalSourceEditorMetadata> ExternalSourceEditors { get; }
		ExternalSourceEditorMetadata SelectedEditor { get; set; }
		bool IsSelectedEditorAvailable { get; }
		bool OpenFile(string file, int lineNumber);
	}
}
