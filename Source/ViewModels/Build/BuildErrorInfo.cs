using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
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
		public ICommand OpenFileCommand { get; }


		public BuildErrorInfo(string filePath, int lineNumber, string errorMessage)
		{
			this.FilePath = filePath;
			this.LineNumber = lineNumber;
			this.ErrorMessage = errorMessage;
			this.OpenFileCommand = new SimpleCommand(this.ExecuteOpenFile, this.CanExecuteOpenFile);
		}

		private bool CanExecuteOpenFile(object obj)
			=> File.Exists(this.FilePath);

		private void ExecuteOpenFile(object obj)
		{
			if (!IoC.Get<IExternalSourceEditorService>().OpenFile(this.FilePath, this.LineNumber))
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