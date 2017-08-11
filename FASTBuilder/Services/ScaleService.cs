using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using FastBuilder.Support;

namespace FastBuilder.Services
{
	internal class ScaleService : IScaleService
	{
		private const double ScalingChangedEventDelay = 100;
		private const double StandardScaling = 50;
		private const double MinimumScaling = 0.4;
		private const double MaximumScaling = 1024;

		private double _scaling = StandardScaling;
		private readonly Timer _scalingChangedEventDelayTimer;

		public double Scaling
		{
			get => _scaling;
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_scaling == value)
					return;

				_scaling = Math.Min(Math.Max(value, MinimumScaling), MaximumScaling);
				_scalingChangedEventDelayTimer.Stop();
				_scalingChangedEventDelayTimer.Start();
				this.PreScalingChanging?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler PreScalingChanging;
		public event EventHandler ScalingChanged;


		public ScaleService()
		{
			_scalingChangedEventDelayTimer = new Timer(ScalingChangedEventDelay)
			{
				AutoReset = false
			};
			_scalingChangedEventDelayTimer.Elapsed += this.ScalingChangedEventDelayTimer_Elapsed;
		}

		private void ScalingChangedEventDelayTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			this.ScalingChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
