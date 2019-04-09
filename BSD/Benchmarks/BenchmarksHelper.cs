using System;
using BSD.Benchmarks.Estimator;
using BSD.Graphs;
using BSD.Operations;

namespace BSD.Benchmarks
{
    public static class BenchmarksHelper
    {
        public static DirectedGraph GenerateDirectedGraph(int verticesCount)
        {
            var graph = new DirectedGraph();

            for (var i = 0; i < verticesCount; i++)
            {
                graph.AddVertex($"A{i}");
            }

            foreach (var v1 in graph.Vertices())
            {
                foreach (var v2 in graph.Vertices())
                {
                    if (v2 == v1)
                    {
                        continue;
                    }
                    graph.AddEdge(v1, v2, 0);
                }
            }

            return graph;
        }

        public static void PrintBenchmarksResultInMatlab(IAlgorithm algorithm, int beginValues = 1, int endValues = 10,
                int step = 1, int repetitionsCount = 5, IАveragerResults averageResult = null, string title = "", 
                string xLabel = "", string y1Label = "", string y2Label = "")
        {
            if (averageResult == null)
            {
                averageResult = new Average(0.8M);
            }

            var results = СomplicationEstimator.Estimate(algorithm, beginValues, endValues,
                step, repetitionsCount, averageResult);

            var x = new int[results.Count];
            var y = new long[results.Count];
            var m = new double[results.Count];

            for (var i = 0; i < results.Count; i++)
            {
                x[i] = results[i].Size;
                y[i] = results[i].Time;
                m[i] = results[i].Memory;
            }

            MatlabOperations.Figure();
            MatlabOperations.PlotBenchmarksResult(x, y, m, title, xLabel, y1Label, y2Label);
            MatlabOperations.Pause();
        }
    }
}