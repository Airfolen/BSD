using System;

namespace BSD.Benchmarks.Estimator
{
	public class Average : IАveragerResults
	{
		private readonly decimal _proportionOfSavedResults;

		public Average(decimal proportionOfSavedResults)
		{
			_proportionOfSavedResults = proportionOfSavedResults;
		}

		public long Calculate(long[] resultsArray)
		{
			long sum = 0;
			var savedResults = DiscardMaximumResults(resultsArray, _proportionOfSavedResults);
			foreach (var element in savedResults)
			{
				sum += element;
			}
			return sum / resultsArray.Length;
		}

		private long[] DiscardMaximumResults(long[] resultsArray, decimal proportionOfSavedResults)
		{
			var newResultsArray = new long[Convert.ToInt16(resultsArray.Length * proportionOfSavedResults)];
			Array.Sort(resultsArray);
			for (var index = 0; index < newResultsArray.Length; index++)
			{
				newResultsArray[index] = resultsArray[index];
			}
			return newResultsArray;
		}
	}
}
