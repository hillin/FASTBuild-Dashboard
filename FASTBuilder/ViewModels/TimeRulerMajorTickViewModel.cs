using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuilder.ViewModels
{
	internal class TimeRulerMajorTickViewModel : TimeRulerTickViewModelBase
	{
		private string _labelText;

		public string LabelText
		{
			get => _labelText;
			set
			{
				if (value == _labelText) return;
				_labelText = value;
				this.NotifyOfPropertyChange();
			}
		}
	}
}
