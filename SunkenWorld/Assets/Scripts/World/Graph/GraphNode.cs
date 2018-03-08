using System.Collections.Generic;

public class GraphNode
{
    protected float x;
    protected float y;

    protected List<GraphNode> edges;

    public GraphNode() : this(0, 0) {}

    public GraphNode(float x, float y)
    {
        this.x = x;
        this.y = y;
        edges = new List<GraphNode>();
    }

    public GraphNode Nearest()
    {
        return Nearest(0);
    }

    public GraphNode Nearest(int index)
    {
        if (index < edges.Count)
        {
            return edges[index];
        }
        else
        {
            return null;
        }
    }

    public void AddEdge(GraphNode edge)
    {
        if (!edges.Contains(edge))
        {
            edges.Add(edge);
        }
    }

    public List<GraphNode> GetEdges()
    {
        return edges;
    }

    public float Y
    {
        get
        {
            return y;
        }

        set
        {
            y = value;
        }
    }

    public float X
    {
        get
        {
            return x;
        }

        set
        {
            x = value;
        }
    }
}
