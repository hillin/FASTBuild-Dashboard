using System;
using System.IO;
using System.Linq;
using System.Timers;

namespace FastBuild.Dashboard.Services
{
	internal class BrokerageService : IBrokerageService
	{
		private const string WorkerPoolRelativePath = @"main\16";

		private string[] _workerNames;

		public string[] WorkerNames
		{
			get => _workerNames;
			private set
			{
				var oldCount = _workerNames.Length;
				_workerNames = value;

				if (oldCount != _workerNames.Length)
				{
					this.WorkerCountChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		private bool _isUpdatingWorkers;

		public string BrokeragePath
		{
			get => Environment.GetEnvironmentVariable("FASTBUILD_BROKERAGE_PATH");
			set => Environment.SetEnvironmentVariable("FASTBUILD_BROKERAGE_PATH", value);
		}

		public event EventHandler WorkerCountChanged;

		public BrokerageService()
		{
			_workerNames = new string[0];

			var checkTimer = new Timer(5000);
			checkTimer.Elapsed += this.CheckTimer_Elapsed;
			checkTimer.AutoReset = true;
			checkTimer.Enabled = true;
			this.UpdateWorkers();
		}

		private void CheckTimer_Elapsed(object sender, ElapsedEventArgs e) => this.UpdateWorkers();

		private void UpdateWorkers()
		{
			if (_isUpdatingWorkers)
				return;

			_isUpdatingWorkers = true;

			try
			{
				var brokeragePath = this.BrokeragePath;
				if (string.IsNullOrEmpty(brokeragePath))
				{
					this.WorkerNames = new string[0];
					return;
				}

				try
				{
					this.WorkerNames = Directory.GetFiles(Path.Combine(brokeragePath, WorkerPoolRelativePath))
						.Select(Path.GetFileName)
						.ToArray();
				}
				catch (IOException)
				{
					this.WorkerNames = new string[0];
				}
			}
			finally
			{
				_isUpdatingWorkers = false;
			}
		}
	}
}
