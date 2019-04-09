using System;
using System.Collections.Generic;
using System.IO;
using BSD.Graphs;
using Newtonsoft.Json;

namespace BSD.Operations
{
    public static class FileOperations
    {
        public static DirectedGraph GetDirectedGraphFromJson(string path)
        {
            var graph = new DirectedGraph();

            using (var r = new StreamReader(path))
            {
                var json = r.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);

                if (array == null)
                {
                    return null;
                }

                foreach (var v in array.vertices)
                {
                    if (v.name == null)
                    {
                        throw new InvalidOperationException("Отсутствует обязательный атрибут name");
                    }

                    var incomingVertices = new Dictionary<string, double>();
                    var outcomingVertices = new Dictionary<string, double>();

                    if (v.incoming != null)
                    {
                        foreach (var incoming in v.incoming)
                        {
                            if (incoming.name == null)
                            {
                                throw new InvalidOperationException("Отсутствует обязательный атрибут name");
                            }

                            double weight = 0;
                            if (incoming.weight != null)
                            {
                                weight = (double)incoming.weight;
                            }

                            incomingVertices.Add((string)incoming.name, weight);
                        }
                    }

                    if (v.outcoming != null)
                    {
                        foreach (var outcoming in v.outcoming)
                        {
                            if (outcoming.name == null)
                            {
                                throw new InvalidOperationException("Отсутствует обязательный атрибут name");
                            }

                            double weight = 0;
                            if (outcoming.weight != null)
                            {
                                weight = (double)outcoming.weight;
                            }

                            outcomingVertices.Add((string)outcoming.name, weight);
                        }
                    }

                    if (incomingVertices.Count == 0 && outcomingVertices.Count == 0)
                    {
                        graph.AddVertex((string)v.name);
                        continue;
                    }

                    if (incomingVertices.Count == 0)
                    {
                        graph.AddVertex((string)v.name, null, outcomingVertices);
                        continue;
                    }

                    if (outcomingVertices.Count == 0)
                    {
                        graph.AddVertex((string)v.name, incomingVertices);
                        continue;
                    }

                    graph.AddVertex((string)v.name, incomingVertices, outcomingVertices);
                }
            }
            return graph;
        }

        public static void SetMatlabConfigFromJson(string path)
        {
            using (var r = new StreamReader(path))
            {
                var json = r.ReadToEnd();
                dynamic config = JsonConvert.DeserializeObject(json);

                if (config == null)
                {
                    throw new InvalidOperationException("Некорректный файл конфигурации");
                }
                CheckConfig(config);

                MatlabOperations.SetPathToBasicFunctionsFolder($"{(string)config.pathToMatlabFolder}" +
                                                               "\\toolbox\\idelink\\extensions\\ticcs");

                MatlabOperations.SetPathToPlotFunctionFolder($"{(string)config.pathToPlotFunctionsFolder}");
            }
        }

        private static void CheckConfig(dynamic config)
        {
            if (config.pathToMatlabFolder == null)
            {
                throw new InvalidOperationException("Отсутствует обязательный атрибут pathToMatlabFolder");
            }

            if (config.pathToPlotFunctionsFolder == null)
            {
                throw new InvalidOperationException("Отсутствует обязательный атрибут pathToPlotFunctionsFolder");
            }
        }
    }
}