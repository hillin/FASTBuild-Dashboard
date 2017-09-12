using System;
using Caliburn.Micro;
using FastBuild.Dashboard.Services;

namespace FastBuild.Dashboard.ViewModels.Build
{
	partial class BuildSessionViewModel
	{
		private bool _isRunning;
		private double _progress;
		private bool _isRestoringHistory;
		private int _inProgressJobCount;
		private int _successfulJobCount;
		private int _failedJobCount;
		private int _cacheHitCount;
		private int _activeWorkerCount;
		private int _activeCoreCount;
		private string[] _poolWorkerNames;

		public bool IsRestoringHistory
		{
			get => _isRestoringHistory;
			set
			{
				if (value == _isRestoringHistory)
				{
					return;
				}

				_isRestoringHistory = value;
				this.NotifyOfPropertyChange();
				this.NotifyOfPropertyChange(nameof(this.IsSessionViewVisible));
				this.NotifyOfPropertyChange(nameof(this.StatusText));

				if (!this.IsRestoringHistory)
				{
					// refresh these values after restoring history because they are not updated during the process
					// in order to increase history restoration performance
					this.NotifyOfPropertyChange(nameof(this.SuccessfulJobCount));
					this.NotifyOfPropertyChange(nameof(this.CacheHitCount));
					this.NotifyOfPropertyChange(nameof(this.InProgressJobCount));
					this.NotifyOfPropertyChange(nameof(this.FailedJobCount));

					this.DetectDebris();
				}
			}
		}

		public bool IsSessionViewVisible => !this.IsRestoringHistory;

		public bool IsRunning
		{
			get => _isRunning;
			private set
			{
				if (value == _isRunning)
				{
					return;
				}

				_isRunning = value;
				this.NotifyOfPropertyChange();
				this.NotifyOfPropertyChange(nameof(this.StatusText));
			}
		}

		public double Progress
		{
			get => _progress;
			private set
			{
				if (value.Equals(_progress))
				{
					return;
				}

				_progress = value;
				this.NotifyOfPropertyChange();
				if (this.IsRunning)
				{
					this.NotifyOfPropertyChange(nameof(this.StatusText));
				}
			}
		}

		public string StatusText
		{
			get
			{
				if (_isRestoringHistory)
				{
					return $"Loading ({this.Progress:0}%)";
				}

				if (this.IsRunning)
				{
					return $"Building ({this.Progress:0}%)";
				}

				return "Finished";
			}
		}

		public int InProgressJobCount
		{
			get => _inProgressJobCount;
			private set
			{
				if (value == _inProgressJobCount)
				{
					return;
				}

				_inProgressJobCount = value;

				if (!this.IsRestoringHistory)
				{
					this.NotifyOfPropertyChange();
				}
			}
		}

		public int SuccessfulJobCount
		{
			get => _successfulJobCount;
			private set
			{
				if (value == _successfulJobCount)
				{
					return;
				}

				_successfulJobCount = value;

				if (!this.IsRestoringHistory)
				{
					this.NotifyOfPropertyChange();
				}
			}
		}

		public int FailedJobCount
		{
			get => _failedJobCount;
			private set
			{
				if (value == _failedJobCount)
				{
					return;
				}

				_failedJobCount = value;

				if (!this.IsRestoringHistory)
				{
					this.NotifyOfPropertyChange();
				}
			}
		}

		public int CacheHitCount
		{
			get => _cacheHitCount;
			private set
			{
				if (value == _cacheHitCount)
				{
					return;
				}

				_cacheHitCount = value;

				if (!this.IsRestoringHistory)
				{
					this.NotifyOfPropertyChange();
				}
			}
		}

		public int ActiveWorkerCount
		{
			get => _activeWorkerCount;
			private set
			{
				if (value == _activeWorkerCount)
				{
					return;
				}

				_activeWorkerCount = value;
				this.NotifyOfPropertyChange();
			}
		}

		public int ActiveCoreCount
		{
			get => _activeCoreCount;
			private set
			{
				if (value == _activeCoreCount)
				{
					return;
				}

				_activeCoreCount = value;
				this.NotifyOfPropertyChange();
			}
		}

		public int PoolWorkerCount => this.PoolWorkerNames.Length;

		public string[] PoolWorkerNames
		{
			get => _poolWorkerNames;
			private set
			{
				_poolWorkerNames = value;
				this.NotifyOfPropertyChange();
				this.NotifyOfPropertyChange(nameof(this.PoolWorkerCount));
			}
		}

		private void UpdateActiveWorkerAndCoreCount()
		{
			if (this.IsRestoringHistory)
			{
				return;
			}

			var activeWorkerCount = 0;
			var activeCoreCount = 0;
			foreach (var worker in this.Workers)
			{
				if (worker.ActiveCoreCount > 0)
				{
					++activeWorkerCount;
					activeCoreCount += worker.ActiveCoreCount;
				}
			}

			this.ActiveWorkerCount = activeWorkerCount;
			this.ActiveCoreCount = activeCoreCount;
		}


		private void BrokerageService_WorkerCountChanged(object sender, EventArgs e)
		{
			this.PoolWorkerNames = IoC.Get<IBrokerageService>().WorkerNames;
		}
	}
}
