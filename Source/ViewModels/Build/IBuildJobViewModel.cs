using System.Windows.Media;

namespace FastBuild.Dashboard.ViewModels.Build
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
	}
}
