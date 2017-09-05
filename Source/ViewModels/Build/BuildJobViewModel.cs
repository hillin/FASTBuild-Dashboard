using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Caliburn.Micro;
using FastBuild.Dashboard.Communication;
using FastBuild.Dashboard.Communication.Events;

namespace FastBuild.Dashboard.ViewModels.Build
{
	[DebuggerDisplay("Job:{" + nameof(BuildJobViewModel.DisplayName) + "}")]
	internal class BuildJobViewModel : PropertyChangedBase, IBuildJobViewModel
	{
		private static string GenerateDisplayName(string eventName)
			=> Path.GetFileName(eventName) ?? eventName;


		public BuildCoreViewModel OwnerCore { get; }

		// double linked-list structure
		public BuildJobViewModel PreviousJob { get; }
		public IBuildJobViewModel NextJob { get; private set; }


		private BuildJobStatus _status;
		private double _elapsedSeconds;
		private Brush _uiForeground;
		private Brush _uiBackground;
		private Brush _uiBorderBrush;
		private Pen _uiBorderPen;
		private string _message;
		private IEnumerable<BuildErrorGroup> _errorGroups;
		public DateTime StartTime { get; }
		public string DisplayStartTime => $"Started: {this.StartTime}";
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
				this.NotifyOfPropertyChange(nameof(this.DisplayElapsedSeconds));
			}
		}
		public string DisplayElapsedSeconds => $"{this.ElapsedSeconds:0.#} seconds elapsed";

		public Brush UIForeground
		{
			get => _uiForeground;
			private set
			{
				if (object.Equals(value, _uiForeground))
				{
					return;
				}

				_uiForeground = value;
				this.NotifyOfPropertyChange();
			}
		}

		public Brush UIBackground
		{
			get => _uiBackground;
			private set
			{
				if (object.Equals(value, _uiBackground))
				{
					return;
				}

				_uiBackground = value;
				this.NotifyOfPropertyChange();
			}
		}

		public Brush UIBorderBrush
		{
			get => _uiBorderBrush;
			private set
			{
				if (object.Equals(value, _uiBorderBrush))
				{
					return;
				}

				_uiBorderBrush = value;
				this.NotifyOfPropertyChange();
			}
		}

		public Pen UIBorderPen
		{
			get => _uiBorderPen;
			private set
			{
				if (object.Equals(value, _uiBorderPen))
				{
					return;
				}

				_uiBorderPen = value;
				this.NotifyOfPropertyChange();
			}
		}

		public bool IsFinished => this.Status != BuildJobStatus.Building;
		public string EventName { get; }
		public string DisplayName { get; }

		public string Message
		{
			get => _message;
			private set
			{
				if (value == _message)
				{
					return;
				}

				_message = value;
				this.NotifyOfPropertyChange();
				this.NotifyOfPropertyChange(nameof(this.HasError));
				this.NotifyOfPropertyChange(nameof(this.ShouldShowErrorMessage));
			}
		}

		public IEnumerable<BuildErrorGroup> ErrorGroups
		{
			get => _errorGroups;
			private set
			{
				if (object.Equals(value, _errorGroups))
				{
					return;
				}

				_errorGroups = value;
				this.NotifyOfPropertyChange();
			}
		}

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
				this.NotifyOfPropertyChange(nameof(this.DisplayStatus));
				this.NotifyOfPropertyChange(nameof(this.HasError));
				this.NotifyOfPropertyChange(nameof(this.ShouldShowErrorMessage));
				this.UpdateUIBrushes();
			}
		}

		public bool HasError => this.Status == BuildJobStatus.Error
								|| this.Status == BuildJobStatus.Failed;

		public bool ShouldShowErrorMessage => this.HasError
											  && !string.IsNullOrEmpty(this.Message);

		public string DisplayStatus
		{
			get
			{
				switch (this.Status)
				{
					case BuildJobStatus.Building:
						return "Building";
					case BuildJobStatus.Success:
						return "Successfully Built";
					case BuildJobStatus.SuccessCached:
						return "Successfully (Cache Hit)";
					case BuildJobStatus.SuccessPreprocessed:
						return "Successfully Preprocessed";
					case BuildJobStatus.Failed:
						return "Failed";
					case BuildJobStatus.Error:
						return "Error Occurred";
					case BuildJobStatus.Timeout:
						return "Timed Out";
					case BuildJobStatus.RacedOut:
						return "Deprecated by Local Race";
					case BuildJobStatus.Stopped:
						return "Stopped";
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private void UpdateUIBrushes()
		{
			this.UIForeground = App.CachedResource<Brush>.GetResource($"JobForegroundBrush_{this.Status}");
			this.UIBackground = App.CachedResource<Brush>.GetResource($"JobBackgroundBrush_{this.Status}");
			this.UIBorderBrush = App.CachedResource<Brush>.GetResource($"JobBorderBrush_{this.Status}");
			this.UIBorderPen = App.CachedResource<Pen>.GetResource($"JobBorderPen_{this.Status}");
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
			this.UpdateUIBrushes();
		}


		public void OnFinished(FinishJobEventArgs e)
		{
			// already raced out
			if (this.Status == BuildJobStatus.RacedOut)
			{
				return;
			}

			if (e.Message != null)
			{
				var message = e.Message.TrimEnd().Replace('\f', '\n');
				var matches = Regex.Matches(message, @"^(.+)\((\d+)\)\s*\: (.+)$", RegexOptions.Multiline);
				if (matches.Count > 0)
				{
					this.ErrorGroups = matches.Cast<Match>()
						.Select(m => new BuildErrorInfo(
							m.Groups[1].Value,
							int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture),
							m.Groups[3].Value,
							this.OwnerCore.OwnerWorker.OwnerSession.InitiatorProcess))
						.GroupBy(i => i.FilePath, (file, group) => new BuildErrorGroup(file, group))
						.ToArray();

					this.Message = null; // we don't show both message and parsed errors
				}
				else
				{
					this.Message = message;
				}
			}

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
