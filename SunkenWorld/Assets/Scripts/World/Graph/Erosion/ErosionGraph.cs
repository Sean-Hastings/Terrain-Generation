using System.Collections.Generic;
using UnityEngine;

public class ErosionGraph
{
    protected int node_count;
    protected TectonicUplift uplift_map;
    protected int terrain_size;

    private List<ErosionNode> nodes;

    public ErosionGraph(TectonicUplift uplift_map, int node_count, int terrain_size)
    {
        this.node_count = node_count;
        this.uplift_map = uplift_map;
        this.terrain_size = terrain_size;

        CreateNodes();

        Triangulate();
    }

    protected void CreateNodes()
    {
        nodes = new List<ErosionNode>(node_count);

        System.Random rand = new System.Random();

        for (int i = 0; i < node_count; i++)
        {
            float x = (float) rand.NextDouble() * terrain_size;
            float y = (float) rand.NextDouble() * terrain_size;
            nodes.Add(new ErosionNode(x, y, 0));
        }
    }
    
    protected void Triangulate()
    {
        // Delauney here (Algorithm described in paper, section 4)
        // http://page.mi.fu-berlin.de/faniry/files/faniry_aims.pdf

        
        Trianglulation tri = new Trianglulation(nodes.ToArray());
        tri.Triangulate(0, terrain_size, 0, terrain_size);
    }
    
    public void Draw()
    {
        Gizmos.color = new Color(1, 1, 1, 1F);
        for (int i = 0; i < nodes.Count; i++)
        {
            ErosionNode node = nodes[i];
            Gizmos.DrawCube(new Vector3(node.X, node.Y, node.Height), Vector3.one);

            foreach (ErosionNode edge in node.GetEdges())
            {
                Gizmos.DrawLine(new Vector3(node.X, node.Y, node.Height), new Vector3(edge.X, edge.Y, edge.Height));
            }
        }
    }
}