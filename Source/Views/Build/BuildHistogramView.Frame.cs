using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FastBuild.Dashboard.Communication;
using FastBuild.Dashboard.ViewModels.Build;

namespace FastBuild.Dashboard.Views.Build
{
	internal partial class BuildHistogramView
	{
		private class Frame
		{
			private static readonly Dictionary<BuildJobStatus, int> StatusPriorityMap
				= new Dictionary<BuildJobStatus, int>
				{
					[BuildJobStatus.SuccessPreprocessed] = 0,
					[BuildJobStatus.Success] = 1,
					[BuildJobStatus.SuccessCached] = 2,
					[BuildJobStatus.RacedOut] = 3,
					[BuildJobStatus.Stopped] = 4,
					[BuildJobStatus.Timeout] = 5,
					[BuildJobStatus.Error] = 6,
					[BuildJobStatus.Failed] = 7,
					[BuildJobStatus.Building] = 8
				};


			private readonly List<BuildJobViewModel> _jobs
				= new List<BuildJobViewModel>();



			public List<Sample> Samples { get; }
				= new List<Sample>();

			public int SumValue
				=> this.Samples.Sum(s => s.Value);

			private bool _samplesInvalidated;

			public void AddJob(BuildJobViewModel job)
			{
				_jobs.Add(job);
				this.InvalidateSamples();
			}

			public void InvalidateSamples()
			{
				_samplesInvalidated = true;
			}

			private void UpdateSamples()
			{
				this.Samples.Clear();
				var groups = _jobs.GroupBy(j => j.Status).OrderBy(g => StatusPriorityMap[g.Key]);
				foreach (var group in groups)
				{
					var firstElement = group.First();
					var sample = new Sample(group.Count(), firstElement.UIBackground, firstElement.UIBorderPen);

					this.Samples.Add(sample);
				}

				_samplesInvalidated = false;
			}

			public void OnRender(DrawingContext dc, double x, double y, double frameWidth, double sampleScale)
			{
				if (_samplesInvalidated)
				{
					this.UpdateSamples();
				}

				foreach (var sample in this.Samples)
				{
					var height = sample.Value * sampleScale;
					y -= height;
					var rect = new Rect(x, y, frameWidth, height);
					dc.DrawRectangle(sample.Background, sample.Border, rect);
				}
			}
		}
	}
}
