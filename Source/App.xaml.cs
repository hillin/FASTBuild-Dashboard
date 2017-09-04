﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using FastBuild.Dashboard.Support;
using Microsoft.Win32;

namespace FastBuild.Dashboard
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
				
				if (!CachedResources.TryGetValue(key, out var resource))
				{
					resource = (T)App.Current.FindResource(key);
					CachedResources.Add(key, resource);
				}

				return resource;
			}
		}

		public new static App Current { get; private set; }
#if DEBUG
		public static bool IsInDesignTime { get; } = DesignerProperties.GetIsInDesignMode(new DependencyObject());
#endif
		public bool StartMinimized { get; private set; }

		public string LocationOverride { get; private set; }

		public App()
		{
			this.InitializeComponent();
			App.Current = this;
		}

		internal void RaiseOnDeactivated()
		{
			this.OnDeactivated(EventArgs.Empty);
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
					if (!string.IsNullOrEmpty(this.LocationOverride))
					{
						location = this.LocationOverride;
					}
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

			foreach (string argument in args)
			{
				if (argument.Contains("-parent="))
				{
					this.LocationOverride = argument.Substring("-parent=".Length, argument.Length - "-parent=".Length).Replace("\"", "");
				}
			}
		}
	}
}
