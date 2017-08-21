using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuilder.Services
{
	internal interface IBrokerageService
    {
		string[] WorkerNames { get; }
		string BrokeragePath { get; set; }

		event EventHandler WorkerCountChanged;
    }
}
