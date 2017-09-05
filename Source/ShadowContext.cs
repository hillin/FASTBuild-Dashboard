using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using FastBuild.Dashboard.Configuration;
using Newtonsoft.Json;

namespace FastBuild.Dashboard
{
	public class ShadowContext
	{
		public const string ShadowContextExtension = ".context.json";

		public static ShadowContext Load()
		{
			var contextPath = Path.ChangeExtension(Assembly.GetEntryAssembly().Location, ShadowContextExtension);
			var context = JsonConvert.DeserializeObject<ShadowContext>(File.ReadAllText(contextPath));
#if !DEBUG_SHADOW_CONTEXT
			File.Delete(contextPath);
#endif
			return context;
		}

		
		public string StorageDirectory { get; set; }

		public ShadowContext()
		{
			this.StorageDirectory = SettingsBase.StorageDirectory;
		}

		public void Save(string shadowPath)
		{
			var contextPath = Path.ChangeExtension(shadowPath, ShadowContextExtension);
			Debug.Assert(contextPath != null, "contextPath != null");
			File.WriteAllText(contextPath, JsonConvert.SerializeObject(this));
		}
	}
}
