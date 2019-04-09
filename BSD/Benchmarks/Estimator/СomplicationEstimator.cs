using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BSD.Benchmarks.Estimator
{
	public class СomplicationEstimator
	{
		public static List<Measurement> Estimate(IAlgorithm algorithm, int beginValues, int endValues, 
            int step, int repetitionsCount, IАveragerResults averageResult)
		{
			var measurements = new List<Measurement>();
			var data = new long[repetitionsCount];
            var memories = new long[repetitionsCount];
			var stopwatch = new Stopwatch();
			for (var number = beginValues; number <= endValues; number += step)
			{
                for (var repetition = 0; repetition < repetitionsCount; repetition++)
				{
					algorithm.Prepare(number);
                    stopwatch.Start();
                    algorithm.Execute();
				    memories[repetition] = Process.GetCurrentProcess().WorkingSet64;
                    stopwatch.Stop();
					data[repetition] = stopwatch.ElapsedMilliseconds;
					stopwatch.Reset();
				}
				measurements.Add(new Measurement
				{
				    Size = number,
                    Time = averageResult.Calculate(data),
                    Memory = (double)averageResult.Calculate(memories)/(1024 * 1024)
                });
			}
			return measurements;
		}
	}
}
