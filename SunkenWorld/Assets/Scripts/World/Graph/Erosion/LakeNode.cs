using System.Collections.Generic;

public class LakeNode
{
    public ErosionNode origin;
    public List<ErosionNode> nodes;
    public bool oceanic;

    public LakeNode(ErosionNode origin)
    {
        this.origin = origin;
        oceanic = origin.IsExterior();
        nodes = new List<ErosionNode>();
    }
}
