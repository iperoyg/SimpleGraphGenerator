using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphGenerator
{
    class Program
    {

        static void Main(string[] args)
        {
            int.TryParse(args[0], out var verticesCount);
            float.TryParse(args[1], out var removeEdgesPercentage);
            removeEdgesPercentage /= 100f;
            var numSamples = 15;

            var graphGenerator = new GraphGenerator();

            var graphList = GenerateGraphSamplesAsync(removeEdgesPercentage, numSamples, graphGenerator).Result;

            FileManage.WriteGraphsAsync(graphList).Wait();
        }

        private static async Task<IList<Graph>> GenerateGraphSamplesAsync(float removeEdgesPercentage, int numSamples, GraphGenerator graphGenerator)
        {
            var graphList = new List<Graph>();
            var removeEdgesPercentages = new float[] { .05f, .40f, .70f };



            using (var cts = new CancellationTokenSource())
                for (int j = 5; j < 8; j++)
                {
                    foreach (var percentage in removeEdgesPercentages)
                    {
                        var verticesCount = 10 * Convert.ToInt32(Math.Pow(2, j));
                        Console.WriteLine($"*****  GENERATE {verticesCount} VERTICES {GetDensityName(percentage)} *****");
                        //var tasks = new List<Task>();
                        for (int i = 0; i < numSamples; i++)
                        {
                            (var edges, var verticesDegree) = graphGenerator.Gen(verticesCount);
                            await graphGenerator.RandomPruneAsync(edges, Convert.ToInt32(edges.Count * percentage), verticesDegree, cts.Token);
                            graphList.Add(new Graph { Edges = edges.ToList(), VerticesCount = verticesCount, Id = $"{(i + 1)}_{GetDensityName(percentage)}" });
                            
                        }
                        //await Task.WhenAll(tasks);
                    }
                }
            return graphList;
        }

        private static string GetDensityName(float percentage)
        {
            return percentage <= 0.1 ? "dense" : percentage <= 0.6 ? "normal" : "sparce";
        }

        class FileManage
        {
            public static async Task WriteFileAsync(string filename, IList<Edge> edges)
            {
                using (var sw = new StreamWriter(filename))
                {
                    foreach (var item in edges)
                    {
                        await sw.WriteLineAsync($"{item.V1},{item.V2},{item.Weight}");
                    }
                }
            }

            public static Task WriteGraphsAsync(IList<Graph> graphsList)
            {
                var tasks = new List<Task>();
                foreach (var graph in graphsList)
                {
                    tasks.Add(FileManage.WriteFileAsync($"graph_v{graph.VerticesCount}_exemplo{graph.Id}_a{graph.Edges.Count}", graph.Edges));
                }
                return Task.WhenAll(tasks);

            }


        }

    }

    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

    public static class Extensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
