using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FastBuilder.ViewModels
{
	internal class MainWindowViewModel : PropertyChangedBase
	{
		public BuildWatcherViewModel BuildWatcher { get; } = new BuildWatcherViewModel();
	}
}
