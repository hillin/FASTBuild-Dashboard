using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FastBuild.Dashboard.Communication
{
	internal class LogWatcher
	{
		private const string LogRelativePath = @"FASTBuild\FastBuildLog.log";

		private readonly string _logPath;

		public event EventHandler HistoryRestorationStarted;
		public event EventHandler HistoryRestorationEnded;

		public event EventHandler LogReset;
		public event EventHandler<string> LogReceived;

		private long _fileStreamPosition;
		private DateTime _currentFileTime;
		private readonly List<byte> _messageBuffer = new List<byte>();

		public bool IsRestoringHistory { get; private set; }

		public LogWatcher()
		{
#if DEBUG && DEBUG_TEST_LOG
			_logPath = @"Test\FastBuildLog.log";
#else
			var path = Path.GetTempPath();
			var fastbuildTempPath = Environment.GetEnvironmentVariable("FASTBUILD_TEMP_PATH");
			if (fastbuildTempPath != null && System.IO.Directory.Exists(fastbuildTempPath))
			{
				path = fastbuildTempPath;
			}

			_logPath = Path.Combine(path, LogRelativePath);
#endif
		}

		public void Start()
		{
			var logDirectory = Path.GetDirectoryName(_logPath);
			if (!Directory.Exists(logDirectory))
			{
				Debug.Assert(logDirectory != null, "logDirectory != null");
				Directory.CreateDirectory(logDirectory);
			}

			if (File.Exists(_logPath))
			{
				this.IsRestoringHistory = true;
				this.HistoryRestorationStarted?.Invoke(this, EventArgs.Empty);
			}

			Task.Factory.StartNew(this.ReadRemainingLogsAsync);
		}

		private void ReadRemainingLogsAsync()
		{
			_fileStreamPosition = 0;
			while (true)
			{
				if (File.Exists(_logPath))
				{
					var fileTime = File.GetLastWriteTime(_logPath);
					if (fileTime != _currentFileTime		// the log file has been reset or saved (closed)
						&& new FileInfo(_logPath).Length < _fileStreamPosition)	// this guarantees it is a reset
					{
						_fileStreamPosition = 0;
						this.LogReset?.Invoke(this, EventArgs.Empty);
						_currentFileTime = fileTime;
					}

					using (var file = new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						var expectedLength = (int)(file.Length - _fileStreamPosition);

						if (expectedLength > 0)
						{
							var buffer = new byte[expectedLength];

							file.Seek(_fileStreamPosition, SeekOrigin.Begin);
							_fileStreamPosition += file.Read(buffer, 0, expectedLength);

							foreach (var c in buffer)
							{
								if (c == '\n')
								{
									this.FlushMessage();
									continue;
								}

								_messageBuffer.Add(c);
							}
						}
					}

					if (this.IsRestoringHistory)
					{
						this.IsRestoringHistory = false;
						this.HistoryRestorationEnded?.Invoke(this, EventArgs.Empty);
					}
				}
				else
				{
					_fileStreamPosition = 0;
				}

				Thread.Sleep(500);
			}
		}

		private void FlushMessage()
		{
			if (_messageBuffer.Count > 0)
			{
				var message = Encoding.Default.GetString(_messageBuffer.ToArray(), 0, _messageBuffer.Count);
				this.LogReceived?.Invoke(this, message);
				_messageBuffer.Clear();
			}
		}
	}
}
