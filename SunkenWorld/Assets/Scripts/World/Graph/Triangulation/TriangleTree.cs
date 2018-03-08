using System.Collections.Generic;
using System.Diagnostics;

public class TriangleTree {

    protected Triangle tri;
    protected List<TriangleTree> children;
    protected bool listed;

    public TriangleTree(Triangle tri)
    {
        this.tri = tri;
        if (!tri.IsCClockwise())
        {
            tri.ReverseDirection();
        }

        children = new List<TriangleTree>();
        listed = false;
    }

    public bool IsInTriangle(GraphNode node)
    {
        return tri.IsInTriangle(node);
    }

    public bool IsInCircle(GraphNode node)
    {
        return tri.IsInCircle(node);
    }

    public List<TriangleTree> SubDivide(GraphNode node, GraphNode[] dummies)
    {
        Debug.Assert(IsLeaf());

        List<Triangle> child_tris = tri.SubDivide(node, dummies);

        foreach (Triangle child_tri in child_tris)
        {
            children.Add(new TriangleTree(child_tri));
        }

        return children;
    }

    public GraphNode GetAdjacentDummy(GraphNode node)
    {
        return tri.GetOppositeByScale(node, .001f);           // MAGIC NUMBER ALERT
    }

    public GraphNode[] GetVertices()
    {
        return tri.GetVertices();
    }

    public List<TriangleTree> GetChildren()
    {
        return children;
    }

    public List<Triangle> AsList()
    {
        List<Triangle> tri_list = new List<Triangle>();
        if (IsLeaf())
        {
            tri_list.Add(tri);
        }
        listed = true;

        foreach (TriangleTree child in children)
        {
            if (!child.IsListed())
            {
                tri_list.AddRange(child.AsList());
            }
        }

        return tri_list;
    }

    public bool IsListed()
    {
        return listed;
    }

    public bool IsLeaf()
    {
        return children.Count == 0;
    }
}
