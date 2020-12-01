using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;
using UnityEngine;

public class ErosionGraph
{
    protected int node_count;
    protected TectonicUplift uplift_map;
    protected int terrain_size;

    private List<ErosionNode> nodes;
    private List<ErosionEdge> edges;

    public ErosionGraph(TectonicUplift uplift_map, int node_count, int terrain_size, double edge_filter_percent)
    {
        this.node_count = node_count;
        this.uplift_map = uplift_map;
        this.terrain_size = terrain_size;

        CreateNodes();

        Triangulate();

        RemoveEdges(edge_filter_percent);

        //Erode(1);
    }

    protected void CreateNodes()
    {
        nodes = new List<ErosionNode>(node_count);

        System.Random rand = new System.Random();
        TectonicUplift fn = new TectonicUplift(2, 2, 2);
        TectonicUplift fn2 = new TectonicUplift(2, 2, 3);
        TectonicUplift fn3 = new TectonicUplift(2, 2, 4);

        for (int i = 0; i < node_count; i++)
        {
            float x = (float) rand.NextDouble() * terrain_size;
            float y = (float) rand.NextDouble() * terrain_size;
            nodes.Add(new ErosionNode(x, y, fn.get(x, y), (float) (5*Math.Pow(10,-4)), (float) (5.61*Math.Pow(10, -7)), 2.0f));
        }
    }

    protected void Triangulate()
    {
        // Delauney here (Algorithm described in paper, section 4)
        // http://page.mi.fu-berlin.de/faniry/files/faniry_aims.pdf


        Trianglulation tri = new Trianglulation(nodes.ToArray());
        edges = tri.Triangulate(0, terrain_size, 0, terrain_size).ConvertAll(x => (ErosionEdge)x);
    }

    protected void RemoveEdges(double percentage)
    {
        int num_remove = (int) (edges.Count * percentage);
        edges.Sort((a,b) => b.Length.CompareTo(a.Length));
        for (int i = 0; i < num_remove; i++)
        {
            List<GraphNode> nodes = edges[0].GetNodes();
            for (int j = 0; j < nodes.Count; j++)
            {
                nodes[j].RemoveEdge(edges[0]);
            }
            edges.Remove(edges[0]);
        }
    }

    public void Erode(float delta_t)
    {
        ClearWatershed();
        UnityEngine.Profiling.Profiler.BeginSample("streams");
        List<ErosionNode> origins = CalculateStreams();
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("lakes");
        CalculateLakes(origins);
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("passes");
        CalculatePasses();
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("heights");
        AdjustHeights(origins, delta_t);
        UnityEngine.Profiling.Profiler.EndSample();
    }

    public void AdjustHeights(List<ErosionNode> origins, float delta_t)
    {
        float max_slope = (float) Math.Tan(0.75); // max angle in radians

        Queue<ErosionNode> node_queue = new Queue<ErosionNode>();
        foreach (ErosionNode origin in origins)
        {
            node_queue.Enqueue(origin);
        }

        while (node_queue.Count > 0)
        {
            ErosionNode node = node_queue.Dequeue();
            foreach (ErosionNode inflow in node.Inflows())
            {
                node_queue.Enqueue(inflow);
            }
            if (node.Outflow != null)
            {
                float fluvial = (float) (node.K * Math.Pow(node.Drainage, node.M) / node.Distance);
                float uplift  = node.Height + delta_t * (node.Uplift + fluvial * node.Outflow.Height);
                uplift = uplift / (1f + delta_t * fluvial);
                node.Height = uplift;

                if (node.Slope > max_slope)
                {
                    node.Height = node.Outflow.Height + node.Distance * max_slope;
                }
                else if (node.Slope < (-1 * max_slope))
                {
                    node.Height = node.Outflow.Height - node.Distance * max_slope;
                }
            }
        }
    }

    public void CalculatePasses()
    {
        List<LakeEdge> passes = BuildPasses();
        passes = IteratePasses(passes);

        foreach (LakeEdge pass in passes)
        {
            pass.Receiver.AddInflow(pass.Source.Lake.origin);
            pass.Source.Lake.origin.Outflow = pass.Receiver;
        }
    }

    public List<LakeEdge> IteratePasses(List<LakeEdge> passes)
    {
        List<LakeEdge> candidates = new List<LakeEdge>();
        List<LakeEdge> chosen_passes = new List<LakeEdge>();
        HashSet<LakeEdge> to_remove = new HashSet<LakeEdge>();

        foreach (LakeEdge pass in passes)
        {
            int count = 0;
            foreach (ErosionNode node in pass.Nodes)
            {
                if (node.Lake.oceanic)
                {
                    count++;
                    pass.Receiver = node;
                    to_remove.Add(pass);
                }
            }
            if (count == 1)
            {
                candidates.Add(pass);
            }
            else if (count >= 2)
            {
                //Debug.Log(count);
                //candidates.Add(pass);
            }
        }
        foreach (LakeEdge pass in to_remove)
        {
            passes.Remove(pass);
        }

        while (candidates.Count > 0)
        {
            candidates.Sort((a,b) => a.Height.CompareTo(b.Height));

            LakeEdge pass = candidates[0];
            candidates.Remove(pass);
            chosen_passes.Add(pass);

            to_remove = new HashSet<LakeEdge>();
            foreach (LakeEdge c_pass in passes)
            {
                foreach (ErosionNode node in c_pass.Nodes)
                {
                    if (pass.Nodes.Any(t => t.Lake == node.Lake))
                    {
                        c_pass.Receiver = node;
                        to_remove.Add(c_pass);
                        candidates.Add(c_pass);
                        break;
                    }
                }
            }
            foreach (LakeEdge c_pass in to_remove)
            {
                passes.Remove(c_pass);
            }

            to_remove = new HashSet<LakeEdge>();
            foreach (LakeEdge c_pass in candidates)
            {
                if (c_pass.Source.Lake == pass.Source.Lake)
                {
                    to_remove.Add(c_pass);
                }
            }
            foreach (LakeEdge c_pass in to_remove)
            {
                candidates.Remove(c_pass);
            }
        }
        return chosen_passes;
    }

