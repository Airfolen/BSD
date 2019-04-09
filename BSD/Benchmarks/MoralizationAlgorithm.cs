using BSD.Benchmarks.Estimator;
using BSD.Graphs;

namespace BSD.Benchmarks
{
    public class MoralizationAlgorithm : IAlgorithm
    {
        private DirectedGraph _graph;

        public void Prepare(int size)
        {
            _graph = BenchmarksHelper.GenerateDirectedGraph(size);
        }

        public void Execute()
        {
            var tmp = _graph.GetMoralGraph();
        }
    }
}