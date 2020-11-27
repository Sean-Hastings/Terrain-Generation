using System.Collections.Generic;

public class LakeNode
{
    public ErosionNode origin;
    public List<ErosionNode> nodes;
    public bool oceanic;

    public LakeNode(ErosionNode origin)
    {
        this.origin = origin;
        oceanic = false;
        nodes = new List<ErosionNode>();
        FillNodes();
    }

    protected void FillNodes()
    {
        Queue<ErosionNode> frontier = new Queue<ErosionNode>();
        frontier.Enqueue(origin);
        while (frontier.Count > 0)
        {
            ErosionNode node = frontier.Dequeue();
            if (node.IsExterior())
            {
                oceanic = true;
            }
            nodes.Add(node);
            node.Lake = this;
            foreach (ErosionNode source in node.Inflows())
            {
                frontier.Enqueue(source);
            }
        }
    }
}
