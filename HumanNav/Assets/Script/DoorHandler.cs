using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHandler : MonoBehaviour
{
    public GameObject wall1;
    public GameObject wall2;

    private void OnDestroy()
    {
        wall1.transform.position = wall1.GetComponent<ScaleWall>().post.transform.position; 
        wall1.GetComponent<ScaleWall>().enabled = false;
        wall2.GetComponent<ScaleWall>().post.transform.parent = null; 
        wall1.GetComponent<ScaleWall>().wallEnd = wall2.GetComponent<ScaleWall>().post;
        wall1.GetComponent<ScaleWall>().enabled = true;
        wall1.GetComponent<ScaleWall>().rescaled = true; 

        Destroy(wall2); 
    }
}

