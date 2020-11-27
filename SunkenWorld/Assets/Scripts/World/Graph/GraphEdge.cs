using System.Collections.Generic;
using System;

public class GraphEdge
{
    protected double length;

    protected List<GraphNode> nodes;

    public GraphEdge(GraphNode a, GraphNode b)
    {
        nodes = new List<GraphNode>();
        nodes.Add(a);
        nodes.Add(b);
        float xDist = (a.X - b.X);
        float yDist = (a.Y - b.Y);
        length = Math.Sqrt(xDist * xDist + yDist * yDist);
    }

    public List<GraphNode> GetNodes()
    {
        return nodes;
    }

    public GraphNode GetOtherNode(GraphNode a)
    {
        if (a == nodes[0])
        {
            return nodes[1];
        }
        else if (a == nodes[1])
        {
            return nodes[0];
        }
        else
        {
            return null;
        }
    }

    public double Length
    {
        get
        {
            return length;
        }

        set
        {
            length = value;
        }
    }

    public bool Equals(GraphEdge other)
    {
        return other.GetOtherNode(nodes[0]) == nodes[1];
    }
}
