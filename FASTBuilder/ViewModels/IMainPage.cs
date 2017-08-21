using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FastBuilder.ViewModels
{
    internal interface IMainPage 
    {
		string Icon { get; }
		string DisplayName { get; }
    }
}
