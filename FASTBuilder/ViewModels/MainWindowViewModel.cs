using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FastBuilder.ViewModels
{
	internal class MainWindowViewModel : Conductor<IMainPage>.Collection.AllActive
	{
		private IMainPage _currentPage;
		public BuildWatcherViewModel BuildWatcher { get; } = new BuildWatcherViewModel();

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
