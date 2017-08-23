using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuilder.Support
{
	internal static class MathEx
	{
		public static double Clamp(double value, double min, double max)
		{
			return value > max ? max : value < min ? min : value;
		}
	}
}
