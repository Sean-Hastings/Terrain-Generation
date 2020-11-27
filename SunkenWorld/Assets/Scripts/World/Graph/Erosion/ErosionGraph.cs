using System.Collections.Generic;
using System.Linq;
using System;
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

        Erode();
    }

    protected void CreateNodes()
    {
        nodes = new List<ErosionNode>(node_count);

        System.Random rand = new System.Random();
        FastNoise fn = new FastNoise();

        for (int i = 0; i < node_count; i++)
        {
            float x = (float) rand.NextDouble() * terrain_size;
            float y = (float) rand.NextDouble() * terrain_size;
            nodes.Add(new ErosionNode(x, y, 0, uplift_map.get(x, y)));
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

    public void Erode()
    {
        ClearWatershed();
        List<ErosionNode> origins = CalculateStreams();
        List<LakeNode> lakes = CalculateLakes(origins);
        CalculatePasses(lakes);
        // CalculateDrainage();
        // calculate change
    }

    public void CalculateDrainage()
    {

    }

    public void CalculatePasses(List<LakeNode> lakes)
    {
        List<LakeEdge> passes = BuildPasses(lakes);
        passes = IteratePasses(lakes, passes);

        foreach (LakeEdge pass in passes)
        {
            pass.Receiver.AddInflow(pass.Source().Lake.origin);
            pass.Source().Lake.origin.Outflow = pass.Receiver;
        }
    }

    public List<LakeEdge> IteratePasses(List<LakeNode> lakes, List<LakeEdge> passes)
    {
        List<LakeEdge> candidates = new List<LakeEdge>();
        List<LakeEdge> chosen_passes = new List<LakeEdge>();
        HashSet<LakeEdge> to_remove = new HashSet<LakeEdge>();

        foreach (LakeEdge pass in passes)
        {
            int count = 0;
            foreach (ErosionNode node in pass.nodes)
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
            else if (count == 2 && pass.nodes[0].Lake == pass.nodes[1].Lake)
            {

                candidates.Add(pass);
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
                foreach (ErosionNode node in c_pass.nodes)
                {
                    if (pass.nodes.Any(t => t.Lake == node.Lake))
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
                if (c_pass.Source().Lake == pass.Source().Lake)
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

    public List<LakeEdge> BuildPasses(List<LakeNode> lakes)
    {
        List<LakeEdge> passes = new List<LakeEdge>();
        List<LakeNode> seen_lakes = new List<LakeNode>();
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
        return passes;
    }

    public List<LakeNode> CalculateLakes(List<ErosionNode> origins)
    {
        List<LakeNode> lakes = new List<LakeNode>();
        foreach (ErosionNode origin in origins)
        {
            lakes.Add(new LakeNode(origin));
        }
        return lakes;
    }

    public void ClearWatershed()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].ClearInflows();
            nodes[i].Lake = null;
        }
    }

    public List<ErosionNode> CalculateStreams()
    {
        List<ErosionNode> lakes = new List<ErosionNode>();
        foreach (ErosionNode node in nodes)
        {
            ErosionNode lowest = node;
            foreach (GraphNode neighbor in node.GetNeighbors())
            {
                if (((ErosionNode) neighbor).Height < lowest.Height)
                {
                    lowest = (ErosionNode) neighbor;
                }
            }
            if (node == lowest || node.IsExterior())
            {
                node.Outflow = null;
                lakes.Add(node);
            }
            else
            {
                node.Outflow = lowest;
                lowest.AddInflow(node);
            }
        }
        return lakes;
    }

    public void Draw()
    {
        Gizmos.color = new Color(1, 1, 1, 1F);
        foreach (ErosionNode node in nodes)
        {
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
