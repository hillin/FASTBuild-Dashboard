using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuild.Dashboard.ViewModels.Build
{
	internal class BuildErrorInfo
	{
		public string FilePath { get; }
		public int LineNumber { get; }
		public string ErrorMessage { get; }


		public BuildErrorInfo(string filePath, int lineNumber, string errorMessage)
		{
			this.FilePath = filePath;
			this.LineNumber = lineNumber;
			this.ErrorMessage = errorMessage;
		}
	}
}
