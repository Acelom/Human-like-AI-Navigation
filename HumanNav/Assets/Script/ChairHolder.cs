using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChairHolder : MonoBehaviour
{
    [SerializeField] private Transform sitSpot;
    public bool spotFilled = false;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "SitSpot")
            {
                sitSpot = child; 
            }
        }
    }

    public Transform GetSitSpot()
    {
        return sitSpot; 
    }
}
