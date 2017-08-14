using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using FastBuilder.Services;

namespace FastBuilder.ViewModels
{

	// todo: this component should be optimized in performance
	internal class TimeRulerViewModel : PropertyChangedBase
	{
		private readonly BuildSessionViewModel _ownerSession;

		private struct GrainStop
		{
			public double Scaling { get; }
			public double MajorInterval { get; }
			public double MinorInterval { get; }

			public GrainStop(double scaling, double majorInterval, double minorInterval)
			{
				this.Scaling = scaling;
				this.MajorInterval = majorInterval;
				this.MinorInterval = minorInterval;
			}
		}

		private static readonly GrainStop[] GrainStops =
		{
			new GrainStop(0.44, 300, 60),
			new GrainStop(0.67, 120, 60),
			new GrainStop(1.33, 60, 30),
			new GrainStop(2.67, 30, 10),
			new GrainStop(7.62, 10, 5),
			new GrainStop(16, 5, 1),
			new GrainStop(44, 2, 1),
			new GrainStop(88.89, 1, 0.5),
			new GrainStop(177.7, 0.5, 0.1),
			new GrainStop(320, 0.2, 0.1),
			new GrainStop(640, 0.1, 0.05)
		};

		private static void EnsureTickCount<T>(IList<T> ticks, int count)
			where T : TimeRulerTickViewModelBase, new()
		{
			lock (ticks)
			{
				for (var i = ticks.Count; i < count; ++i)
				{
					ticks.Add(new T());
				}

				for (var i = ticks.Count - 1; i >= count; --i)
				{
					ticks.RemoveAt(i);
				}
			}
		}

		private static string GetLabelTextFormat(double totalSeconds, double interval)
		{
			var formatBuilder = new StringBuilder();
			if (totalSeconds >= 3600)
				formatBuilder.Append(@"h\:m");

			formatBuilder.Append(@"m\:ss");

			if (interval % 1 > 0)
			{
				formatBuilder.Append(@"\.");

				for (var i = 0; i < 2; ++i)
				{
					if (interval % Math.Pow(10, i) > 0)
					{
						formatBuilder.Append('f');
					}
				}
			}

			return formatBuilder.ToString();
		}

		public BindableCollection<TimeRulerMajorTickViewModel> MajorTicks { get; }
			= new BindableCollection<TimeRulerMajorTickViewModel>();

		public BindableCollection<TimeRulerMinorTickViewModel> MinorTicks { get; }
			= new BindableCollection<TimeRulerMinorTickViewModel>();

		public TimeRulerViewModel(BuildSessionViewModel ownerSession)
		{
			_ownerSession = ownerSession;
			IoC.Get<IScaleService>().PreScalingChanging += this.OnPreScalingChanging;
		}

		private void OnPreScalingChanging(object sender, EventArgs e)
		{
			this.UpdateTicks();
		}

		private void UpdateTicks()
		{
			var duration = _ownerSession.CurrentTime - _ownerSession.StartTime;
			var seconds = duration.TotalSeconds;
			if (seconds <= 0)
				return;

			var scaling = IoC.Get<IScaleService>().Scaling;
			var grain = this.CalculateGrain(scaling);

			var majorTickCount = (int)Math.Floor(seconds / grain.MajorInterval) + 1;
			var minorTickCount = (int)Math.Floor(seconds / grain.MinorInterval) + 1 - majorTickCount;

			TimeRulerViewModel.EnsureTickCount(this.MajorTicks, majorTickCount);
			TimeRulerViewModel.EnsureTickCount(this.MinorTicks, minorTickCount);

			var tickWidth = grain.MajorInterval * scaling;

			var labelTextFormat = TimeRulerViewModel.GetLabelTextFormat(seconds, grain.MajorInterval);

			for (var i = 0; i < this.MajorTicks.Count; ++i)
			{
				var tick = this.MajorTicks[i];
				var tickSeconds = i * grain.MajorInterval;
				tick.Time = TimeSpan.FromSeconds(tickSeconds);
				tick.LabelText = tick.Time.ToString(labelTextFormat);
				tick.UILeft = tickSeconds * scaling - tickWidth / 2.0;
				tick.UIWidth = tickWidth;
			}

			var majorToMinorRatio = (int)(grain.MajorInterval / grain.MinorInterval);

			tickWidth = grain.MinorInterval * scaling;

			var steps = 0;
			for (var i = 0; i < this.MinorTicks.Count; ++i, ++steps)
			{
				if (steps % majorToMinorRatio == 0)
					++steps;

				var tick = this.MinorTicks[i];
				var tickSeconds = steps * grain.MinorInterval;
				tick.Time = TimeSpan.FromSeconds(tickSeconds);
				tick.UILeft = tickSeconds * scaling - tickWidth / 2.0;
				tick.UIWidth = tickWidth;
			}
		}


		private GrainStop CalculateGrain(double scaling)
		{
			foreach (var grainStop in TimeRulerViewModel.GrainStops)
			{
				if (scaling <= grainStop.Scaling)
				{
					return grainStop;
				}
			}

			return GrainStops.Last();
		}

		public void Tick(DateTime now)
		{
			this.UpdateTicks();
		}
	}
}
