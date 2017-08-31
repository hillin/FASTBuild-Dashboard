using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FastBuild.Dashboard.Views.Build
{
	internal partial class BuildHistogramView
	{
		private class Sample
		{
			public double Value { get; }
			public Brush Background { get; }
			public Brush Border { get; }
			public Sample(double value, Brush background, Brush border)
			{
				this.Value = value;
				this.Background = background;
				this.Border = border;
			}
		}
	}
}
