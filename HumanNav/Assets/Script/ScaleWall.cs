using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWall : MonoBehaviour
{
    private Material mat;
    public GameObject post; 
    public GameObject wallEnd;
    private float floorHeight;
    public bool rescaled; 

    private void Awake()
    {
        mat = GetComponent<Renderer>().material;
        GameObject floor = GameObject.FindGameObjectWithTag("Floor");
        floorHeight = floor.transform.position.y + (floor.transform.localScale.y / 2);
    }

    private void Update()
    {
        if (post && wallEnd)
        {
            FollowWallEnd();
            ScaleMaterial();
        }
    }

    private void FollowWallEnd()
    {
        Vector2 distanceVector = (new Vector2(wallEnd.transform.position.x, wallEnd.transform.position.z) - new Vector2(post.transform.position.x, post.transform.position.z));
        float distance = Vector2.Distance(new Vector2(post.transform.position.x, post.transform.position.z), new Vector2(wallEnd.transform.position.x, wallEnd.transform.position.z));
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, distance);
        transform.position = post.transform.position + (wallEnd.transform.position - post.transform.position) / 2 + new Vector3(0, transform.localScale.y / 4, 0);
        transform.position = new Vector3(transform.position.x, floorHeight + transform.localScale.y/2, transform.position.z); 

        float angle = 0; 
        if (distanceVector.y == 0)
        {
            angle = 0; 
        }
        else
        {
           float oppadj = distanceVector.x / distanceVector.y;
            angle = Mathf.Rad2Deg * Mathf.Atan(oppadj); 
        }

        transform.rotation = Quaternion.Euler(0,angle,0);
        if (rescaled)
        {
            {
                this.enabled = false;
                wallEnd.transform.parent = transform; 
            }
        }
    }

    private void ScaleMaterial()
    {
        float scaleX = transform.localScale.z / 2;
        float scaleY = transform.localScale.y / 2;
        mat.mainTextureScale = new Vector2(scaleX, scaleY);
    }    
}
