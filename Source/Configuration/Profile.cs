using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using FastBuild.Dashboard.Services.Build;

namespace FastBuild.Dashboard.Configuration
{
	public class Profile : SettingsBase
	{
		private const string ProfileDomain = "profile";

		private static Profile _default;
		public static Profile Default => _default ?? (_default = SettingsBase.Load<Profile>(ProfileDomain));

		public override string Domain => ProfileDomain;

		public int WindowLeft { get; set; }
		public int WindowTop { get; set; }
		public int WindowWidth { get; set; } = 800;
		public int WindowHeight { get; set; } = 600;
		public bool IsFirstRun { get; set; } = true;
		public WindowState WindowState { get; set; } = WindowState.Normal;
		public int BuildJobDisplayMode { get; set; } = (int)Services.Build.BuildJobDisplayMode.Standard;
	}
}
