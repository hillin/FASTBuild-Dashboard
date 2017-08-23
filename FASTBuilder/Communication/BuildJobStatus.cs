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
		RacedOut,
		Stopped
	}
}
