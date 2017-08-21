using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FastBuilder.ViewModels.About
{
	internal class AboutViewModel : PropertyChangedBase, IMainPage
	{
		public string Icon => "InformationOutline";
		public string DisplayName => "About";

		public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
	}
}
