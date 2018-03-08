using UnityEngine;

public class Generator : MonoBehaviour {

    public int size;
    public int num_sampling_points;
    public int time_scale;
    public int old_time_scale;

    protected TectonicUplift tectonic_uplift_map;
    protected ErosionGraph graph;


    // https://hal.inria.fr/hal-01262376/document

    void Start()
    {

    }

    void Update()
    {
        if (time_scale != old_time_scale)
        {
            old_time_scale = time_scale;
            
            tectonic_uplift_map = new TectonicUplift(size, 2);
            graph = new ErosionGraph(tectonic_uplift_map, num_sampling_points, size);
        }
    }

    private void OnDrawGizmos()
    {
        // tectonic_uplift_map.Draw(GetComponent<Transform>());
         graph.Draw();
    }
    
}
