using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuild.Dashboard.Views.Build
{
	internal partial class BuildHistogramView
	{
		private class Frame
		{
			public Sample[] Samples { get; }
			public double SumValue { get; }
			public Frame(Sample[] samples)
			{
				this.Samples = samples;
				this.SumValue = this.Samples.Sum(s => s.Value);
			}
		}
	}
}
