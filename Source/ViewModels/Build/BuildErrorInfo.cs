using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FastBuild.Dashboard.Support;

namespace FastBuild.Dashboard.ViewModels.Build
{
	internal class BuildErrorInfo
	{
		[DllImport("ole32.dll")]
		private static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);

		[DllImport("ole32.dll")]
		private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr SetActiveWindow(IntPtr hWnd);

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
			if (!this.TryOpenWithVisualStudio())
			{
				Process.Start(this.FilePath);
			}
		}

		private bool TryOpenWithVisualStudio()
		{
			var retVal = BuildErrorInfo.GetRunningObjectTable(0, out IRunningObjectTable rot);

			if (retVal == 0)
			{
				rot.EnumRunning(out IEnumMoniker enumMoniker);

				var fetched = IntPtr.Zero;
				var monikers = new IMoniker[1];
				while (enumMoniker.Next(1, monikers, fetched) == 0)
				{
					var moniker = monikers[0];
					BuildErrorInfo.CreateBindCtx(0, out IBindCtx bindCtx);
					moniker.GetDisplayName(bindCtx, null, out string displayName);
					moniker.GetClassID(out var _);
					if (displayName.StartsWith("!VisualStudio.DTE"))
					{
						try
						{
							rot.GetObject(monikers[0], out dynamic dte);
							var window = dte.ItemOperations.OpenFile(this.FilePath);
							if (window == null)
							{
								continue;
							}

							// workaround: when the DTE window is brought to front, the Deactivated
							// event won't be fired for Application, thus the tooltip won't hide
							App.Current.RaiseOnDeactivated();

							window.Activate();
							window.Document.Activate();

							var selection = window.Document.Selection;
							selection.GotoLine(this.LineNumber, true);

							return true;
						}
						catch (COMException)
						{
							// the debugger might not be ready at this time
						}
					}
				}
			}

			return false;
		}
	}
}