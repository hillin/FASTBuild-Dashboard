using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using FastBuilder.Communication;
using FastBuilder.Communication.Events;
using FastBuilder.Services;

namespace FastBuilder.ViewModels
{
	internal class BuildJobViewModel : PropertyChangedBase
	{
		public BuildCoreViewModel OwnerCore { get; }

		private static string GenerateDisplayName(string eventName)
		{
			return Path.GetFileName(eventName) ?? eventName;
		}

		private static double GetUIScaling()
		{
			return IoC.Get<IViewTransformService>().Scaling;
		}
		
		private BuildJobStatus _status;
		private double _elapsedSeconds;
		private double _uiWidth;
		public DateTime StartTime { get; }
		public double StartTimeOffset { get; }

		public DateTime EndTime => this.StartTime.AddSeconds(this.ElapsedSeconds);

		public double EndTimeOffset => this.StartTimeOffset + this.ElapsedSeconds;
		
		public double ElapsedSeconds
		{
			get => _elapsedSeconds;
			private set
			{
				if (value.Equals(_elapsedSeconds)) return;
				_elapsedSeconds = value;
				this.NotifyOfPropertyChange();
				this.UpdateUIWidth();
				this.NotifyOfPropertyChange(nameof(this.EndTime));
				this.NotifyOfPropertyChange(nameof(this.EndTimeOffset));
				this.NotifyOfPropertyChange(nameof(this.ToolTipText));
			}
		}

		public double UIWidth
		{
			get => _uiWidth;
			set
			{
				if (value.Equals(_uiWidth)) return;
				_uiWidth = value;
				this.NotifyOfPropertyChange();
				this.NotifyOfPropertyChange(nameof(this.ShouldShowText));
			}
		}

		public double UILeft => Math.Max(0.0, this.StartTimeOffset) *
								BuildJobViewModel.GetUIScaling();

		public bool ShouldShowText => this.UIWidth >= 48;

		public Brush UIForeground
		{
			get
			{
				switch (this.Status)
				{
					case BuildJobStatus.Building:
						return Brushes.Black;
					default:
						return Brushes.White;
				}
			}
		}

		public Brush UIBackground
		{
			get
			{
				switch (this.Status)
				{
					case BuildJobStatus.Building:
						return Brushes.White;
					case BuildJobStatus.Success:
						return Brushes.DarkGreen;
					case BuildJobStatus.SuccessCached:
						return Brushes.DarkCyan;
					case BuildJobStatus.SuccessPreprocessed:
						return Brushes.Green;
					case BuildJobStatus.Failed:
						return Brushes.Crimson;
					case BuildJobStatus.Error:
						return Brushes.Crimson;
					case BuildJobStatus.Timeout:
						return Brushes.DarkOrange;
					case BuildJobStatus.Stopped:
						return Brushes.DarkOrange;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public Brush UIBorderBrush
		{
			get
			{
				switch (this.Status)
				{
					case BuildJobStatus.Building:
						return Brushes.DarkGreen;
					default:
						return this.UIBackground;
				}
			}
		}

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
					case BuildJobStatus.Stopped:
						builder.Append("Stopped");
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				builder.AppendLine($" ({this.ElapsedSeconds:0.#} seconds elapsed)");

				if (!string.IsNullOrWhiteSpace(this.Message))
					builder.AppendLine(this.Message);

				return builder.ToString();
			}
		}


		public string Message { get; private set; }

		public BuildJobStatus Status
		{
			get => _status;
			private set
			{
				if (value == _status) return;
				_status = value;
				this.NotifyOfPropertyChange();
				this.NotifyOfPropertyChange(nameof(this.IsFinished));
				this.NotifyOfPropertyChange(nameof(this.ElapsedSeconds));
				this.NotifyOfPropertyChange(nameof(this.UIWidth));
				this.NotifyOfPropertyChange(nameof(this.ToolTipText));
				this.NotifyOfPropertyChange(nameof(this.UIForeground));
				this.NotifyOfPropertyChange(nameof(this.UIBackground));
				this.NotifyOfPropertyChange(nameof(this.UIBorderBrush));
			}
		}

		public BuildJobViewModel(BuildCoreViewModel ownerCore, StartJobEventArgs e, DateTime sessionStartTime)
		{
			this.OwnerCore = ownerCore;
			this.EventName = e.EventName;
			this.DisplayName = BuildJobViewModel.GenerateDisplayName(this.EventName);
			this.StartTime = e.Time;
			this.StartTimeOffset = (e.Time - sessionStartTime).TotalSeconds;
			this.Status = BuildJobStatus.Building;

			var viewTransformService = IoC.Get<IViewTransformService>();
			viewTransformService.ScalingChanged += this.OnScalingChanged;
		}

		private void OnScalingChanged(object sender, EventArgs e)
		{
			this.NotifyOfPropertyChange(nameof(this.UILeft));
			this.UpdateUIWidth();
		}

		public void OnFinished(FinishJobEventArgs e)
		{
			this.Message = e.Message;
			this.ElapsedSeconds = (e.Time - this.StartTime).TotalSeconds;
			this.Status = e.Result;
		}


		public void InvalidateCurrentTime(double currentTimeOffset)
		{
			if (this.IsFinished)
				return;

			this.UpdateDuration(currentTimeOffset);
		}

		private void UpdateDuration(double currentTimeOffset)
		{
			this.ElapsedSeconds = currentTimeOffset - this.StartTimeOffset;
		}

		private void UpdateUIWidth()
		{
			this.UIWidth = Math.Max(0.0, Math.Min(this.ElapsedSeconds, 60 * 60 * 24)) * BuildJobViewModel.GetUIScaling();
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
