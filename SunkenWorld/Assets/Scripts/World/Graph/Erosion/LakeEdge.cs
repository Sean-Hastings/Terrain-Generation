using System.Collections.Generic;
using System;

public class LakeEdge
{
    protected HashSet<ErosionNode> nodes;
    protected ErosionNode receiver;
    protected float height;

    public LakeEdge(ErosionNode a, ErosionNode b)
    {
        nodes = new HashSet<ErosionNode>() {a, b};
        receiver = null;
        height = Math.Max(a.Height, b.Height);
    }

    public HashSet<ErosionNode> Nodes
    {
        get
        {
            return nodes;
        }

        set {}
    }

    public ErosionNode Source
    {
        get
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

        set {}
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
