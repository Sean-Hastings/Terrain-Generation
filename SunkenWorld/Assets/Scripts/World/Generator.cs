using UnityEngine;
using System;

public class Generator : MonoBehaviour {

    public int size;
    public int num_sampling_points;
    public double edge_filter_percent;
    public int time_scale;
    public int old_time_scale;

    protected TectonicUplift tectonic_uplift_map;
    protected ErosionGraph graph;


    // https://hal.inria.fr/hal-01262376/document

    void Start()
    {
        tectonic_uplift_map = new TectonicUplift(size, 2, 1);
        graph = new ErosionGraph(tectonic_uplift_map, num_sampling_points, size, edge_filter_percent);
    }

    void Update()
    {
        if (time_scale > 0)
        {
            graph.Erode((float) time_scale);
        }
    }

    private void OnDrawGizmos()
    {
        // tectonic_uplift_map.Draw(GetComponent<Transform>());
         graph.Draw();
    }

}
