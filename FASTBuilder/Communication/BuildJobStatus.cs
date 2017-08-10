using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBuilder.Communication
{
	internal enum BuildJobStatus
	{
		Building,
		Success,
		SuccessCached,
		SuccessPreprocessed,
		Failed,
		Error,
		Timeout,
		Stopped
	}
}
