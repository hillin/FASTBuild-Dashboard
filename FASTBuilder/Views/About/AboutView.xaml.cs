using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace FastBuilder.Views.About
{
	public partial class AboutView 
	{
		public AboutView() => InitializeComponent();

		private void Hyperlink_Click(object sender, RoutedEventArgs e) => Process.Start(((Hyperlink)sender).NavigateUri.ToString());
	}
}
