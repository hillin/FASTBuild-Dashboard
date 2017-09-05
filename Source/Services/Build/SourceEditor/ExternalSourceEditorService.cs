using System;
using System.Collections.Generic;
using System.Reflection;
using FastBuild.Dashboard.Configuration;

namespace FastBuild.Dashboard.Services.Build.SourceEditor
{
	internal class ExternalSourceEditorService : IExternalSourceEditorService
	{
		private static readonly Dictionary<string, ExternalSourceEditorMetadata> ExternalSourceEditorsMap
			= new Dictionary<string, ExternalSourceEditorMetadata>();

		private static readonly ExternalSourceEditorMetadata DefaultExternalSourceEditor;

		static ExternalSourceEditorService()
		{
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
			{
				var attribute = type.GetCustomAttribute<ExternalSourceEditorAttribute>();
				if (attribute == null)
				{
					continue;
				}

				var metadata = new ExternalSourceEditorMetadata(
					type,
					attribute.Key,
					attribute.Name,
					attribute.Description,
					attribute.AllowOverridePath,
					attribute.AllowSpecifyArgs,
					attribute.AllowSpecifyAdditionalArgs);

				ExternalSourceEditorsMap.Add(metadata.Key, metadata);

				if (metadata.Key == "default")
				{
					DefaultExternalSourceEditor = metadata;
				}
			}
		}

		// todo: maybe cache and reuse instances
		private static IExternalSourceEditor CreateEditorInstance(ExternalSourceEditorMetadata editorMetadata)
			=> (IExternalSourceEditor)Activator.CreateInstance(editorMetadata.Type);

		public IEnumerable<ExternalSourceEditorMetadata> ExternalSourceEditors => ExternalSourceEditorsMap.Values;

		public ExternalSourceEditorMetadata SelectedEditor
		{
			get => ExternalSourceEditorsMap.TryGetValue(AppSettings.Default.ExternalSourceEditor, out var editor)
				? editor
				: DefaultExternalSourceEditor;
			set
			{
				AppSettings.Default.ExternalSourceEditor = value == null ? "default" : value.Key;
				AppSettings.Default.Save();
			}
		}

		public bool IsSelectedEditorAvailable
			=> ExternalSourceEditorService.CreateEditorInstance(this.SelectedEditor).IsAvailable;

		public bool OpenFile(string file, int lineNumber, int initiatorProcessId)
			=> ExternalSourceEditorService.CreateEditorInstance(this.SelectedEditor).OpenFile(file, lineNumber, initiatorProcessId);

	}
}