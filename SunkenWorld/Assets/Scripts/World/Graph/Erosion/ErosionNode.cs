using System.Collections.Generic;

public class ErosionNode : GraphNode
{
    protected float height;
    protected float uplift;
    protected float drainage;
    protected List<ErosionNode> inflow;
    protected ErosionNode outflow;
    protected LakeNode lake;

    public ErosionNode(float x, float y, float height, float uplift) : base(x, y)
    {
        this.height = height;
        this.uplift = uplift;
        inflow = new List<ErosionNode>();
    }

    public bool IsLeaf()
    {
        return inflow.Count == 0;
    }

    public override GraphEdge AddEdgeFromNode(GraphNode node)
    {
        if (!GetNeighbors().Contains(node))
        {
            ErosionEdge edge = new ErosionEdge(this, (ErosionNode) node);
            edges.Add(edge);
            edges.Sort((a,b) => a.Length.CompareTo(b.Length));
            return edge;
        }
        else
        {
            foreach (GraphEdge edge in edges)
            {
                if ((ErosionNode) edge.GetOtherNode(this) == node)
                {
                    return edge;
                }
            }
        }
        return null;
    }

    public float Height
    {
        get
        {
            return height;
        }

        set
        {
            height = value;
        }
    }

    public float Uplift
    {
        get
        {
            return uplift;
        }

        set
        {
            uplift = value;
        }
    }

    public LakeNode Lake
    {
        get
        {
            return lake;
        }

        set
        {
            lake = value;
        }
    }

    public ErosionNode Outflow
    {
        get
        {
            return outflow;
        }

        set
        {
            outflow = value;
        }
    }

    public void ClearInflows()
    {
        inflow = new List<ErosionNode>();
    }

    public List<ErosionNode> Inflows()
    {
        return inflow;
    }

    public void AddInflow(ErosionNode a)
    {
        inflow.Add(a);
    }
}
