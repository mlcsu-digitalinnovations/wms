using System;
using System.Collections.Generic;
using System.Linq;

namespace WmsHub.Business.Extensions
{
	public static class EnumerableExtension
	{
		public static IEnumerable<T> Randomize<T>(
			this IEnumerable<T> source)
		{
			var range = new Random();
			return source.Randomize(range);
		}

		private static IEnumerable<T> Randomize<T>(
			this IEnumerable<T> source, Random range)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (range == null) throw new ArgumentNullException("range");

			return source.RandomizeAlgorithm(range);
		}

		private static IEnumerable<T> RandomizeAlgorithm<T>(
			this IEnumerable<T> source, Random range)
		{
			var temp = source.ToList();

			for (int i = 0; i < temp.Count; i++)
			{
				int j = range.Next(i, temp.Count);
				yield return temp[j];

				temp[j] = temp[i];
			}
		}
	}
}