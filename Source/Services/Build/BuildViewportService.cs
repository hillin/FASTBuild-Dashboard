using System;
using System.Diagnostics.CodeAnalysis;
using FastBuild.Dashboard.Configuration;

namespace FastBuild.Dashboard.Services.Build
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
				this.ScalingChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public double ViewStartTimeOffsetSeconds { get; private set; }
		public double ViewEndTimeOffsetSeconds { get; private set; }
		public double ViewTop { get; private set; }
		public double ViewBottom { get; private set; }

		public BuildJobDisplayMode BuildJobDisplayMode
		{
			get => (BuildJobDisplayMode)Profile.Default.BuildJobDisplayMode;
			private set
			{
				Profile.Default.BuildJobDisplayMode = (int)value;
				Profile.Default.Save();
			}
		}

		public event EventHandler ScalingChanged;
		public event EventHandler ViewTimeRangeChanged;
		public event EventHandler VerticalViewRangeChanged;
		public event EventHandler BuildJobDisplayModeChanged;


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

		public void SetBuildJobDisplayMode(BuildJobDisplayMode mode)
		{
			this.BuildJobDisplayMode = mode;
			this.BuildJobDisplayModeChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
