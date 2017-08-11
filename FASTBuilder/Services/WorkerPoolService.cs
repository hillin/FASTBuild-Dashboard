using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FastBuilder.Services
{
	internal class WorkerPoolService : IWorkerPoolService
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
					this.WorkerCountChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler WorkerCountChanged;

		public WorkerPoolService()
		{
			_workerNames = new string[0];

			var checkTimer = new Timer(1000);
			checkTimer.Elapsed += CheckTimer_Elapsed;
			checkTimer.AutoReset = true;
			checkTimer.Enabled = true;
		}

		private void CheckTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			var brokeragePath = Environment.GetEnvironmentVariable("FASTBUILD_BROKERAGE_PATH");
			if (string.IsNullOrEmpty(brokeragePath))
			{
				this.WorkerNames = new string[0];
				return;
			}

			try
			{
				this.WorkerNames = Directory.GetFiles(Path.Combine(brokeragePath, WorkerPoolRelativePath)).Select(Path.GetFileName).ToArray();
			}
			catch (IOException)
			{
				this.WorkerNames = new string[0];
			}
		}
	}
}
