using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FastBuilder.Support;

namespace FastBuilder.Services
{
	internal class ScaleService : IScaleService
	{
		public static ICommand ScaleCommand = new SimpleCommand(ScaleService.ExecuteScaleCommand);

		private static void ExecuteScaleCommand(object obj)
		{
			
		}

		private double _scaling = 50;

		public double Scaling
		{
			get => _scaling;
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_scaling == value)
					return;

				_scaling = value;
				this.ScalingChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler ScalingChanged;

		
	}
}
