using System;
using System.Collections.Generic;
using System.Linq;

namespace BSD.Graphs
{
    public class UndirectedGraph : IGraph
    {
        private readonly List<string> _source;
        private readonly List<string> _target;
        private readonly List<double> _weights;
        private readonly List<string[]> _edges;
        private readonly bool _saveStates;
        private readonly EdgesHistory _edgesHistory;

        private List<string> _all;

        public bool Directed { get; } = false;

        public UndirectedGraph(IEnumerable<string> source = null, IEnumerable<string> target = null,
            IEnumerable<double> weights = null, bool saveStates = true)
        {
            _source = source as List<string> ?? new List<string>();
            _target = target as List<string> ?? new List<string>();
            _weights = weights as List<double> ?? new List<double>();
            _all = _source.Concat(_target).Distinct().ToList();

            _edges = new List<string[]>();
            for (var i = 0; i < _source.Count; i++)
            {
                _edges.Add(new []{_source[i], _target[i]});
            }

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

        public List<string[]> GetEdges()
        {
            return _edges;
        }

        public void InitVertices(IEnumerable<string> vertices)
        {
            _all = vertices as List<string>;
        }

        public void AddVertex(string name, Dictionary<string, double> vertices= null)
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

            if (vertices == null)
            {
                return;
            }

            foreach (var v in vertices)
            {
                if (!_all.Contains(v.Key))
                {
                    throw new InvalidOperationException($"Граф не содержит вершину {v.Key}");
                }

                if (ContainsEdge(v.Key, name))
                {
                    continue;
                }

                _source.Add(v.Key);
                _target.Add(name);
                _weights.Add(v.Value);
                _edges.Add(new[] { v.Key, name });

                if (!_saveStates)
                {
                    continue;
                }
                _edgesHistory.Marker.Add(1);
                _edgesHistory.Source.Add(v.Key);
                _edgesHistory.Target.Add(name);
                _edgesHistory.Weigth.Add(v.Value);
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

            if (ContainsEdge(begin, end))
            {
                return;
            }

            _source.Add(begin);
            _target.Add(end);
            _weights.Add(weight);
            _edges.Add(new[] {begin, end});

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

                _edges.Remove(new[] {_source[i], _target[i]});
                _edges.Remove(new[] { _target[i], _source[i] });
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

            _edges.RemoveAll(e => e[0] == name || e[1] == name);

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

        public bool ContainsEdge(string begin, string end)
        {
            var f = _edges.FirstOrDefault(e => e[0] == begin && e[1] == end);
            var s = _edges.FirstOrDefault(e => e[0] == end && e[1] == begin);
            return f != null || s != null;
        }
    }
}