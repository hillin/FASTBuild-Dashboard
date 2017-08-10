using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using FastBuilder.Services;

namespace FastBuilder.ViewModels
{
	internal abstract class TimeRulerTickViewModelBase : PropertyChangedBase
	{
		private TimeSpan _time;
		private Thickness _uiMargin;
		private double _uiWidth;

		public TimeSpan Time
		{
			get => _time;
			set
			{
				if (value.Equals(_time)) return;
				_time = value;
				this.NotifyOfPropertyChange();
			}
		}

		public Thickness UIMargin
		{
			get => _uiMargin;
			set
			{
				if (value.Equals(_uiMargin)) return;
				_uiMargin = value;
				this.NotifyOfPropertyChange();
			}
		}

		public double UIWidth
		{
			get => _uiWidth;
			set
			{
				if (value.Equals(_uiWidth)) return;
				_uiWidth = value;
				this.NotifyOfPropertyChange();
			}
		}
	}
}
