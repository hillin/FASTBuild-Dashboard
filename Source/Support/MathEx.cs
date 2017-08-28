namespace FastBuild.Dashboard.Support
{
	internal static class MathEx
	{
		public static double Clamp(double value, double min, double max)
		{
			return value > max ? max : value < min ? min : value;
		}
	}
}
