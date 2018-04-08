using System;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using FastBuild.Dashboard.Configuration;
using FastBuild.Dashboard.ViewModels;
using FastBuild.Dashboard.Services.Worker;

namespace FastBuild.Dashboard.Views
{
	public partial class MainWindowView
	{
		private DispatcherTimer _delayUpdateProfileTimer;
		private readonly TrayNotifier _trayNotifier;
		private bool _isWorking;
		private MainWindowViewModel _viewModel;

		public MainWindowView()
		{
			this.InitializeComponent();
			this.InitializeWindowDimensions();
			_trayNotifier = new TrayNotifier(this);
			this.UpdateTrayIcon();

			this.DataContextChanged += this.OnDataContextChanged;
			this.Loaded += this.OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (App.Current.StartMinimized)
			{
				this.Hide();
			}
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
			vm.SettingsPage.WorkerModeChanged += this.SettingsPage_WorkerModeChanged;
			_viewModel = vm;
		}

		private void BuildWatcherPage_WorkingStateChanged(object sender, bool isWorking)
		{
			_isWorking = isWorking;
			UpdateTrayIcon();
		}

		private void SettingsPage_WorkerModeChanged(object sender, WorkerMode mode)
		{
			UpdateTrayIcon();
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

			if (App.Current.StartMinimized)
			{
				this.WindowState = WindowState.Minimized;
				this.Hide();
			}
			else
			{
				this.WindowState = Profile.Default.WindowState;
			}

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

		public void ChangeWorkerMode(int workerMode)
		{
			if (_viewModel != null)
			{
				_viewModel.SettingsPage.WorkerMode = workerMode;
			}
		}

		private bool IsWorkerEnabled()
		{
			if (IoC.Get<IWorkerAgentService>().WorkerMode == WorkerMode.Disabled)
				return false;
			return true;
		}

		private void UpdateTrayIcon()
		{
			if (_isWorking)
			{
				_trayNotifier.UseWorkingIcon();
			}
			else
			{
				if (this.IsWorkerEnabled())
				{
					_trayNotifier.UseNormalIcon();
				}
				else
				{
					_trayNotifier.UseDisabledIcon();
				}
			}
		}
	}
}
