namespace FastBuild.Dashboard.Services.Build.SourceEditor
{
	internal interface IExternalSourceEditor
	{
		bool IsAvailable { get; }
		bool OpenFile(string file, int lineNumber, int initiatorProcessId);
	}
}
