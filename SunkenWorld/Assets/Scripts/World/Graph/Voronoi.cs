using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi : MonoBehaviour {

    public ComputeShader voronoi_shader;

	void Start ()
    {
        RunShader();
	}

    // http://www.emersonshaffer.com/blog/2016/5/11/unity3d-compute-shader-introduction-tutorial
    protected void RunShader()
    {
        int kernel_index = voronoi_shader.FindKernel("CSMain");

        RenderTexture tex = new RenderTexture(512, 512, 24);
        tex.enableRandomWrite = true;
        tex.Create();

        voronoi_shader.SetTexture(kernel_index, "Result", tex);
        voronoi_shader.Dispatch(kernel_index, 512 / 8, 512 / 8, 1);
    }
	
	void Update ()
    {
		
	}
}
