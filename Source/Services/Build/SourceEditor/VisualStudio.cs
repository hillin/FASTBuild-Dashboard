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
	internal partial class VisualStudio : IExternalSourceEditor
	{
		
		public bool IsAvailable => true;

		public bool OpenFile(string file, int lineNumber, int initiatorProcessId)
		{
			var retVal = WinAPI.GetRunningObjectTable(0, out IRunningObjectTable rot);

			if (retVal == 0)
			{
				rot.EnumRunning(out IEnumMoniker enumMoniker);

				var fetched = IntPtr.Zero;
				var monikers = new IMoniker[1];
				while (enumMoniker.Next(1, monikers, fetched) == 0)
				{
					var moniker = monikers[0];
					WinAPI.CreateBindCtx(0, out IBindCtx bindCtx);
					moniker.GetDisplayName(bindCtx, null, out string displayName);
					moniker.GetClassID(out var _);
					if (displayName.StartsWith("!VisualStudio.DTE"))
					{
						if (initiatorProcessId > 0)
						{
							// DTE's process id is appended in its moniker name after a colon
							var colonIndex = displayName.LastIndexOf(':');
							if (colonIndex < 0)
							{
								continue;
							}

							if (displayName.Substring(colonIndex + 1) != initiatorProcessId.ToString())
							{
								continue;
							}
						}

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
								var selection = window.Document.Selection;
								selection.GotoLine(lineNumber, true);
							}

							// workaround: when the DTE window is brought to front, the Deactivated
							// event won't be fired for Application, thus the tooltip won't hide
							App.Current.RaiseOnDeactivated();

							window.Activate();
							window.Document.Activate();

							// we should use window's hWnd, however seems it is always zero
							var dteHwnd = (IntPtr)dte.MainWindow.HWnd;
							WinAPI.FlashWindow(dteHwnd, false);
							WinAPI.SetForegroundWindow(dteHwnd);

							return true;
						}
						catch (COMException)
						{
							// DTE might not be ready at this time

							if (initiatorProcessId > 0)
							{
								// skip other DTE instances if distinct id is specified
								return false;
							}
						}
					}
				}
			}

			return false;
		}
	}
}
