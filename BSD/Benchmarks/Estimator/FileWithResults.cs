using System;
using System.Collections.Generic;
using System.IO;

namespace BSD.Benchmarks.Estimator
{
	public class FileWithResults
    {
		public static void WriteToFile(string fileName, List<Measurement> measurements)
		{
			{
				foreach (var measurement in measurements)
				{
					File.AppendAllText(fileName, Convert.ToString(measurement.Size)+";"+ Convert.ToString(measurement.Time) + Environment.NewLine);
				}
				File.AppendAllText(fileName, Environment.NewLine);
				File.AppendAllText(fileName, Environment.NewLine);
			}
		}

		public static void Clear(string fileName)
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}
	}
}
