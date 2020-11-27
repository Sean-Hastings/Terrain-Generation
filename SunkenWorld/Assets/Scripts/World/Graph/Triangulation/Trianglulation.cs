using System.Collections.Generic;
using System;

public class Trianglulation
{
    List<GraphNode> nodes;
    TriangleTree tris;

    public Trianglulation(GraphNode[] nodes)
    {
        this.nodes = new List<GraphNode>(nodes.Length);
        this.nodes.AddRange(nodes);
    }

    public List<GraphEdge> Triangulate(float x_low_bound, float x_high_bound, float y_low_bound, float y_high_bound)
    {
        tris = InitTris(x_low_bound, x_high_bound, y_low_bound, y_high_bound);

        GraphNode[] dummy_verts = tris.GetVertices();
        foreach (GraphNode node in nodes)
        {
            AddNode(node, dummy_verts);
        }

        List<Triangle> tri_list = tris.AsList();

        List<GraphEdge> edges = new List<GraphEdge>();
        foreach (Triangle tri in tri_list)
        {
            if (!TriangleContains(tri, tris.GetVertices()))
            {
                GraphNode[] vertices = tri.GetVertices();

                GraphEdge edge = vertices[0].AddEdgeFromNode(vertices[1]);
                edges.Add(edge);
                vertices[1].AddEdge(edge);

                edge = vertices[0].AddEdgeFromNode(vertices[2]);
                edges.Add(edge);
                vertices[2].AddEdge(edge);

                edge = vertices[1].AddEdgeFromNode(vertices[2]);
                edges.Add(edge);
                vertices[2].AddEdge(edge);
            }
        }
        return edges;
    }

    protected bool TriangleContains(Triangle tri, GraphNode[] nodes)
    {
        foreach (GraphNode node in nodes)
        {
            if (Array.IndexOf(tri.GetVertices(), node) > -1)
            {
                return true;
            }
        }
        return false;
    }

    protected TriangleTree InitTris(float x_low_bound, float x_high_bound, float y_low_bound, float y_high_bound)
    {
        float width = x_high_bound - x_low_bound;
        float height = y_high_bound - y_low_bound;

        GraphNode top = new GraphNode(x_low_bound + width / 2, y_high_bound + height * 2);
        GraphNode left = new GraphNode(x_low_bound - width, y_low_bound - height / 10);
        GraphNode right = new GraphNode(x_high_bound + width, y_low_bound - height / 10);

        return new TriangleTree(new Triangle(top, left, right));
    }

    protected void AddNode(GraphNode node, GraphNode[] dummies)
    {
        TriangleTree containing_tri = GetContainingTriangle(node);

        ReworkTriangle(containing_tri, node, dummies);
    }

    protected TriangleTree GetContainingTriangle(GraphNode node)
    {
        TriangleTree tri_within = tris;
        bool outside_triangles = true;

        while (!tri_within.IsLeaf())
        {
            foreach (TriangleTree sub_tri in tri_within.GetChildren())
            {
                if (sub_tri.IsInTriangle(node))
                {
                    tri_within = sub_tri;
                    outside_triangles = false;
                    break;
                }
            }
            if (outside_triangles)
            {
                return null;
            }
            outside_triangles = true;
        }

        return tri_within;
    }

    protected void ReworkTriangle(TriangleTree containing_tri, GraphNode node, GraphNode[] dummies)
    {
        List<TriangleTree> new_tris = containing_tri.SubDivide(node, dummies);

        foreach (TriangleTree tri_tree in new_tris)
        {
            EnsureEdge(tri_tree, node);
        }
    }

    protected void EnsureEdge(TriangleTree triangle, GraphNode node)
    {
        TriangleTree adjacent = GetContainingTriangle(triangle.GetAdjacentDummy(node));

        if (adjacent != null && adjacent.IsLeaf() && adjacent.IsInCircle(node))
        {
            List<TriangleTree> new_tris = FlipEdge(triangle, adjacent);

            foreach (TriangleTree new_tri in new_tris)
            {
                EnsureEdge(new_tri, node);
            }
        }
    }

    protected List<TriangleTree> FlipEdge(TriangleTree tri, TriangleTree adj)
    {
        List<GraphNode> shared_verts = new List<GraphNode>();
        List<GraphNode> separate_verts = new List<GraphNode>();
        separate_verts.AddRange(tri.GetVertices());

        foreach (GraphNode vertex_a in adj.GetVertices())
        {
            bool separate = true;
            foreach (GraphNode vertex_b in separate_verts)
            {
                if (vertex_a.X == vertex_b.X && vertex_a.Y == vertex_b.Y)
                {
                    separate_verts.Remove(vertex_b);
                    shared_verts.Add(vertex_b);
                    separate = false;
                    break;
                }
            }
            if (separate)
            {
                separate_verts.Add(vertex_a);
            }
        }

        List<TriangleTree> new_tris = new List<TriangleTree>();
        new_tris.Add(new TriangleTree(new Triangle(separate_verts[0], separate_verts[1], shared_verts[0])));
        new_tris.Add(new TriangleTree(new Triangle(separate_verts[0], separate_verts[1], shared_verts[1])));

        tri.GetChildren().AddRange(new_tris);
        adj.GetChildren().AddRange(new_tris);

        return new_tris;
    }
}
