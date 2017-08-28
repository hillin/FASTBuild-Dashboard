using Caliburn.Micro;
using FastBuild.Dashboard.ViewModels.About;
using FastBuild.Dashboard.ViewModels.Build;
using FastBuild.Dashboard.ViewModels.Settings;
using FastBuild.Dashboard.ViewModels.Worker;

namespace FastBuild.Dashboard.ViewModels
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
			this.DisplayName = "FASTBuild Dashboard";
		}

		public override void ActivateItem(IMainPage item)
		{
			base.ActivateItem(item);
			this.CurrentPage = item;
		}
	}
}
