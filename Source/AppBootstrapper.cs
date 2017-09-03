using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
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
		private const string FallbackSingleInstanceIdentifier = "FASTBuild-Dashboard";

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
			App.Current.ProcessArgs(e.Args);
			App.Current.SetStartupWithWindows(AppSettings.Default.StartWithWindows);

#if DEBUG && !DEBUG_SINGLE_INSTANCE
			this.DisplayRootViewFor<MainWindowViewModel>();
#else

			var assemblyLocation = Assembly.GetEntryAssembly().Location;

			var identifier = assemblyLocation?.Replace('\\', '_') ?? FallbackSingleInstanceIdentifier;
			if (!SingleInstance<App>.InitializeAsFirstInstance(identifier))
			{
				Environment.Exit(0);
			}

			if (e.Args.Contains("-no-shadow"))
			{
				this.DisplayRootViewFor<MainWindowViewModel>();
			}
			else
			{
				var shadowAssemblyName = $"{Path.GetFileNameWithoutExtension(assemblyLocation)}.shadow.exe";
				var shadowPath = Path.Combine(Path.GetTempPath(), "FBDashboard", shadowAssemblyName);
				try
				{
					if (File.Exists(shadowPath))
					{
						File.Delete(shadowPath);
					}

					Debug.Assert(assemblyLocation != null, "assemblyLocation != null");
					Directory.CreateDirectory(Path.GetDirectoryName(shadowPath));
					File.Copy(assemblyLocation, shadowPath);

					// Copy FBuild folder with worker if exists.
					var workerFolder = Path.Combine(Path.GetDirectoryName(assemblyLocation), "FBuild");
					var workerTargetFolder = Path.Combine(Path.GetDirectoryName(shadowPath), "FBuild");
					if (Directory.Exists(workerFolder))
					{
						Directory.CreateDirectory(workerTargetFolder);
						// Copy all worker files.
						foreach (string newPath in Directory.GetFiles(workerFolder, "*.*", SearchOption.TopDirectoryOnly))
						{
							File.Copy(newPath, newPath.Replace(workerFolder, workerTargetFolder), true);
						}
					}
				}
				catch (UnauthorizedAccessException)
				{
					// may be already running
				}
				catch (IOException)
				{
					// may be already running
				}

				SingleInstance<App>.Cleanup();

				Process.Start(new ProcessStartInfo
				{
					FileName = shadowPath,
					Arguments = string.Join(" ", e.Args.Concat(new[] { $"-no-shadow -parent=\"{assemblyLocation}\"" }))
				});

				Environment.Exit(0);
			}
#endif
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
