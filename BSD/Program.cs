using BSD.Benchmarks;
using BSD.Operations;
using System;

namespace BSD
{
    public class Program
    {
        private static void Main(string[] args)
        {
            FileOperations.SetMatlabConfigFromJson("data/config.json");


            var graph1 = FileOperations.GetDirectedGraphFromJson("data/graph.json");
            var state = graph1.GetMoralGraph();
            var tmp = state.Item1;
            var final = state.Item2;


            MatlabOperations.Figure();
            var figure0 = MatlabOperations.PlotGraph(graph1, "Исходный граф");
            MatlabOperations.Pause(5);

            MatlabOperations.Figure();
            var figure1 = MatlabOperations.PlotGraph(tmp, "Исходный неориентированный граф");
            MatlabOperations.Pause(5);

            MatlabOperations.Figure();
            var figure2 = MatlabOperations.PlotGraphAndHistory(tmp, final.GetEdgesHistory(), "Морализованный граф");
            MatlabOperations.Pause(5);

            var secondStage = graph1.TriangulateGraph(final);
            MatlabOperations.Figure();
            var figure3 = MatlabOperations.PlotGraph(secondStage, "Триангулированный граф");
            MatlabOperations.Pause();
            

            //BenchmarksHelper.PrintBenchmarksResultInMatlab(new MoralizationAlgorithm(), 0, 100, 5,
            //    title: "Бенчмарк для морализации", xLabel: "Число вершин", y1Label: "Время, мс.", y2Label: "Память, МБ");
        }
    }
}
