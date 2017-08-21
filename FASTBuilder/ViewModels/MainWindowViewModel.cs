using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FastBuilder.ViewModels.Build;
using FastBuilder.ViewModels.Settings;
using FastBuilder.ViewModels.Worker;

namespace FastBuilder.ViewModels
{
	internal sealed class MainWindowViewModel : Conductor<IMainPage>.Collection.AllActive
	{
		private IMainPage _currentPage;
		public BuildWatcherViewModel BuildWatcher { get; } = new BuildWatcherViewModel();
		public WorkerViewModel Worker { get; } = new WorkerViewModel();
		public SettingsViewModel Settings { get; } = new SettingsViewModel();

		public IMainPage CurrentPage
		{
			get => _currentPage;
			set
			{
				if (object.Equals(value, _currentPage)) return;
				_currentPage = value;
				this.NotifyOfPropertyChange();
			}
		}

		public MainWindowViewModel()
		{
			this.Items.Add(this.BuildWatcher);
			this.Items.Add(this.Worker);
			this.Items.Add(this.Settings);

			this.CurrentPage = this.BuildWatcher;
			this.DisplayName = "FASTBuilder";
		}

		public override void ActivateItem(IMainPage item)
		{
			base.ActivateItem(item);
			this.CurrentPage = item;
		}
	}
}
