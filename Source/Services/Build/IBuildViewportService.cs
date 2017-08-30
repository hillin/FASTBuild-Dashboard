using System;

namespace FastBuild.Dashboard.Services.Build
{
	internal interface IBuildViewportService
	{
		double Scaling { get; set; }
		double ViewStartTimeOffsetSeconds { get; }
		double ViewEndTimeOffsetSeconds { get; }

		double ViewTop { get; }
		double ViewBottom { get; }
		BuildJobDisplayMode BuildJobDisplayMode { get; }

		event EventHandler ScalingChanged;
		event EventHandler ViewTimeRangeChanged;
		event EventHandler VerticalViewRangeChanged;
		event EventHandler BuildJobDisplayModeChanged;
		void SetViewTimeRange(double startTime, double endTime);
		void SetVerticalViewRange(double top, double bottom);
		void SetBuildJobDisplayMode(BuildJobDisplayMode mode);
	}
}
