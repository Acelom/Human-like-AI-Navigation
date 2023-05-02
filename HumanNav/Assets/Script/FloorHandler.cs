using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation; 

public class FloorHandler : MonoBehaviour
{

    public NavMeshSurface surf; 
    public enum ROOMTYPE
    {
        spawnRoom, 
        staffRoom, 
        shoppingRoom, 
        storageRoom, 
        mixedRoom, 
        emptyRoom, 
    }
    public ROOMTYPE roomType = ROOMTYPE.emptyRoom; 
}
