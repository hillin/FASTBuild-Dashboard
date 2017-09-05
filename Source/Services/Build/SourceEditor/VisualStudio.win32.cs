using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace FastBuild.Dashboard.Services.Build.SourceEditor
{
	internal partial class VisualStudio
	{
		private static class WinAPI
		{
			[DllImport("ole32.dll")]
			public static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);

			[DllImport("ole32.dll")]
			public static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

			[DllImport("user32.dll")]
			public static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

			[DllImport("user32.dll")]
			public static extern bool SetForegroundWindow(IntPtr hWnd);

		}
	}
}
