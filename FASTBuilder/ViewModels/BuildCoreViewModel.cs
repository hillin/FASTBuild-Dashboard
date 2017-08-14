using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using FastBuilder.Communication;
using FastBuilder.Communication.Events;
using FastBuilder.Services;

namespace FastBuilder.ViewModels
{
	internal class BuildCoreViewModel : PropertyChangedBase
	{
		private bool _isBusy;
		private BuildJobViewModel _currentJob;
		private double _uiJobsTotalWidth;
		public int Id { get; }
		public BuildWorkerViewModel OwnerWorker { get; }

		public BindableCollection <BuildJobViewModel> Jobs { get; } = new BindableCollection <BuildJobViewModel>();

		public BuildJobViewModel CurrentJob
		{
			get => _currentJob;
			private set
			{
				if (object.Equals(value, _currentJob)) return;
				_currentJob = value;
				this.NotifyOfPropertyChange();
			}
		}

		public bool IsBusy
		{
			get => _isBusy;
			private set
			{
				if (value == _isBusy) return;
				_isBusy = value;
				this.NotifyOfPropertyChange();
			}
		}

		public double UIJobsTotalWidth
		{
			get => _uiJobsTotalWidth;
			private set
			{
				if (value.Equals(_uiJobsTotalWidth)) return;
				_uiJobsTotalWidth = value;
				this.NotifyOfPropertyChange();
			}
		}

		public BuildCoreViewModel(int id, BuildWorkerViewModel ownerWorker)
		{
			this.Id = id;
			this.OwnerWorker = ownerWorker;
			IoC.Get<IViewTransformService>().PreScalingChanging 
				+= this.ViewTransformService_PreScalingChanging;
		}

		private void ViewTransformService_PreScalingChanging(object sender, EventArgs e)
		{
			this.UpdateUIJobsTotalWidth();
		}

		public void OnJobFinished(FinishJobEventArgs e)
		{
			this.CurrentJob?.OnFinished(e);
			this.CurrentJob = null;
			this.IsBusy = false;
		}

		public void OnJobStarted(StartJobEventArgs e, DateTime sessionStartTime)
		{
			this.IsBusy = true;

			this.CurrentJob = new BuildJobViewModel(e, sessionStartTime);

			// called from log watcher thread
			lock (this.Jobs)
			{
				this.Jobs.Add(this.CurrentJob);
			}
		}

		public void OnSessionStopped(DateTime time)
		{
			foreach (var job in this.Jobs)
			{
				job.OnSessionStopped(time);
			}
		}

		public void Tick(DateTime now)
		{
			// called from tick thread
			lock (this.Jobs)
			{
				foreach (var job in this.Jobs)
				{
					job.Tick(now);
				}
			}

			this.UpdateUIJobsTotalWidth();
		}

		private void UpdateUIJobsTotalWidth()
		{
			this.UIJobsTotalWidth = IoC.Get<IViewTransformService>().Scaling *
			                        this.OwnerWorker.OwnerSession.ElapsedTime.TotalSeconds;
		}
	}
}
