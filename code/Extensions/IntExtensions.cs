using System;

namespace Cinema.Extensions
{
	public static class IntExtensions
	{
		public enum SizeUnits
		{
			Byte, KB, MB, GB, TB, PB, EB, ZB, YB
		}

		public static string ToSize(this int value, SizeUnits unit)
		{
			return (value / (double)Math.Pow(1024, (int)unit)).ToString("0.00");
		}
	}
}
