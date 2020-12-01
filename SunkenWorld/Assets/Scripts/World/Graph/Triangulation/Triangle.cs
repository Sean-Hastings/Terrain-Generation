using System.Collections.Generic;
using System.Diagnostics;
using System;

public class Triangle
{
    protected GraphNode[] vertices;

    public Triangle(GraphNode a, GraphNode b, GraphNode c)
    {
        vertices = new GraphNode[] {a, b, c};
    }

    public float Area()
    {
        return Math.Abs(vertices[0].X * (vertices[1].Y - vertices[2].Y) +
               vertices[1].X * (vertices[2].Y - vertices[0].Y) +
               vertices[2].X * (vertices[0].Y - vertices[1].Y)) / 2;
    }

    // Determinant unwrapped:
    // Ax, Ay, 1
    // Bx, By, 1
    // Cx, Cy, 1
    public bool IsCClockwise()
    {
        return (vertices[1].X - vertices[0].X) * (vertices[2].Y - vertices[0].Y) -
               (vertices[2].X - vertices[0].X) * (vertices[1].Y - vertices[0].Y) > 0;
    }

    public void ReverseDirection()
    {
        GraphNode a = vertices[0];
        vertices[0] = vertices[2];
        vertices[2] = a;
    }

    // This is some sketchy unwrapped determinant math from here: http://mathworld.wolfram.com/TriangleInterior.html
    // I just unwrapped the determinants, moved d(v1,v2) to the comparison, and didn't negate b until necessary
    public bool IsInTriangle(GraphNode node)
    {
        float[] vector_1 = new float[] {vertices[1].X - vertices[0].X, vertices[1].Y - vertices[0].Y};
        float[] vector_2 = new float[] {vertices[2].X - vertices[0].X, vertices[2].Y - vertices[0].Y};

        float a = node.X * vector_2[1] - node.Y * vector_2[0] - vertices[0].X * vector_2[1] + vertices[0].Y * vector_2[0];
        float b = node.X * vector_1[1] - node.Y * vector_1[0] - vertices[0].X * vector_1[1] + vertices[0].Y * vector_1[0];

        return 0 < a && 0 > b && a - b < vector_1[0] * vector_2[1] - vector_1[1] * vector_2[0];
    }

    // More nasty determinant math, this time unwrapping this determinant:
    // Ax, Ay, Ax*Ax+Ay*Ay, 1
    // Bx, ...
    // ...
    // ...
    public bool IsInCircle(GraphNode node)
    {
        float c = vertices[0].X * vertices[0].X + vertices[0].Y * vertices[0].Y;
        float g = vertices[1].X * vertices[1].X + vertices[1].Y * vertices[1].Y;
        float k = vertices[2].X * vertices[2].X + vertices[2].Y * vertices[2].Y;
        float o = node.X * node.X + node.Y * node.Y;

        float kp_ol = k - o;
        float jp_nl = vertices[2].Y - node.Y;
        float jo_nk = vertices[2].Y * o - node.Y * k;
        float ip_lm = vertices[2].X - node.X;
        float io_km = vertices[2].X * o - node.X * k;
        float in_jm = vertices[2].X * node.Y - vertices[2].Y * node.X;

        float a_det = vertices[0].X * (vertices[1].Y * kp_ol - g * jp_nl + jo_nk);
        float b_det = vertices[0].Y * (vertices[1].X * kp_ol - g * ip_lm + io_km);
        float c_det = c * (vertices[1].X * jp_nl - vertices[1].Y * ip_lm + in_jm);
        float d_det = vertices[1].X * jo_nk - vertices[1].Y * io_km + g * in_jm;

        float determinant = a_det - b_det + c_det - d_det;

        return determinant > 0;
    }

    // Keeping so I can easily switch back to this to do voronoi smoothing later
    public List<Triangle> SubDivide(GraphNode node)
    {
        List<Triangle> sub_tris = new List<Triangle>();

        sub_tris.Add(new Triangle(vertices[0], vertices[1], node));
        sub_tris.Add(new Triangle(vertices[1], vertices[2], node));
        sub_tris.Add(new Triangle(vertices[2], vertices[0], node));

        return sub_tris;
    }

    public List<Triangle> SubDivide(GraphNode node, GraphNode[] dummies)
    {
        foreach (GraphNode vert in vertices)
        {
            if (Array.IndexOf(dummies, vert) > -1)
            {
                return SubDivide(node);
            }
        }

        node.X = (vertices[0].X + vertices[1].X + vertices[2].X) / 3;
        node.Y = (vertices[0].Y + vertices[1].Y + vertices[2].Y) / 3;

        return SubDivide(node);
    }

    public GraphNode GetOppositeByScale(GraphNode node, float scale)
    {
       List<GraphNode> other_vertices = new List<GraphNode>();

        foreach (GraphNode vert in vertices)
        {
            if (!(vert.X == node.X && vert.Y == node.Y))
            {
                other_vertices.Add(vert);
            }
        }

        Debug.Assert(other_vertices.Count == 2);

        float new_x = node.X + (other_vertices[0].X + other_vertices[1].X - 2 * node.X) / (2 - scale);
        float new_y = node.Y + (other_vertices[0].Y + other_vertices[1].Y - 2 * node.Y) / (2 - scale);

        return new GraphNode(new_x, new_y);
    }

    public GraphNode[] GetVertices()
    {
        return vertices;
    }
}
