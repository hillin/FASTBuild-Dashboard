using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using FastBuilder.Services;
using FastBuilder.Services.Worker;
using FastBuilder.ViewModels;

namespace FastBuilder
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
			_container.Singleton<IViewTransformService, ViewTransformService>();
			_container.Singleton<IBrokerageService, BrokerageService>();
			_container.Singleton<IWorkerAgentService, WorkerAgentService>();
		}

		protected override void OnStartup(object sender, StartupEventArgs e)
		{
			this.DisplayRootViewFor<MainWindowViewModel>();
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
