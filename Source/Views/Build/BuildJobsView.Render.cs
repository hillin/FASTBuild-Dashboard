using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using FastBuild.Dashboard.Services.Build;
using FastBuild.Dashboard.Support;
using FastBuild.Dashboard.ViewModels.Build;

namespace FastBuild.Dashboard.Views.Build
{
	partial class BuildJobsView
	{
		private const double ShortJobWidthThreshold = 12;
		private const double TextlessJobWidthThreshold = 36;


		public Thickness JobMargin
		{
			get => (Thickness)this.GetValue(JobMarginProperty);
			set => this.SetValue(JobMarginProperty, value);
		}

		public static readonly DependencyProperty JobMarginProperty =
			DependencyProperty.Register(
				"JobMargin",
				typeof(Thickness),
				typeof(BuildJobsView),
				new UIPropertyMetadata(new Thickness(2), BuildJobsView.AffectsVisual));

		public Thickness JobTextMargin
		{
			get => (Thickness)this.GetValue(JobTextMarginProperty);
			set => this.SetValue(JobTextMarginProperty, value);
		}

		public static readonly DependencyProperty JobTextMarginProperty =
			DependencyProperty.Register(
				"JobTextMargin",
				typeof(Thickness),
				typeof(BuildJobsView),
				new UIPropertyMetadata(new Thickness(8, 2, 2, 2), BuildJobsView.AffectsVisual));

		public Style JobTextStyle
		{
			get => (Style)this.GetValue(JobTextStyleProperty);
			set => this.SetValue(JobTextStyleProperty, value);
		}

		public static readonly DependencyProperty JobTextStyleProperty =
			DependencyProperty.Register(
				"JobTextStyle",
				typeof(Style),
				typeof(BuildJobsView),
				new UIPropertyMetadata(null, BuildJobsView.OnJobTextStyleChanged));

		private static void OnJobTextStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			=> ((BuildJobsView)d).OnJobTextStyleChanged((Style)e.NewValue);

		private static void AffectsVisual(DependencyObject d, DependencyPropertyChangedEventArgs e)
			=> ((BuildJobsView)d).InvalidateVisual();

		private readonly TextBlock _jobTextStyleBridge = new TextBlock();
		private GlyphTypeface _jobTextGlyphTypeface;


		private void InitializeRenderPart()
		{
			this.UpdateJobTextTypeface();
			this.Loaded += this.OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			var window = this.FindAncestor<Window>();
			if (window != null)
			{
				_jobTextStyleBridge.FontFamily = window.FontFamily;
				this.UpdateJobTextTypeface();
			}
		}

		private void OnJobTextStyleChanged(Style value)
		{
			_jobTextStyleBridge.Style = value;
			this.UpdateJobTextTypeface();
		}

		private void UpdateJobTextTypeface()
		{
			var jobTextTypeface = new Typeface(
				_jobTextStyleBridge.FontFamily,
				_jobTextStyleBridge.FontStyle,
				_jobTextStyleBridge.FontWeight,
				_jobTextStyleBridge.FontStretch);

			jobTextTypeface.TryGetGlyphTypeface(out _jobTextGlyphTypeface);
			this.InvalidateVisual();
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			dc.DrawRectangle(this.Background, null, LayoutInformation.GetLayoutSlot(this));

			var scaling = _buildViewportService.Scaling;

			var minimumLeft = scaling * _startTimeOffset;
			var maxWidth = scaling * (_endTimeOffset - _startTimeOffset) + 16;
			var showText = _jobDisplayMode == BuildJobDisplayMode.Standard;

			foreach (var job in _activeJobs)
			{
				var left = Math.Max(minimumLeft, job.StartTimeOffset * scaling);
				var acceptedStartTimeOffset = Math.Max(_startTimeOffset, job.StartTimeOffset);
				var width = MathEx.Clamp((job.EndTimeOffset - acceptedStartTimeOffset) * scaling, 0, maxWidth);

				if (width < ShortJobWidthThreshold)
				{
					// try to use space before next job
					width = job.NextJob != null
						? MathEx.Clamp((job.NextJob.StartTimeOffset - acceptedStartTimeOffset) * scaling, 0, ShortJobWidthThreshold)
						: ShortJobWidthThreshold;
				}

				if (width < 1   // job too short to display
					|| left + width < 1)    // left could be negative
				{
					continue;
				}

				var top = _coreTopMap[job.OwnerCore];

				var bounds = new Rect(left + _headerViewWidth, top, width, _jobViewHeight);
				_jobBounds[job] = bounds;
				this.DrawJob(dc, job, bounds, showText);
			}
		}

