using System;

namespace EpidemicSimulator
{
	public static class Randomness
	{
		private static Random _rng = new Random();

		public static T Next<T>(this T[] source)
		{
			var index = _rng.Next(0, source.Length);
			return source[index];
		}

		public static int Percent()
		{
			return _rng.Next(0, 100);
		}
	}
}
