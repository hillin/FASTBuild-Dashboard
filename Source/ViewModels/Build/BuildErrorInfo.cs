using System.IO;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using FastBuild.Dashboard.Services.Build.SourceEditor;
using FastBuild.Dashboard.Support;

namespace FastBuild.Dashboard.ViewModels.Build
{
	internal class BuildErrorInfo
	{

		public string FilePath { get; }
		public int LineNumber { get; }
		public string ErrorMessage { get; }
		public BuildInitiatorProcessViewModel InitiatorProcess { get; }
		public ICommand OpenFileCommand { get; }


		public BuildErrorInfo(string filePath, int lineNumber, string errorMessage, BuildInitiatorProcessViewModel initiatorProcess)
		{
			this.FilePath = filePath;
			this.LineNumber = lineNumber;
			this.ErrorMessage = errorMessage;
			this.InitiatorProcess = initiatorProcess;
			this.OpenFileCommand = new SimpleCommand(this.ExecuteOpenFile, this.CanExecuteOpenFile);
		}

		private bool CanExecuteOpenFile(object obj)
			=> File.Exists(this.FilePath);

		private void ExecuteOpenFile(object obj)
		{
			if (!IoC.Get<IExternalSourceEditorService>().OpenFile(this.FilePath, this.LineNumber, this.InitiatorProcess.InitiatorProcessId))
			{
				MessageBox.Show(
					"Failed to open source file. Please go to the Settings page and check if the selected source editor is correctly configured.",
					"Open Source File",
					MessageBoxButton.OK,
					MessageBoxImage.Exclamation);
			}
		}
		
	}
}