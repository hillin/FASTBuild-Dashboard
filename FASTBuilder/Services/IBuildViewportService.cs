using System;

namespace FastBuilder.Services
{
	internal interface IBuildViewportService
	{
		double Scaling { get; set; }
		double ViewStartTimeOffsetSeconds { get; }
		double ViewEndTimeOffsetSeconds { get; }

		double ViewTop { get; }
		double ViewBottom { get; }

		event EventHandler ScalingChanging;

		event EventHandler ViewTimeRangeChanged;
		event EventHandler VerticalViewRangeChanged;
		void SetViewTimeRange(double startTime, double endTime);
		void SetVerticalViewRange(double top, double bottom);

	}
}
