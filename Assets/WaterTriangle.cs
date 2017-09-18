using UnityEngine;

public class WaterTriangle
{
    public Vector3[] Vertices;
    public Vector3 Center;
    public Vector3 Normal;
    public Vector3 Velocity;
    public Vector3 NormalizedVelocity;
    public int OriginalMeshIndex;
    public float Area;
    public float CosTheta; // Angle between the normal and the velocity
    public float DepthAtCenter;
}