    public List<LakeEdge> BuildPasses()//List<LakeNode> lakes)
    {
        Dictionary<HashSet<ErosionNode>, LakeEdge> lake_passes = new Dictionary<HashSet<ErosionNode>, LakeEdge>();

        foreach (ErosionEdge edge in edges)
        {
            if (((ErosionNode)edge.GetNodes()[0]).Lake != ((ErosionNode)edge.GetNodes()[1]).Lake)
            {
                LakeEdge ledge = new LakeEdge(((ErosionNode)edge.GetNodes()[0]), ((ErosionNode)edge.GetNodes()[1]));
                if (!lake_passes.ContainsKey(ledge.Nodes) || ledge.Height < lake_passes[ledge.Nodes].Height)
                {
                    lake_passes[ledge.Nodes] = ledge;
                }
            }
        }
        List<LakeEdge> passes = new List<LakeEdge>();
        passes.AddRange(lake_passes.Values);

        /*
        foreach (LakeNode lake in lakes)
        {
            seen_lakes.Add(lake);
            Dictionary<LakeNode,LakeEdge> lake_passes = new Dictionary<LakeNode,LakeEdge>();
            foreach (ErosionNode node in lake.nodes)
            {
                foreach (ErosionNode other in node.GetNeighbors())
                {
                    if (!seen_lakes.Any(l => l == other.Lake))
                    {
                        float height = Math.Max(node.Height, other.Height);
                        if (!lake_passes.ContainsKey(other.Lake) || height < lake_passes[other.Lake].Height)
                        {
                            lake_passes[other.Lake] = new LakeEdge(node, other);
                        }
                    }
                }
            }
            passes.AddRange(lake_passes.Values);
        }
        */
        return passes;
    }

    public void CalculateLakes(List<ErosionNode> origins)
    {
        nodes.Sort((a,b) => a.Height.CompareTo(b.Height));
        foreach (ErosionNode node in nodes)
        {
            if (node.Outflow == null)
            {
                node.Lake = new LakeNode(node);
            }
            else
            {
                node.Lake = node.Outflow.Lake;
            }
            node.Lake.nodes.Add(node);
        }
    }

    public void ClearWatershed()
    {
        foreach (ErosionNode node in nodes)
        {
            node.ClearInflows();
            node.Outflow = null;
            node.Lake = null;
            node.Drainage = node.Voronoi;
        }
    }

    public List<ErosionNode> CalculateStreams()
    {
        List<ErosionNode> lakes = new List<ErosionNode>();

        foreach (ErosionNode node in nodes)
        {
            node.Outflow = node;
            foreach (GraphNode neighbor in node.GetNeighbors())
            {
                if (((ErosionNode) neighbor).Height < node.Outflow.Height)
                {
                    node.Outflow = (ErosionNode) neighbor;
                }
            }
        }

        nodes.Sort((a,b) => b.Height.CompareTo(a.Height));
        foreach (ErosionNode node in nodes)
        {
            if (node == node.Outflow || node.IsExterior())
            {
                node.Outflow = null;
                lakes.Add(node);
            }
            else
            {
                node.Outflow.AddInflow(node);
            }

            foreach (ErosionNode inflow in node.Inflows())
            {
                node.Drainage += inflow.Drainage;
            }
        }
        return lakes;
    }

    public void Draw()
    {
        Gizmos.color = new Color(1, 1, 1, 1F);
        foreach (ErosionNode node in nodes)
        {
            /*
            if (node.Outflow == null)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 1f);
            }
            else if (node.Lake.oceanic)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 1f);
            }
            else
            {
                Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            Gizmos.DrawCube(new Vector3(node.X, node.Height, node.Y), Vector3.one);
            */
            foreach (GraphEdge edge in node.GetEdges())
            {
                ErosionNode other = (ErosionNode) edge.GetOtherNode(node);
                if (node.Outflow == other || other.Outflow == node)
                {
                    Gizmos.color = new Color(0f, 0f, 1f, 1f);
                }
                else
                {
                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                Gizmos.DrawLine(new Vector3(node.X, node.Height, node.Y), new Vector3(other.X, other.Height, other.Y));
            }
            if (node.Outflow != null && !node.GetEdges().Any(t => t.GetOtherNode(node) == node.Outflow))
            {
                ErosionNode other = node.Outflow;
                Gizmos.color = new Color(1f, 0f, 1f, 1f);
                Gizmos.DrawLine(new Vector3(node.X, node.Height, node.Y), new Vector3(other.X, other.Height, other.Y));
            }
        }
    }
}
