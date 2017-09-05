using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FastBuild.Dashboard.Configuration
{
	internal static class SettingsExtensions
	{
		public static void Save(this SettingsBase settings) 
			=> SettingsBase.Save(settings);
	}
}
