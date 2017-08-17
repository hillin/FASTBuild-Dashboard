using System;

namespace FastBuilder.Views.Build
{
    public partial class TimeRulerMajorTickView 
    {
        public TimeRulerMajorTickView()
        {
            InitializeComponent();
        }

	    public void SetTime(TimeSpan time, string labelTextFormat)
	    {
		    // ReSharper disable once CompareOfFloatsByEqualityOperator
		    this.LabelText.Text = time.TotalSeconds == 0 ? string.Empty : time.ToString(labelTextFormat);
	    }
    }
}
