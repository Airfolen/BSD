using System.Collections.Generic;

namespace BSD.Graphs
{
    public interface IGraph
    {
        bool Directed { get; }
        EdgesHistory GetEdgesHistory();
        List<double> GetWeights();
        List<string> GetTarget();
        List<string> GetSource();
    }
}