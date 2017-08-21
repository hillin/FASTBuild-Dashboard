using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FastBuilder.Views.About
{
	public partial class AboutView 
	{
		public AboutView() => InitializeComponent();

		private void Hyperlink_Click(object sender, RoutedEventArgs e) => Process.Start(((Hyperlink)sender).NavigateUri.ToString());
	}
}
