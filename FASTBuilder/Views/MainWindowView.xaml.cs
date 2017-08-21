using System;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace FastBuilder.Views
{
	public partial class MainWindowView
	{
		private readonly DispatcherTimer _delayUpdateProfileTimer;

		public MainWindowView()
		{
			this.InitializeComponent();

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
			this.WindowState = Profile.Default.WindowState;

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
			Profile.Default.WindowState = this.WindowState;
			Profile.Default.WindowWidth = (int)this.Width;
			Profile.Default.WindowHeight = (int)this.Height;
			Profile.Default.Save();
		}

		private void MainWindowView_StateChanged(object sender, EventArgs e)
		{
			this.StartDelayedProfileUpdate();
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
	}
}