		private void DrawJob(DrawingContext dc, BuildJobViewModel job, Rect rect, bool showText)
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

			if (rect.Width <= ShortJobWidthThreshold)
			{
				// a very short job, simply draw a rectangle 
				dc.DrawRectangle(job.UIBackground, job.UIBorderPen, paddedRect);
				return;
			}

			var cornerRadius = MathEx.Clamp((rect.Width - ShortJobWidthThreshold) / 2, 0, 2);

			dc.DrawRoundedRectangle(job.UIBackground, job.UIBorderPen, paddedRect, cornerRadius, cornerRadius);

			if (!(rect.Width > TextlessJobWidthThreshold) || !showText)
			{
				return;
			}

			var opactiy = (rect.Width - TextlessJobWidthThreshold) / TextlessJobWidthThreshold;

			if (opactiy <= 0)
			{
				return;
			}

			Brush brush;
			if (opactiy >= 1)
			{
				// don't clone (to save performance)
				brush = job.UIForeground;
			}
			else
			{
				brush = job.UIForeground.Clone();
				brush.Opacity = brush.Opacity * opactiy;
			}

			var textWidth = paddedWidth - this.JobTextMargin.Left - this.JobTextMargin.Right;

			var position = new Point(
				paddedRect.Left + this.JobTextMargin.Left,
				paddedRect.Top + (paddedRect.Height - _jobTextGlyphTypeface.Height * _jobTextStyleBridge.FontSize) / 2);

			dc.DrawGlyphRun(brush, this.CreateGlyphRun(job.DisplayName, position, textWidth, true));

			// draw elapsed time for long jobs
			if (job.Status == Communication.BuildJobStatus.Building)
			{
				var elapsedTimeTextOpacity = job.ElapsedSeconds - 4;

				if (elapsedTimeTextOpacity > 0)
				{
					if (elapsedTimeTextOpacity >= 1)
					{
						// don't clone (to save performance)
						brush = job.UIForeground;
					}
					else
					{
						brush = job.UIForeground.Clone();
						brush.Opacity = brush.Opacity * elapsedTimeTextOpacity;
					}

					position.X += paddedRect.Width;
					var elapsedTimeText = $"+{job.ElapsedSeconds:0.0}";
					dc.DrawGlyphRun(brush, this.CreateGlyphRun(elapsedTimeText, position, double.PositiveInfinity, false));
				}
			}
		}

		private GlyphRun CreateGlyphRun(string text, Point origin, double width, bool useEllipsis)
		{
			var length = text.Length;

			// the final text could be at most length + 2 long (last char omitted and ellipsis appended)
			var glyphIndices = new List<ushort>(length + 2);
			var advanceWidths = new List<double>(length + 2);

			var size = _jobTextStyleBridge.FontSize;

			var baselineOrigin = new Point(origin.X, origin.Y + _jobTextGlyphTypeface.Baseline * size);

			var availableWidth = width;
			var index = 0;
			for (; index < length; ++index)
			{
				var c = text[index];
				var glyphIndex = _jobTextGlyphTypeface.CharacterToGlyphMap[c];
				var advanceWidth = _jobTextGlyphTypeface.AdvanceWidths[glyphIndex] * size;
				var remainingWidth = availableWidth - advanceWidth;
				if (remainingWidth < 0)
				{
					break;
				}

				glyphIndices.Add(glyphIndex);
				advanceWidths.Add(advanceWidth);
				availableWidth = remainingWidth;
			}

			if (index < length && useEllipsis)
			{
				var dotIndex = _jobTextGlyphTypeface.CharacterToGlyphMap['.'];
				var dotAdvanceWidth = _jobTextGlyphTypeface.AdvanceWidths[dotIndex] * _jobTextStyleBridge.FontSize;
				var ellipsisWidth = dotAdvanceWidth * 3;

				for (--index; index >= 0; --index)
				{
					availableWidth += advanceWidths[index];
					advanceWidths.RemoveAt(index);
					glyphIndices.RemoveAt(index);
					if (availableWidth >= ellipsisWidth)
					{
						break;
					}
				}

				for (var i = 0; i < 3; ++i)
				{
					advanceWidths.Add(dotAdvanceWidth);
					glyphIndices.Add(dotIndex);
				}
			}

			return new GlyphRun(
				_jobTextGlyphTypeface,
				0,
				false,
				size,
				glyphIndices,
				baselineOrigin,
				advanceWidths,
				null, null, null, null, null, null);
		}

	}
}
