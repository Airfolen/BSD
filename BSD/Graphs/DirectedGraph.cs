using BSD.Comparers;
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
        private readonly List<string> _catalogCycles;

        public bool Directed { get; } = true;

        public DirectedGraph(IEnumerable<string> source = null, IEnumerable<string> target = null, 
            IEnumerable<double> weights = null, bool saveStates = true)
        {
            _source = source as List<string> ?? new List<string>();
            _target = target as List<string> ?? new List<string>();
            _weights = weights as List<double> ?? new List<double>();
            _catalogCycles = new List<string>();
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

        public UndirectedGraph TriangulateGraph(UndirectedGraph graph)
        {
            //var list1 = new List<List<string>> {
            //            new List<string> { "a", "b" },
            //            new List<string> { "d", "e" },
            //            new List<string> { "a", "j" },
            //            new List<string> { "q", "b" },
            //            new List<string> { "b", "c" }
            //        };

            //var list2 = new List<string> { "a", "b" };

            var cycles = GetCycles(graph);

            while (cycles.Count() > 0)
            {
                foreach (var cycle in cycles)
                {
                    foreach (var vertex in cycle)
                    {
                        var edges = graph.GetEdges().Select(a => a.ToList()).ToList();
                        var parents = edges.Where(a => a.Contains(vertex) && a.All(b => cycle.Contains(b))).SelectMany(a => a)
                            .Where(a => a != vertex).Distinct().ToList();
                        if (parents.Count() > 2) continue;

                        if (edges.All(a =>
                            {
                                return ContainsAllItems(a, parents);
                            })) continue;

                        graph.AddEdge(parents[0], parents[1], 0);
                    }
                }

                cycles = GetCycles(graph);
            }

            return graph;
        }

        class Edge
        {
            public int v1, v2;

            public Edge(int v1, int v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }
        }

        /// <summary>
        /// Поиск всех неповторяющихся циклов в графе без вложенных подциклов
        /// </summary>
        public List<List<string>> GetCycles(UndirectedGraph graph)
        {
            // подготовка вершин и ребер для алгоритма поиска циклов
            var matchingDictionary = _all.ToDictionary(key => key, value => _all.IndexOf(value));
            var edges = new List<Edge>();            
            foreach (var edge in graph.GetEdges())
            {
                edges.Add(new Edge(matchingDictionary[edge[0]], matchingDictionary[edge[1]]));
            }

            var color = new int[_all.Count];
            for (var i = 0; i < _all.Count; i++)
            {
                for (var k = 0; k < _all.Count; k++)
                    color[k] = 1;
            
                var cycle = new List<int>();
                cycle.Add(i + 1);
            
                DFScycle(i, i, edges, color, -1, cycle);
            }

            // приведение результа к виду List<List<string>>()
            var result = new List<List<string>>();
            foreach(var cycle in _catalogCycles)
            {
                var splitedCycle = cycle.Split(new char[] {'-'});
                var cycleResult = new List<string>();
                foreach (var vertex in splitedCycle.Take(splitedCycle.Count() - 1))
                {
                    cycleResult.Add(matchingDictionary.First(a => a.Value == Int32.Parse(vertex) - 1).Key);
                }
                result.Add(cycleResult);
            }

            // удаление палиндромных циклов
            result = result.Distinct(new EnumerableComparer<string>()).Select(a => a.ToList()).ToList();

            
            // Получение циклов без вложенных подциклов
            var resultWithoutNestedSubLoops = new List<List<string>>();
            foreach (var checkingList in result)
            {
                var flag = true;
                foreach (var listFromResult in result)
                {
                    if (checkingList == listFromResult) continue;
                    if (ContainsAllItems(checkingList, listFromResult))
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag) resultWithoutNestedSubLoops.Add(checkingList);
            }

            // пропускаем циклы с 3мя вершинами
            return resultWithoutNestedSubLoops.Where(a => a.Count > 3).ToList();
        }

        /// <summary>
        /// Содежит ли список a все элементы списка b
        /// </summary>
        private static bool ContainsAllItems<T>(List<T> a, List<T> b)
        {
            return b.TrueForAll(delegate (T t)
            {
                return a.Contains(t);
            });
        }

        /// <summary>
        /// Алгоритм взятый из жепы, который мне не хочется переписывать
        /// </summary>
        /// <param name="u">номер текущей вершины</param>
        /// <param name="endV">номер конечной вершины цикла</param>
        /// <param name="E">список ребер</param>
        /// <param name="color">массив, в котором хранятся цвета вершин</param>
        /// <param name="unavailableEdge">Вершину, для которой ищем цикл, перекрашивать в черный цвет не будем,
        /// иначе программа не сможет в нее вернуться, поскольку эта вершина окажется недоступной. 
        /// Поэтому введем переменную unavailableEdge, в которую будем передавать номер последнего ребра,
        /// по которому осуществлялся переход между вершинами, соответственно это ребро на следующем шаге 
        /// окажется недоступным для перехода. В действительности это необходимо только на первом уровне рекурсии,
        /// чтобы избежать некорректной работы алгоритма и вывода.</param>
        /// <param name="cycle">список, в который заносится последовательность вершин искомого цикла</param>
        private void DFScycle(int u, int endV, List<Edge> E, int[] color, int unavailableEdge, List<int> cycle)
        {
            //если u == endV, то эту вершину перекрашивать не нужно, иначе мы в нее не вернемся, а вернуться необходимо
            if (u != endV)
                color[u] = 2;
            else if (cycle.Count >= 2)
            {
                cycle.Reverse();
                string s = cycle[0].ToString();
                for (int i = 1; i < cycle.Count; i++)
                    s += "-" + cycle[i].ToString();
                bool flag = false; //есть ли палиндром для этого цикла графа в List<string> catalogCycles?
                for (int i = 0; i < _catalogCycles.Count; i++)
                    if (_catalogCycles[i].ToString() == s)
                    {
                        flag = true;
                        break;
                    }
                if (!flag)
                {
                    cycle.Reverse();
                    s = cycle[0].ToString();
                    for (int i = 1; i < cycle.Count; i++)
                        s += "-" + cycle[i].ToString();
                    _catalogCycles.Add(s);
                }
                return;
            }
            for (int w = 0; w < E.Count; w++)
            {
                if (w == unavailableEdge)
                    continue;
                if (color[E[w].v2] == 1 && E[w].v1 == u)
                {
                    List<int> cycleNEW = new List<int>(cycle);
                    cycleNEW.Add(E[w].v2 + 1);
                    DFScycle(E[w].v2, endV, E, color, w, cycleNEW);
                    color[E[w].v2] = 1;
                }
                else if (color[E[w].v1] == 1 && E[w].v2 == u)
                {
                    List<int> cycleNEW = new List<int>(cycle);
                    cycleNEW.Add(E[w].v1 + 1);
                    DFScycle(E[w].v1, endV, E, color, w, cycleNEW);
                    color[E[w].v1] = 1;
                }
            }
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