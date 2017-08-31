using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using FastBuild.Dashboard.Services;
using FastBuild.Dashboard.Services.Build;
using FastBuild.Dashboard.ViewModels.Build;
using Action = System.Action;

namespace FastBuild.Dashboard.Views.Build
{
	internal partial class BuildJobsView : BuildSessionCustomRenderingControlBase
	{
		private double StartTimeOffset { get; set; }
		private double EndTimeOffset { get; set; }

		private bool _wasNowInTimeFrame;


		// a set that stores all the cores visible to current viewport
		private readonly HashSet<BuildCoreViewModel> _visibleCores
			= new HashSet<BuildCoreViewModel>();

		public BuildJobsView()
		{
			this.Background = Brushes.Transparent;

			this.InitializeLayoutPart();
			this.InitializeRenderPart();
			this.InitializeTooltipPart();
		}

		protected override void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			base.OnDataContextChanged(sender, e);
			this.UpdateTimeFrame();

			this.InvalidateCores();
			this.InvalidateJobs();
		}

		protected override void OnJobStarted(BuildJobViewModel job)
		{
			this.InvalidateCores();

			if (job.StartTimeOffset <= this.EndTimeOffset && job.EndTimeOffset >= this.StartTimeOffset)
			{
				this.TryAddJob(job);
			}

			base.OnJobStarted(job);
		}

		protected override void Tick(double timeOffset)
		{
			base.Tick(timeOffset);

			var isNowInTimeFrame = this.EndTimeOffset >= this.CurrentTimeOffset
				&& this.StartTimeOffset <= this.CurrentTimeOffset;
			if (isNowInTimeFrame)
			{
				if (!_wasNowInTimeFrame)
				{
					// "now" has come into current time frame, add all active jobs
					foreach (var job in this.JobManager.GetAllJobs().Where(j => !j.IsFinished))
					{
						this.TryAddJob(job);
					}
				}

				this.InvalidateMeasure();
			}

			_wasNowInTimeFrame = isNowInTimeFrame;
		}
	}
}
