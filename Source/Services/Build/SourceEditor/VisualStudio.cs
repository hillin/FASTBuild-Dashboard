using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

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
		private static readonly string[] DteProgIds =
		{
			"VisualStudio.DTE.15.0",	// vs 2017
			"VisualStudio.DTE.14.0",	// vs 2015
			"VisualStudio.DTE.12.0",	// vs 2013
			"VisualStudio.DTE.11.0"		// vs 2012
		};

		private struct DteInstanceInfo
		{
			public dynamic DteInstance { get; set; }
			public string DisplayName { get; set; }
		}

		private static List<DteInstanceInfo> RetrieveDteInstances()
		{
			var candidates = new List<DteInstanceInfo>();

			var retVal = WinAPI.GetRunningObjectTable(0, out IRunningObjectTable rot);

			if (retVal != 0)
			{
				return candidates;
			}

			rot.EnumRunning(out IEnumMoniker enumMoniker);

			var monikers = new IMoniker[1];
			while (enumMoniker.Next(1, monikers, IntPtr.Zero) == 0)
			{
				WinAPI.CreateBindCtx(0, out var bindCtx);
				monikers[0].GetDisplayName(bindCtx, null, out var displayName);

				if (!DteProgIds.Any(progId => displayName.StartsWith($"!{progId}:")))
				{
					continue;
				}

				try
				{
					rot.GetObject(monikers[0], out dynamic dte);
					var instanceInfo = new DteInstanceInfo
					{
						DisplayName = displayName,
						DteInstance = dte
					};
					candidates.Add(instanceInfo);
				}
				catch (COMException)
				{
					// DTE might not be ready at this time
				}
			}

			return candidates;
		}

		private static bool TryOpenWithDte(
			ICollection<DteInstanceInfo> dteInstances,
			Predicate<DteInstanceInfo> condition,
			string file,
			int lineNumber,
			bool distinct = false)
		{
			foreach (var instanceInfo in dteInstances)
			{
				if (condition != null && !condition(instanceInfo))
				{
					continue;
				}

				if (VisualStudio.TryOpenWithDte(instanceInfo.DteInstance, file, lineNumber))
				{
					return true;
				}

				// this dte is proven to be invalid, remove it
				dteInstances.Remove(instanceInfo);

				if (distinct)
				{
					return false;
				}
			}

			return false;
		}


		private static bool TryOpenWithDte(dynamic dte, string file, int lineNumber)
		{
			try
			{
				var window = dte.ItemOperations.OpenFile(file);
				if (window == null)
				{
					return false;
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

				return false;
			}
		}

		private static bool MatchesInitiatorProcessId(DteInstanceInfo instanceInfo, int initiatorProcessId)
		{
			var colonIndex = instanceInfo.DisplayName.LastIndexOf(':');
			return colonIndex >= 0
				   && instanceInfo.DisplayName.Substring(colonIndex + 1) == initiatorProcessId.ToString();
		}

		private static void SortByPseudoSolutionMatching(List<DteInstanceInfo> dteInstances, string file)
		{
			// try to find a VS instance whose solution file matches best with the file to 
			// open, location-wise (i.e. D:\project\source\test.sln matches  D:\project\source\test\test.cpp 
			// better than E:\someplace\1.sln or D:\project\project.sln). 
			// This could be decently fast, and will work well in most cases, unless you have some bizarrely 
			// structured projects.

			int EvaluateScore(DteInstanceInfo instanceInfo)
			{
				try
				{
					var solutionFilename = (string)instanceInfo.DteInstance.Solution.FileName;
					var minLength = Math.Min(solutionFilename.Length, file.Length);
					var i = 0;
					for (; i < minLength; ++i)
					{
						if (solutionFilename[i] != file[i])
						{
							break;
						}
					}

					return i;
				}
				catch (COMException)
				{
					return 0;
				}
			}

			var scores = dteInstances.ToDictionary(d => d, EvaluateScore);

			dteInstances.Sort((d1, d2) => scores[d2] - scores[d1]);
		}

		public bool IsAvailable => true;

		public bool OpenFile(string file, int lineNumber, int initiatorProcessId)
		{
			var dteInstances = VisualStudio.RetrieveDteInstances();

			// prior use if a DTE instance with matching process ID found
			if (initiatorProcessId > 0)
			{
				if (VisualStudio.TryOpenWithDte(
					dteInstances,
					d => VisualStudio.MatchesInitiatorProcessId(d, initiatorProcessId),
					file,
					lineNumber,
					true))
				{
					return true;
				}
			}

			// do a pseudo solution detection
			VisualStudio.SortByPseudoSolutionMatching(dteInstances, file);
			if (VisualStudio.TryOpenWithDte(dteInstances, null, file, lineNumber))
			{
				return true;
			}

			// finally, try to start a new VS instance
			foreach (var progId in DteProgIds)
			{
				var type = Type.GetTypeFromProgID(progId);
				if (type == null)
				{
					continue;
				}

				try
				{
					dynamic dte = Activator.CreateInstance(type);

					if (VisualStudio.TryOpenWithDte(dte, file, lineNumber))
					{
						return true;
					}
				}
				catch (COMException)
				{
					// skip
				}
			}

			return false;
		}

	}
}

