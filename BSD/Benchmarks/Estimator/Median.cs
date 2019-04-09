using System;

namespace BSD.Benchmarks.Estimator
{
	public class Median : IАveragerResults
	{
		public long Calculate(long[] resultsArray)
		{
			Array.Sort(resultsArray);
			var middle = resultsArray.Length / 2;
			return resultsArray[middle];
		}
	}
}
