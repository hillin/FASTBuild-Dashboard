using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace FastBuild.Dashboard.ViewModels.Build
{
	internal partial class BuildInitiatorProcessViewModel
	{
		private enum InitiatorProcessQueryResult
		{
			Success,
			Exited,
			AccessDenied
		}

		internal static bool GetIsProcessAccessible(int processId)
		{
			return WinAPI.OpenProcess(WinAPI.ProcessAccessFlags.QueryInformation, false, processId) != IntPtr.Zero;
		}


		public string Name { get; }
		public int InitiatorProcessId { get; }

		public BuildInitiatorProcessViewModel(int? fbuildProcessId)
		{
			if (fbuildProcessId == null)
			{
				this.Name = "Unknown process";
				this.InitiatorProcessId = -1;
			}
			else
			{
				var result = GetInitiatorProcessId(fbuildProcessId.Value, out var id);
				this.InitiatorProcessId = id;

				switch (result)
				{
					case InitiatorProcessQueryResult.Success:
						try
						{
							var process = Process.GetProcessById(id);
							this.Name = process.ProcessName;
						}
						catch (ArgumentException)
						{
							this.Name = "Unknown process (exited)";
							this.InitiatorProcessId = -1;
						}
						break;
					case InitiatorProcessQueryResult.Exited:
						this.Name = "Unknown process (exited)";
						break;
					case InitiatorProcessQueryResult.AccessDenied:
						this.Name = "Unknown process (access denied)";
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				
			}

		}

		private InitiatorProcessQueryResult GetInitiatorProcessId(int id, out int initiatorId)
		{
			initiatorId = -1;

			Process currentProcess;
			try
			{
				currentProcess = Process.GetProcessById(id);
			}
			catch (ArgumentException)
			{
				// the fbuild process is already terminated
				return InitiatorProcessQueryResult.Exited;
			}

			if (!BuildInitiatorProcessViewModel.GetIsProcessAccessible(id))
			{
				return InitiatorProcessQueryResult.AccessDenied;
			}

			var startTime = currentProcess.StartTime;

			var parentId = WinAPIUtils.GetParentProcessId(id);

			if (!BuildInitiatorProcessViewModel.GetIsProcessAccessible(parentId))
			{
				return InitiatorProcessQueryResult.AccessDenied;
			}

			try
			{
				var process = Process.GetProcessById(parentId);

				if (process.StartTime > startTime)
				{
					// parent is already exited, this 'process' is just a fake one which
					// reused parent's process id - try to find a wrapped initator
					return this.GetWrappedInitiatorProcessId(currentProcess, out initiatorId);
				}

				return GetRootProcessId(id, startTime, out initiatorId);
			}
			catch (ArgumentException)
			{
				// parent not found - try to find a wrapped initator
				return this.GetWrappedInitiatorProcessId(currentProcess, out initiatorId);
			}
		}

		private InitiatorProcessQueryResult GetWrappedInitiatorProcessId(Process process, out int initiatorId)
		{
			// FASTBuild uses a process wrapping mechanism to spawn a chain of (3) processes
			// in order to finally get a "standalone" process which is not a child/descendant 
			// process of the build initiator by terminating the "intermediate" process so 
			// the derivation chain is cut.

			// we use a tricky approach here to find the initiator, but it's best to output
			// the initiator process ID directly from FASTBuild

			// first, find other fbuild.exe processes
			var processes = Process.GetProcessesByName(process.ProcessName)
				.Where(p => BuildInitiatorProcessViewModel.GetIsProcessAccessible(p.Id) && p.Id != process.Id);

			// and pick out the one which is started right before our fbuild process
			// this is hacky and maybe not reliable in a small chance 
			var minDeltaTime = double.MaxValue;
			Process candidateSourceProcess = null;
			foreach (var p in processes)
			{
				var deltaTime = (process.StartTime - p.StartTime).TotalSeconds;
				if (deltaTime < 0)
				{
					continue;
				}

				if (deltaTime < minDeltaTime)
				{
					candidateSourceProcess = p;
					minDeltaTime = deltaTime;
				}
			}

			if (candidateSourceProcess == null)
			{
				initiatorId = -1;
				return InitiatorProcessQueryResult.Exited;
			}

			return this.GetRootProcessId(candidateSourceProcess.Id, candidateSourceProcess.StartTime, out initiatorId);
		}

		private InitiatorProcessQueryResult GetRootProcessId(int id, DateTime startTime, out int rootId)
		{
			while (true)
			{
				var parentId = WinAPIUtils.GetParentProcessId(id);
				if (parentId <= 0)
				{
					// root reached
					rootId = id;
					return InitiatorProcessQueryResult.Success;
				}

				if (!BuildInitiatorProcessViewModel.GetIsProcessAccessible(parentId))
				{
					// parent not accessible (access deined etc.), which might be running under a system account (e.g. svchost)
					// assume root reached
					rootId = id;
					return InitiatorProcessQueryResult.Success;
				}

				Process parentProcess;

				try
				{
					parentProcess = Process.GetProcessById(parentId);
				}
				catch (ArgumentException)
				{
					// parent exited - root reached
					rootId = id;
					return InitiatorProcessQueryResult.Success;
				}

				if (parentProcess.StartTime > startTime)
				{
					// parent exited (and its id is reused by a newer process) - root reached
					rootId = id;
					return InitiatorProcessQueryResult.Success;
				}

				if (parentProcess.ProcessName == "devenv")
				{
					// special care for Visual Studio: stop traverse up even if there are more
					// ancestors (e.g. Visual Studio started by a command line)
					// todo: this is not really reliable, we need more evidence to prove this
					// is a Visual Studio process
					rootId = parentId;
					return InitiatorProcessQueryResult.Success;
				}

				if (parentProcess.ProcessName == "explorer")
				{
					// most programs are started from explorer
					// todo: this is not really reliable, we need more evidence to prove this
					// is an explorer process
					rootId = id;
					return InitiatorProcessQueryResult.Success;
				}

				id = parentId;
				startTime = parentProcess.StartTime;
			}
		}
	}
}
