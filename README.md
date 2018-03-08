# Terrain Generation
A project to implement terrain generation as described in "Large Scale Terrain Generation from Tectonic Uplift and
Fluvial Erosion." The basic idea is to iterate the effects of tectonic uplift and fluvial (water) erosion until they converge into a stable terrain map.

### So far I have implemented the following parts:

* Uplift map generation, currently based on perlin noise (or other 2D stable noise, as of writing this I do not have the code open...)
* Random graph initialization for waterflow simulation
    * Delaunay triangulation of said graph

### To be implemented

* Simulation of waterflow and corresponding erosion over graph
* Lake overflow algorithm

### Notes and things I'm keeping in mind

* Current graph generation enforces square shape, that's boring and should be overhauled
* Instead of generating uplift map an uplift value should be sampled from the perlin space for each graph point when it's created
* Current graphs are not uniform, sometimes nodes can be very close to each other
    * More ideal is random initialization, then voronoi smoothing to convergence
        * Slow, but probably doable with ComputeShaders (but I don't know HLSL at all so the cost of implementing this is extra high for that fact)
            * possibility: points -> delaunay -> voronoi -> new points

### Future extension ideas, after main paper implementation

* Use of 3D stable noise to simulate change in uplift over time
* Simulation of irregularity of ground composition
    * Different properties at different places, another potential use of 3D stable noise
    * Would allow for more natural/interesting/complex terrain formation
* More complex erosion
    * Non-fluvial sources
        * Likely weighted comparatively quite low
    * "rainfall" based on simulated weather system
        * eg. Mountains cause rain to drop, plains get passed over, wave sweeps in [direction] with [amount] of water and drops/picks up as it goes across terrain
