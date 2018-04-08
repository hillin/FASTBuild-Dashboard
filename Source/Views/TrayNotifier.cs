using System;
using System.Windows.Threading;
using Application = System.Windows.Application;
using WinForms = System.Windows.Forms;

namespace FastBuild.Dashboard.Views
{
	internal class TrayNotifier
	{
		private readonly MainWindowView _owner;
		private readonly WinForms.NotifyIcon _trayNotifier;

		private int _workingIconStage = 0;
		private readonly DispatcherTimer _workingIconTimer;

		public TrayNotifier(MainWindowView owner)
		{
			_owner = owner;
			_trayNotifier = new WinForms.NotifyIcon();
			_trayNotifier.DoubleClick += this.TrayNotifier_DoubleClick;
			_trayNotifier.Text = "FASTBuild Dashboard is running. Double click to show window.";
			_trayNotifier.Visible = true;

			_workingIconTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(0.3)
			};

			_workingIconTimer.Tick += this.WorkingIconTimer_Tick;

			_trayNotifier.ContextMenuStrip = new WinForms.ContextMenuStrip();
			_trayNotifier.ContextMenuStrip.Items.Add("Show", GetImage("/Resources/Icons/tray_normal_16.ico"), this.TrayNotifier_DoubleClick);
			_trayNotifier.ContextMenuStrip.Items.Add(new WinForms.ToolStripSeparator());
			_trayNotifier.ContextMenuStrip.Items.Add("Work Always", GetImage("/Resources/Icons/tray_working_all_16.ico"), (sender, args) => MenuChangeWorkerMode(2));
			_trayNotifier.ContextMenuStrip.Items.Add("Work when Idle", GetImage("/Resources/Icons/tray_normal_16.ico"), (sender, args) => MenuChangeWorkerMode(1));
			_trayNotifier.ContextMenuStrip.Items.Add("Disabled", GetImage("/Resources/Icons/tray_disabled_16.ico"), (sender, args) => MenuChangeWorkerMode(0));

			this.UseNormalIcon();
		}

		private void TrayNotifier_DoubleClick(object sender, EventArgs e) => _owner.ShowAndActivate();

		private void WorkingIconTimer_Tick(object sender, EventArgs e) => this.ShiftWorkingIcon();
		public void UseNormalIcon()
		{
			_workingIconTimer.Stop();
			this.SetTrayIcon("/Resources/Icons/tray_normal_16.ico");
		}

		public void UseDisabledIcon()
		{
			_workingIconTimer.Stop();
			this.SetTrayIcon("/Resources/Icons/tray_disabled_16.ico");
		}

		public void UseWorkingIcon()
		{
			this.ShiftWorkingIcon();
			_workingIconTimer.Start();
		}

		private void ShiftWorkingIcon()
		{
			this.SetTrayIcon($"/Resources/Icons/tray_working_{_workingIconStage}_16.ico");
			_workingIconStage = (_workingIconStage + 1) % 3;
		}

		private System.Drawing.Image GetImage(string resourcePath)
		{
			var iconInfo = Application.GetResourceStream(new Uri(resourcePath, UriKind.Relative));
			if (iconInfo != null)
			{
				using (var iconStream = iconInfo.Stream)
				{
					return System.Drawing.Image.FromStream(iconStream);
				}
			}
			return null;
		}

		private void SetTrayIcon(string resourcePath)
		{
			var iconInfo = Application.GetResourceStream(new Uri(resourcePath, UriKind.Relative));
			if (iconInfo != null)
			{
				using (var iconStream = iconInfo.Stream)
				{
					_trayNotifier.Icon = new System.Drawing.Icon(iconStream);
				}
			}
		}

		public void Close()
		{
			_trayNotifier.Visible = false;
			_trayNotifier.Dispose();
		}

		private void MenuChangeWorkerMode(int workerMode)
		{
			_owner.ChangeWorkerMode(workerMode);
		}
	}
}
