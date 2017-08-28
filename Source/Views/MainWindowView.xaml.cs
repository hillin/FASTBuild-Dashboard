using System;
using System.Windows;
using System.Windows.Threading;
using FastBuild.Dashboard.ViewModels;

namespace FastBuild.Dashboard.Views
{
	public partial class MainWindowView
	{
		private DispatcherTimer _delayUpdateProfileTimer;
		private readonly TrayNotifier _trayNotifier;

		public MainWindowView()
		{
			this.InitializeComponent();
			this.InitializeWindowDimensions();
			_trayNotifier = new TrayNotifier(this);

			this.DataContextChanged += this.OnDataContextChanged;
		}

		protected override void OnClosed(EventArgs e)
		{
			_trayNotifier.Close();
			base.OnClosed(e);
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = e.NewValue as MainWindowViewModel;
			if (vm == null)
			{
				return;
			}

			vm.BuildWatcherPage.WorkingStateChanged += this.BuildWatcherPage_WorkingStateChanged;
		}

		private void BuildWatcherPage_WorkingStateChanged(object sender, bool isWorking)
		{
			if (isWorking)
			{
				_trayNotifier.UseWorkingIcon();
			}
			else
			{
				_trayNotifier.UseNormalIcon();
			}
		}

		private void InitializeWindowDimensions()
		{
			if (Profile.Default.IsFirstRun)
			{
				Profile.Default.IsFirstRun = false;
				Profile.Default.Save();
			}
			else
			{
				this.Left = Profile.Default.WindowLeft;
				this.Top = Profile.Default.WindowTop;
			}

			this.Width = Profile.Default.WindowWidth;
			this.Height = Profile.Default.WindowHeight;

			this.WindowState = App.Current.StartMinimized
				? WindowState.Minimized
				: Profile.Default.WindowState;

			this.LocationChanged += this.MainWindowView_LocationChanged;
			this.SizeChanged += this.MainWindowView_SizeChanged;
			this.StateChanged += this.MainWindowView_StateChanged;

			_delayUpdateProfileTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(500)
			};

			_delayUpdateProfileTimer.Tick += this.DelayUpdateProfileTimer_Tick;
		}

		private void DelayUpdateProfileTimer_Tick(object sender, EventArgs e)
		{
			_delayUpdateProfileTimer.Stop();

			Profile.Default.WindowLeft = (int)this.Left;
			Profile.Default.WindowTop = (int)this.Top;
			if (this.WindowState != WindowState.Minimized)
			{
				Profile.Default.WindowState = this.WindowState;
			}
			Profile.Default.WindowWidth = (int)this.Width;
			Profile.Default.WindowHeight = (int)this.Height;
			Profile.Default.Save();
		}

		private void MainWindowView_StateChanged(object sender, EventArgs e)
		{
			this.StartDelayedProfileUpdate();

			if (this.WindowState == WindowState.Minimized)
			{
				this.Hide();
			}
		}

		private void StartDelayedProfileUpdate()
		{
			_delayUpdateProfileTimer.Stop();
			_delayUpdateProfileTimer.Start();
		}

		private void MainWindowView_LocationChanged(object sender, EventArgs e)
		{
			this.StartDelayedProfileUpdate();
		}

		private void MainWindowView_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			this.StartDelayedProfileUpdate();
		}

		private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			this.MenuToggleButton.IsChecked = false;
		}

		public void ShowAndActivate()
		{
			this.Show();
			if (this.WindowState == WindowState.Minimized)
			{
				this.WindowState = Profile.Default.WindowState;
			}

			this.Activate();
		}
	}
}
