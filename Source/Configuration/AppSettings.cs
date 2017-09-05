using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FastBuild.Dashboard.Configuration
{
	public class AppSettings : SettingsBase
	{
		private const string AppSettingsDomain = "app";

		private static AppSettings _default;
		public static AppSettings Default => _default ?? (_default = SettingsBase.Load<AppSettings>(AppSettingsDomain));

		public override string Domain => AppSettingsDomain;
		public int WorkerCores { get; set; } = -1;
		public int WorkerMode { get; set; } = (int)Services.Worker.WorkerMode.WorkWhenIdle;
		public bool StartWithWindows { get; set; } = true;
		public string ExternalSourceEditorPath { get; set; }
		public string ExternalSourceEditorArgs { get; set; }
		public string ExternalSourceEditorAdditionalArgs { get; set; }
		public string ExternalSourceEditor { get; set; } = "visual-studio";
	}
}
