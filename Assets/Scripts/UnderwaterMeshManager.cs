using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnderwaterMeshManager 
{
    private readonly List<SlammingForce> _slammingForces;
    private readonly Transform _boatTrans;
    private readonly Vector3[] _boatVertices;
    private readonly int[] _boatTriangles;
    
    private float[] _boatVerticesDistanceToWater;
    
    public List<WaterTriangle> UnderwaterTriangles { get; private set; }
    
    public UnderwaterMeshManager(GameObject sourceBoat, List<SlammingForce> slammingForces)
    {
        _slammingForces = slammingForces;
        _boatTrans = sourceBoat.transform;
        _boatVertices = sourceBoat.GetComponent<MeshFilter>().mesh.vertices;
        _boatTriangles = sourceBoat.GetComponent<MeshFilter>().mesh.triangles;
        
        _boatVerticesDistanceToWater = new float[_boatVertices.Length];
        UnderwaterTriangles = new List<WaterTriangle>();
    }

    public void GenerateUnderwaterMesh()
    {
        UnderwaterTriangles.Clear();

        CalculateVerticeToWaterDistance();
        GenerateUnderwaterTriangles();
    }

    public void CreateDisplayMesh(Mesh mesh)
    {
        var meshVerticies = new Vector3[UnderwaterTriangles.Count * 3];
        var meshTriangles = new int[UnderwaterTriangles.Count * 3];

        for (var index = 0; index < UnderwaterTriangles.Count; index++)
        {
            var position = index * 3;
            var triangle = UnderwaterTriangles[index];

            for (var triVert = 0; triVert < 3; triVert++)
            {
                meshVerticies[position + triVert] = triangle.Vertices[triVert];
                meshTriangles[position + triVert] = position + triVert;
            }
		
            //Debug.DrawRay(_boatTrans.TransformPoint(triangle.Center), triangle.Normal * 1f, Color.white);
        }
        
        mesh.Clear();

        mesh.vertices = meshVerticies;
        mesh.triangles = meshTriangles;
        
        mesh.RecalculateBounds();
    }
    
    Matrix4x4 LookAtTransformMatrix( Vector3 vector )  
    {  
        return Matrix4x4.LookAt(new Vector3(), vector, Vector3.up);
    }

    private void CalculateVerticeToWaterDistance()
    {
        for (var index = 0; index < _boatVertices.Length; index++)
        {
            var boatVertex = _boatVertices[index];
            var globalPosition = _boatTrans.TransformPoint(boatVertex);
            _boatVerticesDistanceToWater[index] = globalPosition.y; // TODO update to use water mesh.
        }
    }

    private void GenerateUnderwaterTriangles()
    {   
        var comparer = new Vector3Comparer();
        
        for (var index = 0; index < _boatTriangles.Length; index += 3)
        {
            var originalMeshIndex = index / 3;
            var slammingForce = _slammingForces[originalMeshIndex];
            slammingForce.PreviousSubmergedArea = slammingForce.SubmergedArea;
            slammingForce.SubmergedArea = 0;
            
            var poly = new List<Vector3>();

            var v0 = _boatTriangles[index];
            var v1 = _boatTriangles[index + 1];
            var v2 = _boatTriangles[index + 2];
            
            poly = poly.Union(WaterIntersection(v0, v1), comparer).ToList();
            poly = poly.Union(WaterIntersection(v1, v2), comparer).ToList();
            poly = poly.Union(WaterIntersection(v2, v0), comparer).ToList();
            
            // if poly.length == 4 -> split poly into 2 tris
            switch (poly.Count)
            {
                case 4:
                    AnalyzeTriangle(originalMeshIndex, poly[0], poly[1], poly[3]);
                    AnalyzeTriangle(originalMeshIndex, poly[1], poly[2], poly[3]);
                    break;
                case 3:
                    AnalyzeTriangle(originalMeshIndex, poly[0], poly[1], poly[2]);
                    break;
                case 0:
                    break;
                default:
                    throw new InvalidProgramException("Wrong number of verticies in underwater poly.");
            }
        }
    }

    private IEnumerable<Vector3> WaterIntersection(int vertexA, int vertexB)
    {
        var heightA = _boatVerticesDistanceToWater[vertexA];
        var heightB = _boatVerticesDistanceToWater[vertexB];
        var result = new List<Vector3>();
        
        if (heightA > 0 && heightB > 0)
        {
            // both heights above water
            return result;
        }
        if (heightA < 0 && heightB < 0)
        {
            // both heights below water
            result.Add(_boatVertices[vertexA]);
            result.Add(_boatVertices[vertexB]);
            return result;
        }
        
        if (heightA > 0)
        {
            // v1 = -hb / (ha - hb)
            result.Add(GetPositionBetweenPoints(
                _boatVertices[vertexB], 
                _boatVertices[vertexA],
                GetIntersectionRatio(heightA, heightB)));
            // v2 = vb
            result.Add(_boatVertices[vertexB]);
        }
        else
        {
            // v1 = va
            result.Add(_boatVertices[vertexA]);
            // v2 = -ha / (hb - ha)
            result.Add(GetPositionBetweenPoints(
                _boatVertices[vertexA], 
                _boatVertices[vertexB],
                GetIntersectionRatio(heightB, heightA)));
        }

        return result;
    }

    private void AnalyzeTriangle(int originalMeshIndex, Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        var center = (pointA + pointB + pointC) / 3f;
        
        //Area of the triangle
        var a = Vector3.Distance(pointA, pointB);
        var c = Vector3.Distance(pointC, pointA);
        var area = a * c * Mathf.Sin(Vector3.Angle(pointB - pointA, pointC - pointA) * Mathf.Deg2Rad) / 2f;
        
        var triangle = new WaterTriangle
        {
            Vertices = new[] { pointA, pointB, pointC },
            Center = center,
            DepthAtCenter = _boatTrans.TransformPoint(center).y,
            Normal = Vector3.Cross(pointB - pointA, pointC - pointA).normalized,
            Area = area,
            OriginalMeshIndex = originalMeshIndex
        };

        UnderwaterTriangles.Add(triangle);

        _slammingForces[originalMeshIndex].SubmergedArea += triangle.Area;
    }

    private static float GetIntersectionRatio(float high, float low)
    {
        return -low / (high - low);
    }

    private static Vector3 GetPositionBetweenPoints(Vector3 pointA, Vector3 pointB, float distanceRatio)
    {
        var diff = pointB - pointA;
        var adjust = diff * distanceRatio;
        return pointA + adjust;
    }
}
