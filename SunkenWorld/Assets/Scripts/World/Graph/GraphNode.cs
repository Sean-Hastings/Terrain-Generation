using System.Collections.Generic;
using System.Linq;

public class GraphNode
{
    protected float x;
    protected float y;
    protected float voronoi;

    protected bool exterior;
    protected bool calcd_e;

    protected List<GraphEdge> edges;

    public GraphNode() : this(0, 0) {}

    public GraphNode(float x, float y)
    {
        this.x = x;
        this.y = y;
        voronoi = 0;
        edges = new List<GraphEdge>();
        calcd_e = false;
    }

    public GraphNode Nearest()
    {
        return Nearest(0);
    }

    public GraphNode Nearest(int index)
    {
        if (index < edges.Count)
        {
            return edges[index].GetOtherNode(this);
        }
        else
        {
            return null;
        }
    }

    public void AddEdge(GraphEdge edge)
    {
        if (!edges.Contains(edge))
        {
            edges.Add(edge);
            edges.Sort((a,b) => a.Length.CompareTo(b.Length));
        }
    }

    public virtual GraphEdge AddEdgeFromNode(GraphNode node)
    {
        if (!GetNeighbors().Contains(node))
        {
            GraphEdge edge = new GraphEdge(this, node);
            edges.Add(edge);
            edges.Sort((a,b) => a.Length.CompareTo(b.Length));
            return edge;
        }
        else
        {
            foreach (GraphEdge edge in edges)
            {
                if (edge.GetOtherNode(this) == node)
                {
                    return edge;
                }
            }
        }
        return null;
    }

    public void RemoveEdge(GraphEdge edge)
    {
        if (edges.Contains(edge))
        {
            edges.Remove(edge);
        }
    }

    public List<GraphEdge> GetEdges()
    {
        return edges;
    }

    public List<GraphNode> GetNeighbors()
    {
        return (List<GraphNode>) (from edge in edges select edge.GetOtherNode(this)).ToList();
    }


    public bool IsExterior()
    {
        if (!calcd_e)
        {
            exterior = CalcExterior();
            calcd_e = true;
        }
        return exterior;
    }

    public bool CalcExterior()
    {
        foreach (GraphNode node in GetNeighbors())
        {
            int count = 0;
            foreach (GraphNode other in node.GetNeighbors())
            {
                if (GetNeighbors().Any(i => i == other))
                {
                    count++;
                }
            }
            if (count < 2)
            {
                return true;
            }
            else if (count > 2)
            {
                // This happens a lot but I don't think it should?
            }
        }
        return false;
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

    public float Voronoi
    {
        get
        {
            return voronoi;
        }

        set
        {
            voronoi = value;
        }
    }
}
