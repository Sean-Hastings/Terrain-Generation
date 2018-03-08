public class ErosionNode : GraphNode
{
    protected float height;

    public ErosionNode(float x, float y, float height) : base(x, y)
    {
        this.height = height;
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
