using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class EditMesh : MonoBehaviour
{
    public List<Transform> points;
    private List<Vector3> vector3s;
    private PolyShape shape;
    private NavMeshSurface surf;
    private Vector4 boundingBox;
    private Vector2 boxCenter;
    private MeshCollider col;

    private void Awake()
    {
        shape = GetComponent<PolyShape>();
        surf = GetComponent<NavMeshSurface>();
        col = GetComponent<MeshCollider>();
        points = new List<Transform>();
        vector3s = new List<Vector3>();
    }


    public bool AddPoint(Transform point)
    {
        bool nearby = false;

        foreach (Transform pos in points)
        {
            if (Vector3.Distance(point.position, pos.position) < 2)
            {
                nearby = true;
            }
        }

        if (nearby)
        { 
            return true; 
        }
        else 
        { 
            points.Add(point);
            return false; 
        }
    }

    public void SetMesh()
    {
        vector3s = new List<Vector3>();
        if (points.Count > 0)
        {
            foreach (Transform point in points)
            {
                vector3s.Add(point.position);
            }
        }
        shape.SetControlPoints(vector3s);
        shape.CreateShapeFromPolygon();
        shape.extrude = 0.2f;
    }

}
