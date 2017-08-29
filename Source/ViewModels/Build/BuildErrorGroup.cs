using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FastBuild.Dashboard.Support;
using Microsoft.CSharp.RuntimeBinder;

namespace FastBuild.Dashboard.ViewModels.Build
{
	internal class BuildErrorGroup
	{
		public string FilePath { get; }
		public IEnumerable<BuildErrorInfo> Errors { get; }

		public BuildErrorGroup(string fileName, IEnumerable<BuildErrorInfo> errors)
		{
			this.FilePath = fileName;
			this.Errors = errors;
		}
	}
}
