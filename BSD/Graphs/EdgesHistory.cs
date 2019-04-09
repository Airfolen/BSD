using System.Collections.Generic;

namespace BSD.Graphs
{
    public struct EdgesHistory
    {
        public List<string> Source { get; internal set; }
        public List<string> Target { get; internal set; }
        public List<double> Weigth { get; internal set; }
        public List<byte> Marker { get; internal set; }
    }
}