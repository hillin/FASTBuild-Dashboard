using System;

namespace FastBuild.Dashboard.Services.Build.SourceEditor
{
	[AttributeUsage(AttributeTargets.All, Inherited = false)]
	internal sealed class ExternalSourceEditorAttribute : Attribute
	{
		public string Key { get; }
		public string Name { get; }
		public string Description { get; }
		public bool AllowOverridePath { get; set; } = true;
		public bool AllowSpecifyArgs { get; set; } = false;
		public bool AllowSpecifyAdditionalArgs { get; set; } = true;

		public ExternalSourceEditorAttribute(string key, string name, string description)
		{
			this.Key = key;
			this.Name = name;
			this.Description = description;
		}
		
	}
}
