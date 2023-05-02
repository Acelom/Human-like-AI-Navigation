using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;


namespace DynamicNavMesh
{
    public class BakeNavMesh: MonoBehaviour
    {

        private List<NavMeshSurface> surfaces;

        private void Update()
        {
            surfaces = new List<NavMeshSurface>(FindObjectsOfType<NavMeshSurface>());
        }

        public void UpdateNavMesh()
        {
            foreach (NavMeshSurface surface in surfaces)
            {
                surface.BuildNavMesh(); 
            }
        }
    }
}