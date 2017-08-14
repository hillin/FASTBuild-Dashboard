using System;
using System.Collections.Generic;
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

namespace FastBuilder.Views
{
    public partial class TimeRulerMajorTickView 
    {
        public TimeRulerMajorTickView()
        {
            InitializeComponent();
        }

	    public void SetTime(TimeSpan time, string labelTextFormat)
	    {
		    this.LabelText.Text = time.ToString(labelTextFormat);
	    }
    }
}
