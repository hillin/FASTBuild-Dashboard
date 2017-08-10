using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuilder.Services
{
	internal interface IScaleService
    {
		double Scaling { get; set; }
	    event EventHandler ScalingChanged;
    }
}
