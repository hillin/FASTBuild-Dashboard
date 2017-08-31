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
			public int Value { get;  }
			public Brush Background { get; }
			public Pen Border { get; }
			public Sample(int value, Brush background, Pen border)
			{
				this.Background = background;
				this.Border = border;
				this.Value = value;
			}
		}
	}
}
