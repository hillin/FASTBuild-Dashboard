using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuild.Dashboard.ViewModels.Build
{
	internal partial class BuildInitiatorProcessViewModel
	{
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
				var id = GetInitiatorProcessId(fbuildProcessId.Value);
				if (id < 0)
				{
					this.Name = "Unknown process (exited)";
					this.InitiatorProcessId = -1;
				}
				else
				{
					try
					{
						var process = Process.GetProcessById(id);
						this.Name = process.ProcessName;
						this.InitiatorProcessId = id;
					}
					catch (ArgumentException)
					{
						this.Name = "Unknown process (exited)";
						this.InitiatorProcessId = -1;
					}
				}
			}

		}


		private int GetInitiatorProcessId(int id)
		{
			Process currentProcess;
			try
			{
				currentProcess = Process.GetProcessById(id);
			}
			catch (ArgumentException)
			{
				// the fbuild process is already terminated
				return -1;
			}

			var startTime = currentProcess.StartTime;

			var parentId = WinAPIUtils.GetParentProcessId(id);

			try
			{
				var process = Process.GetProcessById(parentId);
				if (process.StartTime > startTime)
				{
					return -1;
				}

				return GetRootProcessId(id, startTime);
			}
			catch (ArgumentException)
			{
				// parent not found - try to find a wrapped initator
				return this.GetWrappedInitiatorProcessId(currentProcess);
			}
		}

		private int GetWrappedInitiatorProcessId(Process process)
		{
			// FASTBuild uses a process wrapping mechanism to spawn a chain of (3) processes
			// in order to finally get a "standalone" process which is not a child/descendant 
			// process of the build initiator by terminating the "intermediate" process so 
			// the derivation chain is cut.

			// we use a tricky approach here to find the initiator, but it's best to output
			// the initiator process ID directly from FASTBuild

			// first, find other fbuild.exe processes
			var processes = Process.GetProcessesByName(process.ProcessName).Where(p => p.Id != process.Id);

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
				return -1;
			}

			return this.GetRootProcessId(candidateSourceProcess.Id, candidateSourceProcess.StartTime);
		}

		private int GetRootProcessId(int id, DateTime startTime)
		{
			while (true)
			{
				var parentId = WinAPIUtils.GetParentProcessId(id);
				if (parentId <= 0)
				{
					// root reached
					return id;
				}

				Process process;

				try
				{
					process = Process.GetProcessById(parentId);
				}
				catch (ArgumentException)
				{
					return id;
				}

				if (process.StartTime > startTime)
				{
					return -1;
				}

				if (process.ProcessName == "explorer")
				{
					// most programs are started from explorer
					return id;
				}

				id = parentId;
				startTime = process.StartTime;
			}
		}
	}
}
