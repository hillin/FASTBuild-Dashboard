using System;
using System.Diagnostics.CodeAnalysis;
using System.Timers;

namespace FastBuilder.Services
{
	internal class BuildViewportService : IBuildViewportService
	{
		private const double StandardScaling = 50;
		private const double MinimumScaling = 0.4;
		private const double MaximumScaling = 1024;

		private double _scaling = StandardScaling;

		public double Scaling
		{
			get => _scaling;
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_scaling == value)
				{
					return;
				}

				_scaling = Math.Min(Math.Max(value, MinimumScaling), MaximumScaling);
				this.ScalingChanging?.Invoke(this, EventArgs.Empty);
			}
		}

		public double ViewStartTimeOffsetSeconds { get; private set; }
		public double ViewEndTimeOffsetSeconds { get; private set; }
		public double ViewTop { get; private set; }
		public double ViewBottom { get; private set; }

		public event EventHandler ScalingChanging;
		public event EventHandler ViewTimeRangeChanged;
		public event EventHandler VerticalViewRangeChanged;


		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public void SetViewTimeRange(double startTime, double endTime)
		{
			if (this.ViewStartTimeOffsetSeconds == startTime && this.ViewEndTimeOffsetSeconds == endTime)
			{
				return;
			}

			this.ViewStartTimeOffsetSeconds = startTime;
			this.ViewEndTimeOffsetSeconds = endTime;
			this.ViewTimeRangeChanged?.Invoke(this, EventArgs.Empty);
		}


		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public void SetVerticalViewRange(double top, double bottom)
		{
			if (this.ViewTop == top && this.ViewBottom == bottom)
			{
				return;
			}

			this.ViewTop = top;
			this.ViewBottom = bottom;
			this.VerticalViewRangeChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
