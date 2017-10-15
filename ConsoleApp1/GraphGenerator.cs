using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphGenerator
{
    public class GraphGenerator
    {

        public (IList<Edge>, int[]) Gen(int verticesCount)
        {
            IList<Edge> edges = new List<Edge>();
            int[] verticesDegree = new int[verticesCount];

            for (int i = 0; i < verticesDegree.Length; i++)
            {
                verticesDegree[i] = 0;
            }
            for (int i = 0; i < verticesCount - 1; i++)
            {
                for (int j = i + 1; j < verticesCount; j++)
                {
                    verticesDegree[i]++;
                    verticesDegree[j]++;
                    var weight = ThreadSafeRandom.ThisThreadsRandom.Next(1, 100);
                    edges.Add(new Edge
                    {
                        V1 = i,
                        V2 = j,
                        Weight = weight
                    });
                }
            }
            return (edges, verticesDegree);
        }

        public Task RandomPruneAsync(IList<Edge> edges, int removeEdgesCount, int[] verticesDegree, CancellationToken token)
        {
            edges.Shuffle();
            return Task.Run(() =>
            {
                var sw = new Stopwatch(); sw.Start();
                var maxrun = edges.Count;
                while (removeEdgesCount > 0 && !token.IsCancellationRequested && maxrun > 0)
                {
                    maxrun--;
                    var edge = edges.First();
                    var couldRemoveV1 = (verticesDegree[edge.V1] - 1) > 0;
                    var couldRemoveV2 = (verticesDegree[edge.V2] - 1) > 0;
                    if (couldRemoveV1 && couldRemoveV2)
                    {
                        removeEdgesCount--;
                        verticesDegree[edge.V1]--;
                        verticesDegree[edge.V2]--;
                        edges.RemoveAt(0);
                    }
                }
                sw.Stop();
                Console.WriteLine($"      SAMPLE takes {sw.ElapsedMilliseconds / 1000f} seconds to prune");
            });
        }

    }

    public class Edge
    {
        public int V1 { get; set; }
        public int V2 { get; set; }
        public int Weight { get; set; }
    }

    public class Graph
    {
        public int VerticesCount { get; set; }
        public List<Edge> Edges { get; set; }
        public string Id { get; set; }
    }

}
