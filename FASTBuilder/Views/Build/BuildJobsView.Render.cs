using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FastBuilder.Services;
using FastBuilder.Support;
using FastBuilder.ViewModels.Build;

namespace FastBuilder.Views.Build
{
	partial class BuildJobsView
	{

		public Thickness JobMargin
		{
			get => (Thickness)GetValue(JobMarginProperty);
			set => SetValue(JobMarginProperty, value);
		}

		public static readonly DependencyProperty JobMarginProperty =
			DependencyProperty.Register("JobMargin", typeof(Thickness), typeof(BuildJobsView), new UIPropertyMetadata(new Thickness(2), BuildJobsView.AffectsVisual));

		public Thickness JobTextMargin
		{
			get => (Thickness)GetValue(JobTextMarginProperty);
			set => SetValue(JobTextMarginProperty, value);
		}

		public static readonly DependencyProperty JobTextMarginProperty =
			DependencyProperty.Register("JobTextMargin", typeof(Thickness), typeof(BuildJobsView), new UIPropertyMetadata(new Thickness(8, 2, 2, 2), BuildJobsView.AffectsVisual));

		public Style JobTextStyle
		{
			get => (Style)GetValue(JobTextStyleProperty);
			set => SetValue(JobTextStyleProperty, value);
		}

		public static readonly DependencyProperty JobTextStyleProperty =
			DependencyProperty.Register("JobTextStyle", typeof(Style), typeof(BuildJobsView), new UIPropertyMetadata(null, BuildJobsView.OnJobTextStyleChanged));

		private static void OnJobTextStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((BuildJobsView)d).OnJobTextStyleChanged((Style)e.NewValue);

		private static void AffectsVisual(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((BuildJobsView)d).InvalidateVisual();

		private readonly TextBlock _jobTextStyleBridge = new TextBlock();
		private Typeface _jobTextTypeface;

		private readonly Dictionary<BuildJobViewModel, FormattedText> _jobTextCache
			= new Dictionary<BuildJobViewModel, FormattedText>();

		private void InitializeRenderPart()
		{
			this.UpdateJobTextTypeface();
		}

		private void OnJobTextStyleChanged(Style value)
		{
			_jobTextStyleBridge.Style = value;
			this.UpdateJobTextTypeface();
		}

		private void UpdateJobTextTypeface()
		{
			_jobTextTypeface = new Typeface(
				_jobTextStyleBridge.FontFamily,
				_jobTextStyleBridge.FontStyle,
				_jobTextStyleBridge.FontWeight,
				_jobTextStyleBridge.FontStretch);
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			var scaling = _buildViewportService.Scaling;

			var minimumLeft = scaling * _startTimeOffset;
			var maxWidth = 24 * 60 * 60 * scaling;
			var performanceMode = _activeJobs.Count > 8;
			var showText = _jobDisplayMode == BuildJobDisplayMode.Standard;

			foreach (var job in _activeJobs)
			{
				var left = Math.Max(minimumLeft, job.StartTimeOffset * scaling);
				var acceptedStartTimeOffset = Math.Max(_startTimeOffset, job.StartTimeOffset);
				var width = MathEx.Clamp((job.EndTimeOffset - acceptedStartTimeOffset) * scaling, 0, maxWidth);

				if (width < BuildJobView.ShortJobWidthThreshold)
				{
					// try to use space before next job
					width = job.NextJob != null
						? MathEx.Clamp((job.NextJob.StartTimeOffset - acceptedStartTimeOffset) * scaling, 0, BuildJobView.ShortJobWidthThreshold)
						: BuildJobView.ShortJobWidthThreshold;
				}

				if (width < 1   // job too short to display
					|| left + width < 1)    // left could be negative
				{
					// we don't recycle this view because it might be shown again after a zoom
					continue;
				}

				var top = _coreTopMap[job.OwnerCore];

				this.DrawJob(dc, job, new Rect(left + _headerViewWidth, top, width, _jobViewHeight), showText, performanceMode);
			}
		}

		private void DrawJob(DrawingContext dc, BuildJobViewModel job, Rect rect, bool showText, bool performanceMode)
		{
			var paddedLeft = rect.X + this.JobMargin.Left;
			var paddedWidth = rect.Width - this.JobMargin.Left - this.JobMargin.Right;

			if (paddedWidth < 1)
			{
				// make space horizontally to ensure the border is at least 1px wide
				paddedLeft += (paddedWidth - 1) / 2;
				paddedWidth = 1;
			}

			var paddedRect = new Rect(
				paddedLeft,
				rect.Y + this.JobMargin.Top,
				paddedWidth,
				rect.Height - this.JobMargin.Top - this.JobMargin.Bottom);

			if (rect.Width <= 12)
			{
				dc.DrawRectangle(job.UIBackground, job.UIBorderPen, paddedRect);
				return;
			}

			if (performanceMode)
			{
				dc.DrawRectangle(job.UIBackground, job.UIBorderPen, paddedRect);
			}
			else
			{
				var cornerRadius = MathEx.Clamp((rect.Width - 12) / 2, 0, 2);

				dc.DrawRoundedRectangle(job.UIBackground, job.UIBorderPen, paddedRect, cornerRadius, cornerRadius);
			}

			if (rect.Width > 36 && showText)
			{
				var opactiy = Math.Min(1, Math.Max(0, (rect.Width - 36) / 48));
				var brush = job.UIForeground.Clone();
				brush.Opacity = opactiy;
				var formattedText = this.GetJobText(job, brush);
				formattedText.MaxTextWidth = paddedWidth - this.JobTextMargin.Left - this.JobTextMargin.Right;

				var position = new Point(
					paddedRect.Left + this.JobTextMargin.Left, 
					paddedRect.Top + (paddedRect.Height - formattedText.Height) / 2);

				dc.DrawText(formattedText, position);
			}
		}

		private FormattedText GetJobText(BuildJobViewModel job, Brush foreground)
		{
			if (!_jobTextCache.TryGetValue(job, out var formattedText))
			{
				formattedText = new FormattedText(job.DisplayName,
					CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					_jobTextTypeface,
					_jobTextStyleBridge.FontSize,
					foreground)
				{
					MaxLineCount = 1,
					Trimming = TextTrimming.CharacterEllipsis
				};
				_jobTextCache[job] = formattedText;
			}

			return formattedText;
		}
	}
}
