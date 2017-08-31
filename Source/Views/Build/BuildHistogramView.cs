using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FastBuild.Dashboard.ViewModels.Build;

namespace FastBuild.Dashboard.Views.Build
{
	internal partial class BuildHistogramView : BuildSessionCustomRenderingControlBase
	{
		private const double FrameInterval = 0.1;
		private const double MaximumFrameWidth = 5;

		private readonly List<Frame> _frames
			= new List<Frame>();

		private double _lastFrameEndTimeOffset;

		protected override void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			base.OnDataContextChanged(sender, e);

			_lastFrameEndTimeOffset = 0;

			this.InvalidateVisual();
		}

		protected override void Tick(double timeOffset)
		{
			base.Tick(timeOffset);

			if (_frames.Count == 0
				&& timeOffset - _lastFrameEndTimeOffset > FrameInterval * 2)
			{
				this.BatchCreateFrames(timeOffset);
			}
			else
			{
				while (timeOffset >= _lastFrameEndTimeOffset + FrameInterval)
				{
					this.CreateFrame(_lastFrameEndTimeOffset);
					_lastFrameEndTimeOffset += FrameInterval;
				}
			}

			this.InvalidateVisual();
		}

		protected override void OnJobFinished(BuildJobViewModel job)
		{
			base.OnJobFinished(job);

			foreach (var frame in EnumFrames(job.StartTimeOffset, job.EndTimeOffset))
			{
				frame.InvalidateSamples();
			}

			this.InvalidateVisual();
		}

		private IEnumerable<Frame> EnumFrames(double startTimeOffset, double endTimeOffset)
		{
			var startFrameIndex = (int)Math.Floor(startTimeOffset / FrameInterval);
			var endFrameIndex = Math.Min((int)Math.Ceiling(endTimeOffset / FrameInterval), _frames.Count - 1);
			for (var i = startFrameIndex; i <= endFrameIndex; ++i)
			{
				yield return _frames[i];
			}
		}

		private void BatchCreateFrames(double currentTimeOffset)
		{
			_frames.Clear();
			for (var time = 0.0; time + FrameInterval <= currentTimeOffset; time += FrameInterval)
			{
				_frames.Add(new Frame());
			}

			foreach (var job in this.JobManager.EnumerateJobs(0, currentTimeOffset))
			{
				foreach (var frame in EnumFrames(job.StartTimeOffset, job.EndTimeOffset))
				{
					frame.AddJob(job);
				}
			}
		}

		private void CreateFrame(double fromTimeOffset)
		{
			var frame = new Frame();
			var jobs = this.JobManager.EnumerateJobs(fromTimeOffset, fromTimeOffset + FrameInterval);
			foreach (var job in jobs)
			{
				frame.AddJob(job);
			}

			_frames.Add(frame);
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			if (_frames.Count == 0)
			{
				return;
			}

			var totalTime = this.SessionViewModel.ElapsedTime;
			var frameWidth = Math.Min(MaximumFrameWidth, this.ActualWidth / _frames.Count);

			var x = 0.0;
			var y = this.ActualHeight;

			var maxFrameValue = _frames.Max(f => f.SumValue);
			var sampleScale = (this.ActualHeight * 0.8) / maxFrameValue;

			foreach (var frame in _frames)
			{
				frame.OnRender(dc, x, y, frameWidth, sampleScale);
				x += frameWidth;
			}
		}
	}
}
