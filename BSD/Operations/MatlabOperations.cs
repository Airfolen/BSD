using System.Collections.Generic;
using BSD.Graphs;

namespace BSD.Operations
{
    public class MatlabOperations
    {
        private static readonly MLApp.MLApp Matlab = new MLApp.MLApp();
        private static string _pathToPlotFunctionFolder;
        private static string _pathToBasicFunctionsFolder;

        public static object PlotGraphHistory(IGraph graph, string msg = "")
        {
            var graphHistory = graph.GetEdgesHistory();
            Matlab.Execute($"cd '{_pathToPlotFunctionFolder}'");
            Matlab.Feval("plot_graph_history", 0, out var figure, graphHistory.Source.ToArray(), 
                graphHistory.Target.ToArray(), graphHistory.Weigth.ToArray(), graphHistory.Marker.ToArray(),
                graph.Directed, msg);
            return figure;
        }

        public static object PlotGraph(IGraph graph, string msg = "")
        {
            Matlab.Execute($"cd '{_pathToPlotFunctionFolder}'");
            Matlab.Feval("plot_graph", 0, out var figure, graph.GetSource().ToArray(),
                graph.GetTarget().ToArray(), graph.GetWeights().ToArray(), graph.Directed, msg);
            return figure;
        }

        public static object PlotGraphAndHistory(IGraph graph, EdgesHistory graphHistory, string msg = "")
        {
            Matlab.Execute($"cd '{_pathToPlotFunctionFolder}'");
            Matlab.Feval("plot_graph_and_history", 0, out var figure, graph.GetSource().ToArray(),
                graph.GetTarget().ToArray(), graph.GetWeights().ToArray(), graphHistory.Source.ToArray(),
                graphHistory.Target.ToArray(), graphHistory.Weigth.ToArray(), graphHistory.Marker.ToArray(),
                graph.Directed, msg);
            return figure;
        }

        public static void CloseAll()
        {
            Matlab.Execute("close all");
        }

        public static void CloseFigure(object figure)
        {
            Matlab.Execute($"cd '{_pathToBasicFunctionsFolder}'");
            Matlab.Feval("close", 0, out _, figure);
        }

        public static void Pause(uint seconds = 0)
        {
            Matlab.Execute($"cd '{_pathToBasicFunctionsFolder}'");
            if (seconds == 0)
            {
                Matlab.Feval("pause", 0, out _);
                return;
            }
            Matlab.Feval("pause", 0, out _, seconds);
        }

        public static void Figure()
        {
            Matlab.Execute("figure");
        }

        public static object PlotBenchmarksResult(IEnumerable<int> x, IEnumerable<long> y1,
            IEnumerable<double> y2, string title = "", string xLabel = "", string y1Label = "", string y2Label = "")
        {
            object figure;
            Matlab.Execute($"cd '{_pathToPlotFunctionFolder}'");
            Matlab.Feval("plot_benchmarks", 0, out figure, x, y1, y2, title, xLabel, y1Label, y2Label);
            return figure;
        }

        public static void SetPathToPlotFunctionFolder(string path)
        {
            _pathToPlotFunctionFolder = path;
        }

        public static void SetPathToBasicFunctionsFolder(string path)
        {
            _pathToBasicFunctionsFolder = path;
        }
    }
}