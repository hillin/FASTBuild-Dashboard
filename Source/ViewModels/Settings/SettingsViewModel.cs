using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Caliburn.Micro.Validation;
using FastBuild.Dashboard.Configuration;
using FastBuild.Dashboard.Services;
using FastBuild.Dashboard.Services.Worker;
using Ookii.Dialogs.Wpf;

namespace FastBuild.Dashboard.ViewModels.Settings
{
	internal sealed partial class SettingsViewModel : ValidatingScreen<SettingsViewModel>, IMainPage
	{
		public event EventHandler<WorkerMode> WorkerModeChanged;

		[CustomValidation(typeof(SettingsValidator), "ValidateBrokeragePath")]
		public string BrokeragePath
		{
			get => IoC.Get<IBrokerageService>().BrokeragePath;
			set
			{
				IoC.Get<IBrokerageService>().BrokeragePath = value;
				this.NotifyOfPropertyChange();
			}
		}

		public string DisplayWorkersInPool
		{
			get
			{
				var workerCount = IoC.Get<IBrokerageService>().WorkerNames.Length;
				switch (workerCount)
				{
					case 0:
						return "no workers in pool";
					case 1:
						return "1 worker in pool";
					default:
						return $"{workerCount} workers in pool";
				}
			}
		}

		public int WorkerMode
		{
			get => (int)IoC.Get<IWorkerAgentService>().WorkerMode;
			set
			{
				IoC.Get<IWorkerAgentService>().WorkerMode = (WorkerMode)value;
				this.WorkerModeChanged?.Invoke(this, (WorkerMode)value);
				this.NotifyOfPropertyChange();
			}
		}

		public int WorkerCores
		{
			get => IoC.Get<IWorkerAgentService>().WorkerCores;
			set
			{
				IoC.Get<IWorkerAgentService>().WorkerCores = Math.Max(1, Math.Min(this.MaximumCores, value));
				this.NotifyOfPropertyChange();
				this.NotifyOfPropertyChange(nameof(this.DisplayCores));
			}
		}

		public bool StartWithWindows
		{
			get => AppSettings.Default.StartWithWindows;
			set
			{
				AppSettings.Default.StartWithWindows = value;
				AppSettings.Default.Save();
				App.Current.SetStartupWithWindows(value);
				this.NotifyOfPropertyChange();
			}
		}

		public string DisplayCores => this.WorkerCores == 1 ? "1 core" : $"up to {this.WorkerCores} cores";

		public int MaximumCores { get; }
		public DoubleCollection CoreTicks { get; }

		public string Icon => "Settings";

		public SettingsViewModel()
		{
			this.MaximumCores = Environment.ProcessorCount;
			this.CoreTicks = new DoubleCollection(Enumerable.Range(1, this.MaximumCores).Select(i => (double)i));

			this.DisplayName = "Settings";

			var brokerageService = IoC.Get<IBrokerageService>();
			brokerageService.WorkerCountChanged += this.BrokerageService_WorkerCountChanged;
		}

		private void BrokerageService_WorkerCountChanged(object sender, EventArgs e)
		{
			this.NotifyOfPropertyChange(nameof(this.DisplayWorkersInPool));
		}

		public void BrowseBrokeragePath()
		{
			var dialog = new VistaFolderBrowserDialog
			{
				Description = "Browse Brokerage Path",
				SelectedPath = this.BrokeragePath,
				ShowNewFolderButton = false
			};

			if (dialog.ShowDialog(App.Current.MainWindow) == true)
			{
				this.BrokeragePath = dialog.SelectedPath;
			}
		}
	}
}
