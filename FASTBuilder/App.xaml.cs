using System.Collections.Generic;
using System.Windows;
using Microsoft.Shell;

namespace FASTBuilder
{
	public partial class App : ISingleInstanceApp
	{
		public App() => this.InitializeComponent();
		public bool SignalExternalCommandLineArgs(IList<string> args)
		{
			Application.Current.MainWindow.Show();
			Application.Current.MainWindow.Activate();
			return true;
		}
	}
}
