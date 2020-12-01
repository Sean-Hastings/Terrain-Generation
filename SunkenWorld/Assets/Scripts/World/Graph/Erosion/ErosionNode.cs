using System.Collections.Generic;
using System;
using UnityEngine;

public class ErosionNode : GraphNode
{
    protected float height;
    protected float uplift;
    protected float drainage;
    protected float k;
    protected float m;
    protected List<ErosionNode> inflow;
    protected ErosionNode outflow;
    protected LakeNode lake;

    public ErosionNode(float x, float y, float height, float uplift, float k, float m) : base(x, y)
    {
        this.height = height;
        this.uplift = uplift;
        this.k = k;
        this.m = m;
        inflow = new List<ErosionNode>();
        drainage = 0;
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

    public float Drainage
    {
        get
        {
            return drainage;
        }

        set
        {
            drainage = value;
        }
    }

    public float Distance
    {
        get
        {
            if (outflow != null)
            {
                float xDist = (float) (x - outflow.X);
                float yDist = (float) (y - outflow.Y);
                return (float) Math.Sqrt(xDist * xDist + yDist * yDist);
            }
            else
            {
                return 0;
            }
        }

        set{}
    }

    public float Slope
    {
        get
        {
            if (outflow != null)
            {
                return (height - outflow.Height) / Distance;
            }
            else
            {
                return 0;
            }
        }

        set {}
    }

    public float K
    {
        get
        {
            return k;
        }

        set
        {
            k = value;
        }
    }

    public float M
    {
        get
        {
            return m;
        }

        set
        {
            m = value;
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
