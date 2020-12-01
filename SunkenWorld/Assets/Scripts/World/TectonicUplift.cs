//using UnityEngine;

public class TectonicUplift
{
    private int size;
    private float scale;
    private int seed;
    private FastNoise fn;

    public TectonicUplift(int size, float scale, int seed)
    {
        this.seed = seed;
        this.size = size;
        this.scale = scale;
        fn = new FastNoise(seed);
    }

    public float get(float x, float y)
    {
        return fn.GetSimplexFractal(x / scale, y / scale);
    }

    /*

    public void Draw(Transform transform)
    {
        Gizmos.color = new Color(1, 0, 0, 0.5F);
        Vector3 draw_loc = transform.position;
        for (int r = 0; r < size; r++)
        {
            draw_loc.z += .5f;
            draw_loc.x = transform.position.x;
            for (int c = 0; c < size; c++)
            {
                draw_loc.x += .5f;
                draw_loc.y = get(r, c) * 10;
                Gizmos.DrawCube(draw_loc, new Vector3(.2f, .2f, .2f));
            }
        }
    }
    */
}
