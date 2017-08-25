using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Microsoft.Shell;
using Microsoft.Win32;
using System.Linq;

namespace FASTBuilder
{
	public partial class App : ISingleInstanceApp
	{
		public static class CachedResource<T>
		{
			private static readonly Dictionary<string, T> CachedResources
				= new Dictionary<string, T>();

			public static T GetResource(string key)
			{
#if DEBUG
				if (App.IsInDesignTime)
				{
					return default(T);
				}
#endif

				if (!CachedResources.TryGetValue(key, out var brush))
				{
					brush = (T)App.Current.FindResource(key);
					CachedResources.Add(key, brush);
				}

				return brush;
			}
		}

		public new static App Current { get; private set; }
#if DEBUG
		public static bool IsInDesignTime { get; } = DesignerProperties.GetIsInDesignMode(new DependencyObject());
#endif
		public bool StartMinimized { get; private set; }

		public App()
		{
			this.InitializeComponent();
			App.Current = this;
		}

		public bool SignalExternalCommandLineArgs(IList<string> args)
		{
			Application.Current.MainWindow.Show();
			Application.Current.MainWindow.Activate();
			return true;
		}

		public bool SetStartupWithWindows(bool startUp)
		{
			var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			var entryAssembly = Assembly.GetEntryAssembly();

			if (key != null && !string.IsNullOrEmpty(entryAssembly.Location))
			{
				if (startUp)
				{
					var location = entryAssembly.Location;
					Debug.Assert(location != null, "location != null");
					if (location.EndsWith(".shadow.exe", System.StringComparison.InvariantCultureIgnoreCase))
					{
						location = location.Substring(0, location.Length - ".shadow.exe".Length) + ".exe";
					}

					key.SetValue(entryAssembly.GetName().Name, $"\"{location}\" -minimized");
				}
				else
				{
					key.DeleteValue(entryAssembly.GetName().Name, false);
				}

				return true;
			}

			return false;
		}

		public void ProcessArgs(string[] args)
		{
			this.StartMinimized = args.Contains("-minimized");
		}
	}
}
