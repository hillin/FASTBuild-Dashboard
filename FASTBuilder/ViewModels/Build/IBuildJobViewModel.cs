using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FastBuilder.ViewModels.Build
{
	internal interface IBuildJobViewModel
	{
		BuildCoreViewModel OwnerCore { get; }
		double StartTimeOffset { get; }
		double EndTimeOffset { get; }
		Brush UIForeground { get; }
		Brush UIBackground { get; }
		Brush UIBorderBrush { get; }
		string DisplayName { get; }
		string ToolTipText { get; }
	}
}
