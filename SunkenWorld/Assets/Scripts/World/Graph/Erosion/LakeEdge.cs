using System.Collections.Generic;
using System;

public class LakeEdge
{
    public List<ErosionNode> nodes;
    protected ErosionNode receiver;
    protected float height;

    public LakeEdge(ErosionNode a, ErosionNode b)
    {
        nodes = new List<ErosionNode>() {a, b};
        receiver = null;
        height = Math.Max(a.Height, b.Height);
    }

    public ErosionNode Source()
    {
        foreach (ErosionNode node in nodes)
        {
            if (node != receiver)
            {
                return node;
            }
        }
        return null;
    }

    public ErosionNode Receiver
    {
        get
        {
            return receiver;
        }

        set
        {
            receiver = value;
        }
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
}
