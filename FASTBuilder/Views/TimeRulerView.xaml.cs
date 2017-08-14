using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Caliburn.Micro;
using FastBuilder.Services;
using FastBuilder.ViewModels;

namespace FastBuilder.Views
{
	public partial class TimeRulerView
	{
		private struct GrainStop
		{
			private const double MinimumMajorTickDistance = 160;

			public double Scaling { get; }
			public double MajorInterval { get; }
			public double MinorInterval { get; }

			public GrainStop(double majorInterval, double minorInterval)
			{
				this.Scaling = MinimumMajorTickDistance / majorInterval;
				this.MajorInterval = majorInterval;
				this.MinorInterval = minorInterval;
			}
		}

		private static readonly GrainStop[] GrainStops =
		{
			new GrainStop(300, 60),
			new GrainStop(120, 60),
			new GrainStop(60, 30),
			new GrainStop(30, 10),
			new GrainStop(10, 5),
			new GrainStop(5, 1),
			new GrainStop(2, 1),
			new GrainStop(1, 0.5),
			new GrainStop(0.5, 0.1),
			new GrainStop(0.2, 0.1),
			new GrainStop(0.1, 0.05)
		};

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

		private readonly List<TimeRulerMajorTickView> _majorTickPool
			= new List<TimeRulerMajorTickView>();

		private int _majorTickPoolIndex;

		private readonly List<TimeRulerMinorTickView> _minorTickPool
			= new List<TimeRulerMinorTickView>();

		private int _minorTickPoolIndex;

		public TimeRulerView()
		{
			InitializeComponent();
			var viewTransformService = IoC.Get<IViewTransformService>();
			viewTransformService.PreScalingChanging += this.OnPreScalingChanging;
			viewTransformService.ViewTimeRangeChanged += ViewTransformService_ViewTimeRangeChanged;
		}

		private void ViewTransformService_ViewTimeRangeChanged(object sender, ViewTimeRangeChangeReason viewTimeRangeChangeReason)
		{
			this.UpdateTicks();
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
			this.UpdateTicks();
		}

		private void OnPreScalingChanging(object sender, EventArgs e)
		{
			this.UpdateTicks();
		}

		private GrainStop CalculateGrain(double scaling)
		{
			foreach (var grainStop in GrainStops)
			{
				if (scaling <= grainStop.Scaling)
				{
					return grainStop;
				}
			}

			return GrainStops.Last();
		}

		private TimeRulerMajorTickView PoolGetMajorTick()
		{
			if (_majorTickPoolIndex == _majorTickPool.Count)
			{
				var newTick = new TimeRulerMajorTickView();
				this.Canvas.Children.Add(newTick);
				_majorTickPool.Add(newTick);
			}

			var tick = _majorTickPool[_majorTickPoolIndex];
			tick.Visibility = Visibility.Visible;
			++_majorTickPoolIndex;
			return tick;
		}

		private TimeRulerMinorTickView PoolGetMinorTick()
		{
			if (_minorTickPoolIndex == _minorTickPool.Count)
			{
				var newTick = new TimeRulerMinorTickView();
				this.Canvas.Children.Add(newTick);
				_minorTickPool.Add(newTick);
			}

			var tick = _minorTickPool[_minorTickPoolIndex];
			tick.Visibility = Visibility.Visible;
			++_minorTickPoolIndex;
			return tick;
		}


		private void UpdateTicks()
		{
			if (DesignerProperties.GetIsInDesignMode(this))
				return;
			
			var viewTransformService = IoC.Get<IViewTransformService>();

			var startTime = viewTransformService.ViewStartTimeOffsetSeconds;
			var endTime = viewTransformService.ViewEndTimeOffsetSeconds;
			var duration = endTime - startTime;
			
			var scaling = viewTransformService.Scaling;
			var grain = this.CalculateGrain(scaling);

			_majorTickPoolIndex = 0;
			_minorTickPoolIndex = 0;

			var positiveStartTime = Math.Max(0, startTime);

			var firstMajorTickTime = positiveStartTime - positiveStartTime % grain.MajorInterval;
			var firstMinorTickTime = positiveStartTime - positiveStartTime % grain.MinorInterval;
			
			var lastMajorTickTime = endTime + grain.MajorInterval - endTime % grain.MajorInterval;
			var lastMinorTickTime = endTime + grain.MinorInterval - endTime % grain.MajorInterval;

			var realStartTime = Math.Min(firstMinorTickTime, firstMajorTickTime);
			var realEndTime = Math.Max(lastMajorTickTime, lastMinorTickTime);

			var labelTextFormat = TimeRulerView.GetLabelTextFormat(duration, grain.MajorInterval);
			var tickWidth = grain.MajorInterval * scaling;

			for (var time = realStartTime; time <= realEndTime; time += grain.MajorInterval)
			{
				var tick = this.PoolGetMajorTick();
				tick.Width = tickWidth;
				tick.SetTime(TimeSpan.FromSeconds(time), labelTextFormat);
				Canvas.SetLeft(tick, (time - startTime) * scaling - tickWidth / 2.0 );
			}

			tickWidth = grain.MinorInterval * scaling;

			for (var time = realStartTime; time >= 0 && time <= realEndTime; time += grain.MinorInterval)
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (time % grain.MajorInterval == 0)
					continue;

				var tick = this.PoolGetMinorTick();
				tick.Width = tickWidth;
				Canvas.SetLeft(tick, (time - startTime) * scaling - tickWidth / 2.0 );
			}

			for (var i = _majorTickPoolIndex; i < _majorTickPool.Count; ++i)
			{
				_majorTickPool[i].Visibility = Visibility.Hidden;
			}

			for (var i = _minorTickPoolIndex; i < _minorTickPool.Count; ++i)
			{
				_minorTickPool[i].Visibility = Visibility.Hidden;
			}
		}
	}
}
