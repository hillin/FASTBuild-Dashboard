using System;
using System.Windows.Media;
using Caliburn.Micro;
using FastBuild.Dashboard.Services.Worker;

namespace FastBuild.Dashboard.ViewModels.Worker
{
	internal class WorkerCoreStatusViewModel : PropertyChangedBase
	{
		public int CoreId { get; }

		private WorkerCoreStatus _status;
		public string HostHelping => _status.HostHelping;
		public string WorkingItem => _status.WorkingItem;
		public bool IsWorking => _status.State == WorkerCoreState.Working;

		public string DisplayState
		{
			get
			{
				switch (_status.State)
				{
					case WorkerCoreState.Disabled:
						return "Disabled";
					case WorkerCoreState.Idle:
						return "Idle";
					case WorkerCoreState.Working:
						return "Working";
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public Brush UIBulbBorderColor
		{
			get
			{
				switch (_status.State)
				{
					case WorkerCoreState.Disabled:
						return Brushes.Gray;
					case WorkerCoreState.Idle:
					case WorkerCoreState.Working:
						return Brushes.DarkGreen;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public Brush UIBulbFillColor
		{
			get
			{
				switch (_status.State)
				{
					case WorkerCoreState.Disabled:
					case WorkerCoreState.Idle:
						return Brushes.Transparent;
					case WorkerCoreState.Working:
						return Brushes.Green;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public Brush UIBulbForeground
		{
			get
			{
				switch (_status.State)
				{
					case WorkerCoreState.Disabled:
						return Brushes.Gray;
					case WorkerCoreState.Idle:
						return Brushes.Green;
					case WorkerCoreState.Working:
						return Brushes.White;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public WorkerCoreStatusViewModel(int coreId) => this.CoreId = coreId;

		public void UpdateStatus(WorkerCoreStatus status)
		{
			_status = status;
			this.NotifyOfPropertyChange(nameof(this.HostHelping));
			this.NotifyOfPropertyChange(nameof(this.WorkingItem));
			this.NotifyOfPropertyChange(nameof(this.UIBulbBorderColor));
			this.NotifyOfPropertyChange(nameof(this.UIBulbFillColor));
			this.NotifyOfPropertyChange(nameof(this.UIBulbForeground));
			this.NotifyOfPropertyChange(nameof(this.IsWorking));
			this.NotifyOfPropertyChange(nameof(this.DisplayState));
		}
	}
}
