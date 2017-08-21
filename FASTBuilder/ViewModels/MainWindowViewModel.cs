using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FastBuilder.ViewModels.About;
using FastBuilder.ViewModels.Build;
using FastBuilder.ViewModels.Settings;
using FastBuilder.ViewModels.Worker;

namespace FastBuilder.ViewModels
{
	internal sealed class MainWindowViewModel : Conductor<IMainPage>.Collection.AllActive
	{
		private IMainPage _currentPage;
		public BuildWatcherViewModel BuildWatcherPage { get; } = new BuildWatcherViewModel();
		public WorkerViewModel WorkerPage { get; } = new WorkerViewModel();
		public SettingsViewModel SettingsPage { get; } = new SettingsViewModel();
		public AboutViewModel AboutPage { get; } = new AboutViewModel();

		public IMainPage CurrentPage
		{
			get => _currentPage;
			set
			{
				if (object.Equals(value, _currentPage))
				{
					return;
				}

				_currentPage = value;
				this.NotifyOfPropertyChange();
			}
		}

		public MainWindowViewModel()
		{
			this.Items.Add(this.BuildWatcherPage);
			this.Items.Add(this.WorkerPage);
			this.Items.Add(this.SettingsPage);
			this.Items.Add(this.AboutPage);

			this.CurrentPage = this.BuildWatcherPage;
			this.DisplayName = "FASTBuilder";
		}

		public override void ActivateItem(IMainPage item)
		{
			base.ActivateItem(item);
			this.CurrentPage = item;
		}
	}
}
