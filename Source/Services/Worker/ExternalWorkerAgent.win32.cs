using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace FastBuild.Dashboard.Services.Worker
{
	internal partial class ExternalWorkerAgent
	{
#pragma warning disable 414
#pragma warning disable 169

		[SuppressMessage("ReSharper", "InconsistentNaming")]
		[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private static class WinAPI
		{
			public enum ComboBoxMessages
			{
				CB_SETCURSEL = 0x014E
			}

			public enum ComboBoxNotifications
			{
				CBN_SELCHANGE = 0x0001
			}

			public enum WindowsMessages
			{
				WM_COMMAND = 0x0111
			}

			public enum ShowWindowCommands
			{
				SW_HIDE = 0x0000
			}

			public enum ListViewMessages
			{
				LVM_GETITEMCOUNT = 0x1004,
				LVM_GETITEMTEXT = 0x104B
			}

			public enum ListViewItemFilters : uint
			{
				LVIF_TEXT = 0x0001,
			}

			public const uint MAX_LVMSTRING = 255;

			[StructLayout(LayoutKind.Sequential)]
			public struct LVITEM
			{
				public uint mask;
				public int iItem;
				public int iSubItem;
				public uint state;
				public uint stateMask;
				public IntPtr pszText;
				public int cchTextMax;
				public int iImage;
				public IntPtr lParam;
			}

			[DllImport("user32.dll")]
			public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

			[return: MarshalAs(UnmanagedType.Bool)]
			[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
			public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

			public enum GetWindowLongIndexes
			{
				GWL_HWNDPARENT = -8,
				GWL_ID = -12
			}

			[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
			public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

			[DllImport("user32.dll")]
			public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

			public delegate bool EnumWindowsFilter(IntPtr hWnd, IntPtr lParam);

			[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool EnumWindows(EnumWindowsFilter lpEnumFunc, IntPtr lParam);

			[DllImport("user32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsFilter lpEnumFunc, IntPtr lParam);

			[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static extern IntPtr GetParent(IntPtr hWnd);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

			[DllImport("user32.dll", EntryPoint = "GetWindowText", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
			public static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpWindowText, int nMaxCount);

			[DllImport("user32.dll", EntryPoint = "RealGetWindowClass")]
			public static extern uint RealGetWindowClass(IntPtr hWnd, [Out] StringBuilder lpWindowClass, int nMaxCount);

			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			[SuppressMessage("ReSharper", "UnusedMember.Local")]
			public class NOTIFYICONDATA
			{
				public int cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA));
				public IntPtr hWnd;
				public int uID;
				public int uFlags;
				public int uCallbackMessage;
				public IntPtr hIcon;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x80)]
				public string szTip;
				public int dwState;
				public int dwStateMask;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
				public string szInfo;
				public int uTimeoutOrVersion;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x40)]
				public string szInfoTitle;
				public int dwInfoFlags;
			}

			public enum NotifyIconMessages
			{
				NIM_DELETE = 0x0002
			}

			[DllImport("shell32.dll")]
			public static extern bool Shell_NotifyIcon(NotifyIconMessages Message, NOTIFYICONDATA data);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint lpdwProcessId);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern bool IsWindow(IntPtr hWnd);

			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern IntPtr OpenProcess(
				ProcessAccessFlags processAccess,
				bool bInheritHandle,
				uint processId
			);

			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern bool CloseHandle(IntPtr hHandle);


			[Flags]
			public enum ProcessAccessFlags : uint
			{
				VirtualMemoryOperation = 0x00000008,
				VirtualMemoryRead = 0x00000010,
				VirtualMemoryWrite = 0x00000020,
			}

			[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
			public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
				uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

			[Flags]
			public enum AllocationType
			{
				Commit = 0x1000,
				Release = 0x8000,
			}

			[Flags]
			public enum MemoryProtection
			{
				ReadWrite = 0x04,
			}

			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, uint nSize, out int lpNumberOfBytesWritten);


			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern bool ReadProcessMemory(
				IntPtr hProcess,
				IntPtr lpBaseAddress,
				[Out] byte[] buffer,
				int dwSize,
				out IntPtr lpNumberOfBytesRead);

			[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
			public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
				int dwSize, AllocationType dwFreeType);
		}

#pragma warning restore 169
#pragma warning restore 414

		private static class WinAPIUtils
		{
			private static int MakeWParam(int loWord, int hiWord) => (loWord & 0xFFFF) + ((hiWord & 0xFFFF) << 16);

			public static void SetComboBoxSelectedIndex(IntPtr hWnd, int index)
			{
				WinAPI.SendMessage(hWnd, (int)WinAPI.ComboBoxMessages.CB_SETCURSEL, index, IntPtr.Zero);

				var parentHwnd = WinAPI.GetWindowLongPtr(hWnd, (int)WinAPI.GetWindowLongIndexes.GWL_HWNDPARENT);
				var controlId = WinAPI.GetWindowLongPtr(hWnd, (int)WinAPI.GetWindowLongIndexes.GWL_ID).ToInt32();
				WinAPI.PostMessage(parentHwnd, (int)WinAPI.WindowsMessages.WM_COMMAND,
					WinAPIUtils.MakeWParam(controlId, (int)WinAPI.ComboBoxNotifications.CBN_SELCHANGE), hWnd);
			}

			public static string GetWindowText(IntPtr hWnd)
			{
				var textBuffer = new StringBuilder(255);
				WinAPI.GetWindowText(hWnd, textBuffer, textBuffer.Capacity + 1);
				return textBuffer.ToString();
			}

			public static string GetWindowClass(IntPtr hWnd)
			{
				var textBuffer = new StringBuilder(255);
				WinAPI.RealGetWindowClass(hWnd, textBuffer, textBuffer.Capacity + 1);
				return textBuffer.ToString();
			}
		}
	}
}
