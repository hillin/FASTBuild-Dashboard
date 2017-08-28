using System;
using Caliburn.Micro;

namespace FastBuild.Dashboard.Services
{
	internal class BuildViewportServiceXamlSupport : PropertyChangedBase
	{
		public static BuildViewportServiceXamlSupport Instance { get; }

		static BuildViewportServiceXamlSupport()
		{
			BuildViewportServiceXamlSupport.Instance = new BuildViewportServiceXamlSupport();
		}

		private BuildJobDisplayMode _jobDisplayMode;

		public BuildJobDisplayMode JobDisplayMode
		{
			get => _jobDisplayMode;
			private set
			{
				if (value == _jobDisplayMode)
				{
					return;
				}

				_jobDisplayMode = value;
				this.NotifyOfPropertyChange();
				this.NotifyOfPropertyChange(nameof(this.IsCompactDisplayMode));
			}
		}

		public bool IsCompactDisplayMode
		{
			get => _jobDisplayMode == BuildJobDisplayMode.Compact;
			set => IoC.Get<IBuildViewportService>().SetBuildJobDisplayMode(value ? BuildJobDisplayMode.Compact : BuildJobDisplayMode.Standard);
		}

		public BuildViewportServiceXamlSupport()
		{
			var viewportService = IoC.Get<IBuildViewportService>();
			viewportService.BuildJobDisplayModeChanged += this.ViewportService_BuildJobDisplayModeChanged;
		}

		private void ViewportService_BuildJobDisplayModeChanged(object sender, EventArgs e)
			=> this.JobDisplayMode = IoC.Get<IBuildViewportService>().BuildJobDisplayMode;
	}
}
