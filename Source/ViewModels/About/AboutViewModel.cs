using System.Reflection;
using Caliburn.Micro;

namespace FastBuild.Dashboard.ViewModels.About
{
	internal class AboutViewModel : PropertyChangedBase, IMainPage
	{
		public string Icon => "InformationOutline";
		public string DisplayName => "About";

		public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
	}
}
