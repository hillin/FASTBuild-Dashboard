using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using FastBuild.Dashboard.ViewModels.Build;

namespace FastBuild.Dashboard.Services.Build.SourceEditor
{
	[ExternalSourceEditor(
		"visual-studio", 
		"Visual Studio", 
		"Open source file in a running Visual Studio instance. Will fallback to system default editor if Visual Studio is not running.",
		AllowOverridePath = false, 
		AllowSpecifyAdditionalArgs = false, 
		AllowSpecifyArgs = false)]
	internal class VisualStudio : IExternalSourceEditor
	{
		[DllImport("ole32.dll")]
		private static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);

		[DllImport("ole32.dll")]
		private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr SetActiveWindow(IntPtr hWnd);


		public bool IsAvailable => true;

		public bool OpenFile(string file, int lineNumber)
		{
			var retVal = VisualStudio.GetRunningObjectTable(0, out IRunningObjectTable rot);

			if (retVal == 0)
			{
				rot.EnumRunning(out IEnumMoniker enumMoniker);

				var fetched = IntPtr.Zero;
				var monikers = new IMoniker[1];
				while (enumMoniker.Next(1, monikers, fetched) == 0)
				{
					var moniker = monikers[0];
					VisualStudio.CreateBindCtx(0, out IBindCtx bindCtx);
					moniker.GetDisplayName(bindCtx, null, out string displayName);
					moniker.GetClassID(out var _);
					if (displayName.StartsWith("!VisualStudio.DTE"))
					{
						try
						{
							rot.GetObject(monikers[0], out dynamic dte);
							var window = dte.ItemOperations.OpenFile(file);
							if (window == null)
							{
								continue;
							}

							if (lineNumber > 0)
							{
								// workaround: when the DTE window is brought to front, the Deactivated
								// event won't be fired for Application, thus the tooltip won't hide
								App.Current.RaiseOnDeactivated();

								window.Activate();
								window.Document.Activate();

								var selection = window.Document.Selection;
								selection.GotoLine(lineNumber, true);
							}

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
