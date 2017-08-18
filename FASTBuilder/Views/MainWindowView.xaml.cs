using System.Windows;

namespace FastBuilder.Views
{
	public partial class MainWindowView
	{
		public MainWindowView()
		{
			this.InitializeComponent();
		}

		private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			this.MenuToggleButton.IsChecked = false;
		}
	}
}
