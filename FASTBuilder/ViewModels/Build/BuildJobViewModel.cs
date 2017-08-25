using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Media;
using Caliburn.Micro;
using FastBuilder.Communication;
using FastBuilder.Communication.Events;
using FASTBuilder;

namespace FastBuilder.ViewModels.Build
{
	[DebuggerDisplay("Job:{" + nameof(BuildJobViewModel.DisplayName) + "}")]
	internal class BuildJobViewModel : PropertyChangedBase, IBuildJobViewModel
	{
		private static readonly Dictionary<string, Brush> CachedJobBrushes
			= new Dictionary<string, Brush>();

		private static Brush GetJobBrush(string type, BuildJobStatus status)
		{
			var key = $"Job{type}Brush_{status}";
			if (!CachedJobBrushes.TryGetValue(key, out var brush))
			{
				brush = (Brush)App.Current.FindResource(key);
				CachedJobBrushes.Add(key, brush);
			}

			return brush;
		}

		public BuildCoreViewModel OwnerCore { get; }

		// double linked-list structure
		public BuildJobViewModel PreviousJob { get; }
		public IBuildJobViewModel NextJob { get; private set; }

		private static string GenerateDisplayName(string eventName) => Path.GetFileName(eventName) ?? eventName;

		private BuildJobStatus _status;
		private double _elapsedSeconds;
		public DateTime StartTime { get; }
		public double StartTimeOffset { get; }

		public DateTime EndTime => this.StartTime.AddSeconds(this.ElapsedSeconds);

		public double EndTimeOffset => this.StartTimeOffset + this.ElapsedSeconds;

		public double ElapsedSeconds
		{
			get => _elapsedSeconds;
			private set
			{
				if (value.Equals(_elapsedSeconds))
				{
					return;
				}

				_elapsedSeconds = value;
				this.NotifyOfPropertyChange();
				this.NotifyOfPropertyChange(nameof(this.EndTime));
				this.NotifyOfPropertyChange(nameof(this.EndTimeOffset));
				this.NotifyOfPropertyChange(nameof(this.ToolTipText));
			}
		}


		public Brush UIForeground
			=> BuildJobViewModel.GetJobBrush("Foreground", this.Status);

		public Brush UIBackground 
			=> BuildJobViewModel.GetJobBrush("Background", this.Status);

		public Brush UIBorderBrush 
			=> BuildJobViewModel.GetJobBrush("Border", this.Status);

		public bool IsFinished => this.Status != BuildJobStatus.Building;
		public string EventName { get; }
		public string DisplayName { get; }

		public string ToolTipText
		{
			get
			{
				var builder = new StringBuilder();
				builder.AppendLine(this.EventName);
				builder.AppendLine($"Started: {this.StartTime}");

				switch (this.Status)
				{
					case BuildJobStatus.Building:
						builder.Append("Building");
						break;
					case BuildJobStatus.Success:
						builder.Append("Successfully built");
						break;
					case BuildJobStatus.SuccessCached:
						builder.Append("Successfully built (cache hit)");
						break;
					case BuildJobStatus.SuccessPreprocessed:
						builder.Append("Successfully preprocessed");
						break;
					case BuildJobStatus.Failed:
						builder.Append("Failed");
						break;
					case BuildJobStatus.Error:
						builder.Append("Error occurred");
						break;
					case BuildJobStatus.Timeout:
						builder.Append("Timed out");
						break;
					case BuildJobStatus.RacedOut:
						builder.Append("Deprecated by local race");
						break;
					case BuildJobStatus.Stopped:
						builder.Append("Stopped");
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				builder.AppendLine($" ({this.ElapsedSeconds:0.#} seconds elapsed)");

				if (!string.IsNullOrWhiteSpace(this.Message))
				{
					builder.AppendLine(this.Message);
				}

				return builder.ToString();
			}
		}


		public string Message { get; private set; }

		public BuildJobStatus Status
		{
			get => _status;
			private set
			{
				if (value == _status)
				{
					return;
				}

				_status = value;
				this.NotifyOfPropertyChange();
				this.NotifyOfPropertyChange(nameof(this.IsFinished));
				this.NotifyOfPropertyChange(nameof(this.ElapsedSeconds));
				this.NotifyOfPropertyChange(nameof(this.ToolTipText));
				this.NotifyOfPropertyChange(nameof(this.UIForeground));
				this.NotifyOfPropertyChange(nameof(this.UIBackground));
				this.NotifyOfPropertyChange(nameof(this.UIBorderBrush));
			}
		}

		public BuildJobViewModel(BuildCoreViewModel ownerCore, StartJobEventArgs e, BuildJobViewModel previousJob = null)
		{
			this.OwnerCore = ownerCore;
			this.PreviousJob = previousJob;
			if (previousJob != null)
			{
				previousJob.NextJob = this;
			}

			this.EventName = e.EventName;
			this.DisplayName = BuildJobViewModel.GenerateDisplayName(this.EventName);
			this.StartTime = e.Time;
			this.StartTimeOffset = (e.Time - ownerCore.OwnerWorker.OwnerSession.StartTime).TotalSeconds;
			this.Status = BuildJobStatus.Building;
		}


		public void OnFinished(FinishJobEventArgs e)
		{
			// already raced out
			if (this.Status == BuildJobStatus.RacedOut)
			{
				return;
			}

			this.Message = e.Message;
			this.ElapsedSeconds = (e.Time - this.StartTime).TotalSeconds;
			this.Status = e.Result;
		}


		public void InvalidateCurrentTime(double currentTimeOffset)
		{
			if (this.IsFinished)
			{
				return;
			}

			this.UpdateDuration(currentTimeOffset);
		}

		private void UpdateDuration(double currentTimeOffset)
		{
			this.ElapsedSeconds = currentTimeOffset - this.StartTimeOffset;
		}

		public void OnSessionStopped(double currentTimeOffset)
		{
			if (!this.IsFinished)
			{
				this.Status = BuildJobStatus.Stopped;
				this.ElapsedSeconds = currentTimeOffset - this.StartTimeOffset;
			}
		}

		public void Tick(double currentTimeOffset)
		{
			this.InvalidateCurrentTime(currentTimeOffset);
		}
	}
}
