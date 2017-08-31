using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using FastBuild.Dashboard.Services.Build;
using FastBuild.Dashboard.ViewModels.Build;
using Action = System.Action;

namespace FastBuild.Dashboard.Views.Build
{
	internal abstract class BuildSessionCustomRenderingControlBase : Control
	{

		protected double CurrentTimeOffset { get; private set; }

		protected BuildSessionJobManager JobManager { get; private set; }
		protected BuildSessionViewModel SessionViewModel { get; private set; }
		protected IBuildViewportService ViewportService { get; }

		protected BuildSessionCustomRenderingControlBase()
		{
			this.ViewportService = IoC.Get<IBuildViewportService>();
			this.DataContextChanged += this.OnDataContextChanged;
		}

		protected virtual void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.SessionViewModel != null)
			{
				this.SessionViewModel.Ticked -= this.SessionViewModel_Ticked;
				this.SessionViewModel = null;

				this.JobManager.OnJobFinished -= this.JobManager_OnJobFinished;
				this.JobManager.OnJobStarted -= this.JobManager_OnJobStarted;
				this.JobManager = null;

				this.Clear();
			}

			var vm = this.DataContext as BuildSessionViewModel;
			if (vm == null)
			{
				return;
			}

			this.SessionViewModel = vm;
			this.SessionViewModel.Ticked += this.SessionViewModel_Ticked;

			this.JobManager = vm.JobManager;
			this.JobManager.OnJobStarted += this.JobManager_OnJobStarted;
			this.JobManager.OnJobFinished += this.JobManager_OnJobFinished;
		}


		private void JobManager_OnJobFinished(object sender, BuildJobViewModel e)
			=> this.Dispatcher.BeginInvoke(new Action(() => this.OnJobFinished(e)));

		protected virtual void OnJobFinished(BuildJobViewModel job)
			=> this.InvalidateVisual();

		private void JobManager_OnJobStarted(object sender, BuildJobViewModel e)
			=> this.Dispatcher.BeginInvoke(new Action(() => this.OnJobFinished(e)));

		protected virtual void OnJobStarted(BuildJobViewModel job)
			=> this.InvalidateVisual();


		private void SessionViewModel_Ticked(object sender, double timeOffset)
			=> this.Dispatcher.BeginInvoke(new Action(() => this.Tick(timeOffset)));

		protected virtual void Tick(double timeOffset)
			=> this.CurrentTimeOffset = timeOffset;

		protected virtual void Clear()
			=> this.InvalidateVisual();
	}
}
