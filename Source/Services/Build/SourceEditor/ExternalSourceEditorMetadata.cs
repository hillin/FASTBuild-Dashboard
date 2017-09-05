using System;

namespace FastBuild.Dashboard.Services.Build.SourceEditor
{
	internal class ExternalSourceEditorMetadata
	{
		public string Key { get; }
		public string Name { get; }
		public string Description { get; }
		public bool AllowOverridePath { get; }
		public bool AllowSpecifyArgs { get; }
		public bool AllowSpecifyAdditionalArgs { get; }
		public Type Type { get; }

		public ExternalSourceEditorMetadata(Type type, string key, string name, string description, bool allowOverridePath, bool allowSpecifyArgs, bool allowSpecifyAdditionalArgs)
		{
			this.Name = name;
			this.Description = description;
			this.AllowOverridePath = allowOverridePath;
			this.AllowSpecifyArgs = allowSpecifyArgs;
			this.AllowSpecifyAdditionalArgs = allowSpecifyAdditionalArgs;
			this.Key = key;
			this.Type = type;
		}

	}
}
