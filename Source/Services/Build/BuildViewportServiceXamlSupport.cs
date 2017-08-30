using System;
using Caliburn.Micro;

namespace FastBuild.Dashboard.Services.Build
{
	internal class BuildViewportServiceXamlSupport : PropertyChangedBase
	{
		public static BuildViewportServiceXamlSupport Instance { get; }

		static BuildViewportServiceXamlSupport() 
			=> BuildViewportServiceXamlSupport.Instance = new BuildViewportServiceXamlSupport();


		public BuildJobDisplayMode JobDisplayMode => IoC.Get<IBuildViewportService>().BuildJobDisplayMode;

		public bool IsCompactDisplayMode
		{
			get => this.JobDisplayMode == BuildJobDisplayMode.Compact;
			set => IoC.Get<IBuildViewportService>().SetBuildJobDisplayMode(value ? BuildJobDisplayMode.Compact : BuildJobDisplayMode.Standard);
		}

		public BuildViewportServiceXamlSupport()
		{
			var viewportService = IoC.Get<IBuildViewportService>();
			viewportService.BuildJobDisplayModeChanged += this.ViewportService_BuildJobDisplayModeChanged;
		}

		private void ViewportService_BuildJobDisplayModeChanged(object sender, EventArgs e)
		{
			this.NotifyOfPropertyChange(nameof(this.JobDisplayMode));
			this.NotifyOfPropertyChange(nameof(this.IsCompactDisplayMode));
		}
	}
}
