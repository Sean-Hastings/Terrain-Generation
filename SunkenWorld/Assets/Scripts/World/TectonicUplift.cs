//using UnityEngine;

public class TectonicUplift
{
    private int size;
    private float[,] map;

    public TectonicUplift(int size, float scale)
    {
        this.size = size;
        map = GenerateRandomMap(scale);
    }

    public float get(int row, int col)
    {
        return map[row, col];
    }

    public float[,] GenerateRandomMap(float scale)
    {
        float[,] map = new float[size, size];

        FastNoise fn = new FastNoise();
        
        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                map[r, c] = fn.GetSimplexFractal(r / scale, c / scale);
            }
        }

        return map;
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
