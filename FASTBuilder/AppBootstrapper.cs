using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using FastBuilder.Services;
using FastBuilder.Services.Worker;
using FastBuilder.ViewModels;
using FASTBuilder;
using Microsoft.Shell;

namespace FastBuilder
{
	internal class AppBootstrapper : BootstrapperBase
	{
		private readonly SimpleContainer _container = new SimpleContainer();
		private const string FallbackSingleInstanceIdentifier = "FASTBuilder";

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
			_container.Singleton<IViewTransformService, ViewTransformService>();
			_container.Singleton<IBrokerageService, BrokerageService>();
			_container.Singleton<IWorkerAgentService, WorkerAgentService>();
		}

		protected override void OnStartup(object sender, StartupEventArgs e)
		{
			App.Current.SetStartupWithWindows(AppSettings.Default.StartWithWindows);
			App.Current.ProcessArgs(e.Args);

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
				var shadowPath = Path.Combine(Environment.CurrentDirectory, shadowAssemblyName);
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

				SingleInstance<App>.Cleanup();

				Process.Start(new ProcessStartInfo
				{
					FileName = shadowPath,
					Arguments = string.Join(" ", e.Args.Concat(new[] { "-no-shadow" }))
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
		{
			return _container.GetInstance(serviceType, key);
		}

		protected override IEnumerable<object> GetAllInstances(Type serviceType)
		{
			return _container.GetAllInstances(serviceType);
		}

		protected override void BuildUp(object instance)
		{
			_container.BuildUp(instance);
		}
	}
}
