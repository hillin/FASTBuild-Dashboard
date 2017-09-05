using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using FastBuild.Dashboard.Configuration;
using FastBuild.Dashboard.Services;
using FastBuild.Dashboard.Services.Build;
using FastBuild.Dashboard.Services.Build.SourceEditor;
using FastBuild.Dashboard.Services.Worker;
using FastBuild.Dashboard.Support;
using FastBuild.Dashboard.ViewModels;

namespace FastBuild.Dashboard
{
	internal class AppBootstrapper : BootstrapperBase
	{
		private readonly SimpleContainer _container = new SimpleContainer();

		public AppBootstrapper()
		{
			this.Initialize();
		}

		protected override void Configure()
		{
			base.Configure();
			_container.Singleton<IWindowManager, WindowManager>();
			_container.Singleton<IEventAggregator, EventAggregator>();
			_container.Singleton<MainWindowViewModel>();
			_container.Singleton<IBuildViewportService, BuildViewportService>();
			_container.Singleton<IBrokerageService, BrokerageService>();
			_container.Singleton<IWorkerAgentService, WorkerAgentService>();
			_container.Singleton<IExternalSourceEditorService, ExternalSourceEditorService>();
		}

		protected override void OnStartup(object sender, StartupEventArgs e)
		{		
			if (!App.Current.IsShadowProcess)
			{
				// a shadow process is always started by a non-shadow process, which
				// should already have the startup registry value set
				App.Current.SetStartupWithWindows(AppSettings.Default.StartWithWindows);
			}

#if DEBUG && !DEBUG_SINGLE_INSTANCE
			this.DisplayRootViewFor<MainWindowViewModel>();
			return;
#else
			var assemblyLocation = Assembly.GetEntryAssembly().Location;

			var identifier = assemblyLocation.Replace('\\', '_');
			if (!SingleInstance<App>.InitializeAsFirstInstance(identifier))
			{
				Environment.Exit(0);
			}

			if (App.Current.DoNotSpawnShadowExecutable || App.Current.IsShadowProcess)
			{
				this.DisplayRootViewFor<MainWindowViewModel>();
			}
			else
			{
				AppBootstrapper.SpawnShadowProcess(e, assemblyLocation);
				Environment.Exit(0);
			}
#endif
		}

		private static void CreateShadowContext(string shadowPath)
		{
			var shadowContext = new ShadowContext();
			shadowContext.Save(shadowPath);
		}

		private static void SpawnShadowProcess(StartupEventArgs e, string assemblyLocation)
		{
			var shadowAssemblyName = $"{Path.GetFileNameWithoutExtension(assemblyLocation)}.shadow.exe";
			var shadowPath = Path.Combine(Path.GetTempPath(), shadowAssemblyName);
			try
			{
				if (File.Exists(shadowPath))
				{
					File.Delete(shadowPath);
				}

				Debug.Assert(assemblyLocation != null, "assemblyLocation != null");
				File.Copy(assemblyLocation, shadowPath);
			}
			catch (UnauthorizedAccessException)
			{
				// may be already running
			}
			catch (IOException)
			{
				// may be already running
			}

			AppBootstrapper.CreateShadowContext(shadowPath);
			SingleInstance<App>.Cleanup();

			Process.Start(new ProcessStartInfo
			{
				FileName = shadowPath,
				Arguments = string.Join(" ", e.Args.Concat(new[] { AppArguments.ShadowProc }))
			});
		}

		protected override void OnExit(object sender, EventArgs e)
		{
			SingleInstance<App>.Cleanup();
			base.OnExit(sender, e);
		}

		protected override object GetInstance(Type serviceType, string key)
			=> _container.GetInstance(serviceType, key);

		protected override IEnumerable<object> GetAllInstances(Type serviceType)
			=> _container.GetAllInstances(serviceType);

		protected override void BuildUp(object instance)
			=> _container.BuildUp(instance);
	}
}
