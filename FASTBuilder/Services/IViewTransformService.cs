using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuilder.Services
{
	internal interface IViewTransformService
	{
		double Scaling { get; set; }
		double ViewStartTimeOffsetSeconds { get; }
		double ViewEndTimeOffsetSeconds { get; }

		/// <summary>
		/// An event triggers when the scaling is decisively changed, after a certain delay from the last
		/// input event which triggered this scaling change
		/// </summary>
		event EventHandler ScalingChanged;

		/// <summary>
		/// An event triggers whenever the scaling is changed. Because changing scaling and updating UI is
		/// expensive, we don't want every user input that triggers a scaling change to be reflected on the 
		/// UI, so a decisive <see cref="ScalingChanged"/> event will only be triggered after a delay
		/// </summary>
		event EventHandler PreScalingChanging;

		event EventHandler<ViewTimeRangeChangeReason> ViewTimeRangeChanged;
		void SetViewTimeRange(double startTime, double endTime, ViewTimeRangeChangeReason reason);
	}
}
