using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FastBuilder.Support
{
	internal enum EllipsisPosition
	{
		Start,
		Middle,
		End
	}

	internal static class TextTrimmer
	{
		private const string Ellipsis = "...";

		private static readonly Size InifinitySize = new Size(double.PositiveInfinity, double.PositiveInfinity);

		private static double MeasureString(TextBlock textBlock, string text)
		{
			textBlock.Text = text;
			textBlock.Measure(InifinitySize);
			return textBlock.DesiredSize.Width;
		}

		/// <summary>
		/// Take a <paramref name="textBlock"/> and trim its text down to a specified <paramref name="width"/>, with ellipsis added into the specified <paramref name="ellipsisPosition"/>
		/// </summary>
		public static void SetTrimmedText(this TextBlock textBlock, string text, double width = double.NaN, EllipsisPosition ellipsisPosition = EllipsisPosition.End)
		{
			if (double.IsNaN(width))
			{
				width = textBlock.ActualWidth;
			}

			if (width <= 0)
			{
				return;
			}

			// this actually sets textBlock's text back to its original value
			var desiredSize = TextTrimmer.MeasureString(textBlock, text);

			if (desiredSize <= width)
			{
				textBlock.Text = text;
				return;
			}

			var ellipsisSize = TextTrimmer.MeasureString(textBlock, Ellipsis);
			width -= ellipsisSize;
			var epsilon = ellipsisSize / 3;

			if (width < epsilon)
			{
				textBlock.Text = text;
				return;
			}

			var segments = new List<string>();

			var builder = new StringBuilder();

			switch (ellipsisPosition)
			{
				case EllipsisPosition.End:
					TextTrimmer.TrimText(textBlock, text, width, segments, epsilon, false);
					foreach (var segment in segments)
					{
						builder.Append(segment);
					}

					builder.Append(Ellipsis);
					break;

				case EllipsisPosition.Start:
					TextTrimmer.TrimText(textBlock, text, width, segments, epsilon, true);
					builder.Append(Ellipsis);
					foreach (var segment in ((IEnumerable<string>)segments).Reverse())
					{
						builder.Append(segment);
					}

					break;

				case EllipsisPosition.Middle:
					var textLength = text.Length / 2;
					var firstHalf = text.Substring(0, textLength);
					var secondHalf = text.Substring(textLength);

					width /= 2;

					TextTrimmer.TrimText(textBlock, firstHalf, width, segments, epsilon, false);
					foreach (var segment in segments)
					{
						builder.Append(segment);
					}

					builder.Append(Ellipsis);

					segments.Clear();

					TextTrimmer.TrimText(textBlock, secondHalf, width, segments, epsilon, true);
					foreach (var segment in ((IEnumerable<string>)segments).Reverse())
					{
						builder.Append(segment);
					}

					break;
				default:
					throw new NotSupportedException();
			}

			textBlock.Text = builder.ToString();
		}


		private static void TrimText(TextBlock textBlock,
			string text,
			double size,
			ICollection<string> segments,
			double epsilon,
			bool reversed)
		{
			while (true)
			{
				if (text.Length == 1)
				{
					var textSize = TextTrimmer.MeasureString(textBlock, text);
					if (textSize <= size)
					{
						segments.Add(text);
					}

					return;
				}

				var halfLength = Math.Max(1, text.Length / 2);
				var firstHalf = reversed ? text.Substring(halfLength) : text.Substring(0, halfLength);
				var remainingSize = size - TextTrimmer.MeasureString(textBlock, firstHalf);
				if (remainingSize < 0)
				{
					// only one character and it's still too large for the room, skip it
					if (firstHalf.Length == 1)
					{
						return;
					}

					text = firstHalf;
					continue;
				}

				segments.Add(firstHalf);

				if (remainingSize > epsilon)
				{
					var secondHalf = reversed ? text.Substring(0, halfLength) : text.Substring(halfLength);
					text = secondHalf;
					size = remainingSize;
					continue;
				}

				break;
			}
		}
	}
}