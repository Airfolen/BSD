using System;
using System.Collections.Generic;
using System.Linq;

namespace BSD.Graphs
{
    public class DirectedGraph : IGraph
    {
        private readonly List<string> _source;
        private readonly List<string> _target;
        private readonly List<double> _weights;
        private readonly List<string> _all;
        private readonly bool _saveStates;
        private readonly EdgesHistory _edgesHistory;

        public bool Directed { get; } = true;

        public DirectedGraph(IEnumerable<string> source = null, IEnumerable<string> target = null, 
            IEnumerable<double> weights = null, bool saveStates = true)
        {
            _source = source as List<string> ?? new List<string>();
            _target = target as List<string> ?? new List<string>();
            _weights = weights as List<double> ?? new List<double>();
            _all = _source.Concat(_target).Distinct().ToList();
            if (!saveStates)
            {
                return;
            }
            _saveStates = true;
            _edgesHistory = new EdgesHistory
            {
                Marker = new List<byte>(),
                Source = new List<string>(),
                Target = new List<string>(),
                Weigth = new List<double>()
            };
        }

        public List<string> GetSource()
        {
            return _source;
        }

        public List<string> GetTarget()
        {
            return _target;
        }

        public List<double> GetWeights()
        {
            return _weights;
        }

        public EdgesHistory GetEdgesHistory()
        {
            return _edgesHistory;
        }

        public List<string> Vertices()
        {
            return _all;
        }

        public void AddVertex(string name, Dictionary<string, double> incoming = null, 
            Dictionary<string, double> outcoming = null)
        {
            if (name == null)
            {
                return;
            }

            if (_all.Contains(name))
            {
                throw new InvalidOperationException($"Граф уже содержит вершину {name}");
            }
            _all.Add(name);

            if (_saveStates)
            {
                _edgesHistory.Marker.Add(1);
                _edgesHistory.Source.Add(name);
                _edgesHistory.Target.Add(name);
                _edgesHistory.Weigth.Add(0);
            }

            if (incoming != null)
            {
                foreach (var edge in incoming)
                {
                    if (!_all.Contains(edge.Key))
                    {
                        throw new InvalidOperationException($"Граф не содержит вершину {edge.Key}");
                    }

                    _source.Add(edge.Key);
                    _target.Add(name);
                    _weights.Add(edge.Value);

                    if (!_saveStates)
                    {
                        continue;
                    }
                    _edgesHistory.Marker.Add(1);
                    _edgesHistory.Source.Add(edge.Key);
                    _edgesHistory.Target.Add(name);
                    _edgesHistory.Weigth.Add(edge.Value);
                }
            }

            if (outcoming == null)
            {
                return;
            }

            foreach (var edge in outcoming)
            {
                if (!_all.Contains(edge.Key))
                {
                    throw new InvalidOperationException($"Граф не содержит вершину {edge.Key}");
                }

                _source.Add(name);
                _target.Add(edge.Key);
                _weights.Add(edge.Value);

                if (!_saveStates)
                {
                    continue;
                }

                _edgesHistory.Marker.Add(1);
                _edgesHistory.Source.Add(name);
                _edgesHistory.Target.Add(edge.Key);
                _edgesHistory.Weigth.Add(edge.Value);
            }
        }

        public void AddEdge(string begin, string end, double weight)
        {
            if (!_all.Contains(begin))
            {
                throw new InvalidOperationException($"Граф не содержит вершину {begin}");
            }

            if (!_all.Contains(end))
            {
                throw new InvalidOperationException($"Граф не содержит вершину {end}");
            }

            _source.Add(begin);
            _target.Add(end);
            _weights.Add(weight);

            if (!_saveStates)
            {
                return;
            }

            _edgesHistory.Marker.Add(1);
            _edgesHistory.Source.Add(begin);
            _edgesHistory.Target.Add(end);
            _edgesHistory.Weigth.Add(weight);
        }

        public void RemoveEdge(string begin, string end)
        {
            if (!_all.Contains(begin))
            {
                return;
            }

            if (!_all.Contains(end))
            {
                return;
            }

            for (var i = _source.Count - 1; i >= 0; i--)
            {
                if (_source[i] != begin || _target[i] != end)
                {
                    continue;
                }
                _source.RemoveAt(i);
                _target.RemoveAt(i);

                if (!_saveStates)
                {
                    continue;
                }

                _edgesHistory.Marker.Add(0);
                _edgesHistory.Source.Add(begin);
                _edgesHistory.Target.Add(end);
                _edgesHistory.Weigth.Add(0);
            }
        }

        public void RemoveVertex(string name)
        {
            if (!_all.Contains(name))
            {
                return;
            }
            _all.Remove(name);

            if (_saveStates)
            {
                _edgesHistory.Marker.Add(0);
                _edgesHistory.Source.Add(name);
                _edgesHistory.Target.Add(name);
                _edgesHistory.Weigth.Add(0);
            }

            for (var i = _source.Count - 1; i >= 0; i--)
            {
                if (_source[i] != name && _target[i] != name)
                {
                    continue;
                }

                _source.RemoveAt(i);
                _target.RemoveAt(i);
            }
        }

        public bool ContainsVertex(string name)
        {
            return _all.Contains(name);
        }

        public Tuple<UndirectedGraph, UndirectedGraph> GetMoralGraph()
        {
            var tmp = new UndirectedGraph();
            tmp.InitVertices(_all);

            foreach (var vertex in _all)
            {
                var children = GetTargetsBySource(vertex);
                foreach (var ch in children)
                {
                    tmp.AddEdge(vertex, ch, 0);
                }
            }

            var result = new UndirectedGraph(
                new List<string>(tmp.GetSource()), 
                new List<string>(tmp.GetTarget()),
                new List<double>(tmp.GetWeights()));

            foreach (var vertex in _all)
            {
                var parents = GetSourcesByTarget(vertex);
                
                if (parents.Count <= 1)
                {
                    continue;
                }

                for (var i = 0; i < parents.Count - 1; i++)
                {
                    result.AddEdge(parents[i], parents[i + 1], 0);
                }
                result.AddEdge(parents[parents.Count - 1], parents[0], 0);
            }

            return new Tuple<UndirectedGraph, UndirectedGraph>(tmp, result);
        }

        private List<string> GetSourcesByTarget(string target)
        {
            var result = new List<string>();

            for (var i = 0; i < _target.Count; i++)
            {
                if (_target[i] == target)
                {
                    result.Add(_source[i]);
                }
            }

            return result;
        }

        private List<string> GetTargetsBySource(string source)
        {
            var result = new List<string>();

            for (var i = 0; i < _source.Count; i++)
            {
                if (_source[i] == source)
                {
                    result.Add(_target[i]);
                }
            }

            return result;
        }
    }
}