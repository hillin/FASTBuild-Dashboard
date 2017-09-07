using System;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
// ReSharper disable All

namespace FastBuild.Dashboard.ViewModels.Build
{
	internal partial class BuildInitiatorProcessViewModel
	{
		private static class WinAPI
		{
			public const int ERROR_NO_MORE_FILES = 0x12;
			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern SafeSnapshotHandle CreateToolhelp32Snapshot(SnapshotFlags flags, uint id);
			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern bool Process32First(SafeSnapshotHandle hSnapshot, ref PROCESSENTRY32 lppe);
			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern bool Process32Next(SafeSnapshotHandle hSnapshot, ref PROCESSENTRY32 lppe);

			[Flags]
			public enum SnapshotFlags : uint
			{
				HeapList = 0x00000001,
				Process = 0x00000002,
				Thread = 0x00000004,
				Module = 0x00000008,
				Module32 = 0x00000010,
				All = (HeapList | Process | Thread | Module),
				Inherit = 0x80000000,
				NoHeaps = 0x40000000
			}
			[StructLayout(LayoutKind.Sequential)]
			public struct PROCESSENTRY32
			{
				public uint dwSize;
				public uint cntUsage;
				public uint th32ProcessID;
				public IntPtr th32DefaultHeapID;
				public uint th32ModuleID;
				public uint cntThreads;
				public uint th32ParentProcessID;
				public int pcPriClassBase;
				public uint dwFlags;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szExeFile;
			};
			[SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
			public sealed class SafeSnapshotHandle : SafeHandleMinusOneIsInvalid
			{
				internal SafeSnapshotHandle() : base(true)
				{
				}

				[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
				internal SafeSnapshotHandle(IntPtr handle) : base(true)
					=> this.SetHandle(handle);

				protected override bool ReleaseHandle()
					=> SafeSnapshotHandle.CloseHandle(handle);

				[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
				[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
				private static extern bool CloseHandle(IntPtr handle);
			}

			[Flags]
			public enum ProcessAccessFlags : uint
			{
				QueryInformation = 0x00000400,
			}

			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern IntPtr OpenProcess(
				ProcessAccessFlags processAccess,
				bool bInheritHandle,
				int processId
			);
		}

		private static class WinAPIUtils
		{
			public static int GetParentProcessId(int id)
			{
				var pe32 = new WinAPI.PROCESSENTRY32
				{
					dwSize = (uint)Marshal.SizeOf(typeof(WinAPI.PROCESSENTRY32))
				};

				using (var hSnapshot = WinAPI.CreateToolhelp32Snapshot(WinAPI.SnapshotFlags.Process, (uint)id))
				{
					if (hSnapshot.IsInvalid)
					{
						throw new Win32Exception();
					}

					if (!WinAPI.Process32First(hSnapshot, ref pe32))
					{
						var errno = Marshal.GetLastWin32Error();
						if (errno == WinAPI.ERROR_NO_MORE_FILES)
						{
							return -1;
						}

						throw new Win32Exception(errno);
					}
					do
					{
						if (pe32.th32ProcessID == (uint)id)
						{
							return (int)pe32.th32ParentProcessID;
						}

					} while (WinAPI.Process32Next(hSnapshot, ref pe32));
				}

				return -1;
			}
		}
	}
}
