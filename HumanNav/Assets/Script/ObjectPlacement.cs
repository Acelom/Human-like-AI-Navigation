using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIInteractions;
using PlayerController;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Linq;
using DynamicNavMesh;
using MoneyMaster;
using NavManager;


public class ObjectPlacement : MonoBehaviour
{

    [SerializeField] private float rotationSpeed;
    private Inputs inputs;
    private UIInteraction interactions;
    private GameObject placementObject = null;
    private Transform prevWallEnd;
    private float floorHeight;
    private bool roofEnabled;
    private GameObject roof;
    private GameObject floor;
    private BakeNavMesh nav;
    private MoneyManager money;
    private NavAgentManager navManager;

    [SerializeField] private int wallPrice;
    [SerializeField] private int doorPrice;
    [SerializeField] private int shelfPrice;
    [SerializeField] private int registerPrice;
    [SerializeField] private int chairPrice;

    private void Awake()
    {
        roofEnabled = true;
        money = GameObject.FindObjectOfType<MoneyManager>();
        inputs = GetComponent<Inputs>();
        interactions = GameObject.FindGameObjectWithTag("GameController").GetComponent<UIInteraction>();
        GameObject floor = GameObject.FindGameObjectWithTag("Floor");
        floorHeight = floor.transform.position.y + (floor.transform.localScale.y / 2);
        nav = GameObject.FindGameObjectWithTag("GameController").GetComponent<BakeNavMesh>();
        navManager = GameObject.FindObjectOfType<NavAgentManager>(); 
    }

    private void DeleteObject()
    {
        if (interactions.objectSelector == UIInteraction.OBJECTSELECTOR.deletion && inputs.leftClick)
        {
           
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            LayerMask layerMask = 1 << LayerMask.NameToLayer("Deletion Ignore");
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerMask))
            {
                GameObject obj = null;
                if (hit.transform.parent)
                {
                    obj = hit.transform.parent.gameObject;
                }
                else
                {
                    obj = hit.transform.gameObject;
                }
                Destroy(obj);
            }
        }
    }

    private void Update()
    {
        ShelfUI();

        if (floor)
        {
            floor.transform.position = new Vector3(floor.transform.position.x, floorHeight - 1.6f, floor.transform.position.z);
        }

        if (roof)
        {
            roof.transform.position = new Vector3(roof.transform.position.x, floorHeight + 1.29f, roof.transform.position.z);
        }

        PickObject();
        DeleteObject();
        if (placementObject != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                switch (interactions.objectSelector)
                {
                    case UIInteraction.OBJECTSELECTOR.wall:
                        if (hit.collider.gameObject.tag == "Post")
                        {
                            objectFollowMouse(hit.transform.position + (Vector3.down * 1.5f));
                        }
                        else if (hit.collider.gameObject.tag == "Wall")
                        {
                            Vector3 pos = hit.point;
                            pos = pos - hit.normal.normalized * (hit.transform.localScale.x / 2);
                            objectFollowMouse(PutOnTheFloor(pos, placementObject.transform.localScale));
                        }
                        else
                        {
                            objectFollowMouse(PutOnTheFloor(hit.point, placementObject.transform.localScale));
                        }
                        break;

                    case UIInteraction.OBJECTSELECTOR.door:
                        objectFollowMouse(PutOnTheFloor(hit.point, placementObject.transform.localScale) + Vector3.down * 0.5f);
                        placementObject.transform.rotation = hit.transform.rotation;
                        placementObject.transform.Rotate(0, 90, 0);
                        break;
                    case UIInteraction.OBJECTSELECTOR.genericObject:
                        objectFollowMouse(PutOnTheFloor(hit.point, placementObject.transform.localScale) + Vector3.down * 0.5f);
                        placementObject.transform.Rotate(0, inputs.rotationChange * rotationSpeed * Time.deltaTime, 0);


                        break;
                }
                placeObject();
            }
            else
            {
                placementObject.transform.position += new Vector3(0, -100000000000000, 0);
            }


            if (placementObject)
            {
                if (interactions.objectSelector == UIInteraction.OBJECTSELECTOR.door)
                {
                    if (DoorValidPlace())
                    {
                        PaintObjectColour(Color.white, placementObject.gameObject);
                    }
                    else
                    {
                        PaintObjectColour(Color.red, placementObject.gameObject);
                    }
                }
                else
                {
                    if (IsValidToPlace())
                    {
                        PaintObjectColour(Color.white, placementObject.gameObject);
                    }
                    else
                    {
                        PaintObjectColour(Color.red, placementObject.gameObject);
                    }
                }
            }

        }
    }

    private void PaintObjectColour(Color colour, GameObject obj)
    {
        if (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
        }

        if (obj.TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
        {
            mesh.material.color = colour;
        }

        if (obj.transform.childCount > 0)
        {
            foreach (Transform child in obj.transform)
            {
                if (child.TryGetComponent<MeshRenderer>(out MeshRenderer render))
                {
                    render.material.color = colour;
                }
            }
        }

    }

    private bool DoorValidPlace()
    {
        bool hitWall = false;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            hitWall = hit.collider.gameObject.tag == "Wall";
        }

        return hitWall && IsValidToPlace();
    }

    private bool IsValidToPlace()
    {
        if (placementObject.GetComponent<ShelfHandler>() && money.currMoney < shelfPrice)
        {
            return false;
        }
        else if (placementObject.GetComponent<ChairHolder>() && money.currMoney < chairPrice)
        {
            return false;
        }
        else if (placementObject.GetComponent<DoorHandler>() && money.currMoney < doorPrice)
        {
            return false;
        }
        else if (interactions.objectSelector == UIInteraction.OBJECTSELECTOR.wall && money.currMoney < wallPrice)
        {
            return false;
        }
        else if (placementObject.GetComponent<RegisterHolder>() && money.currMoney < registerPrice)
        {
            return false; 
        }


        if (placementObject)
        {
            GameObject obj = placementObject;
            List<Collider> colList = new List<Collider>();
            List<Collider> finalList = new List<Collider>();
            List<Collider> boxCheckList = new List<Collider>();
            Vector3 negativeEnds = Vector3.zero;

            if (placementObject.transform.parent != null)
            {
                obj = placementObject.transform.parent.gameObject;
            }

            if (obj.GetComponent<ScaleWall>())
            {
                negativeEnds = new Vector3(0, 0, 1.5f);
            }


            colList.Add(obj.GetComponent<Collider>());
            boxCheckList.AddRange(Physics.OverlapBox(obj.transform.position + obj.transform.up * 0.1f,
                (obj.transform.localScale / 2) - negativeEnds, obj.transform.rotation));

            foreach (Transform child in obj.transform)
            {
                if (child.TryGetComponent<Collider>(out Collider col))
                {
                    colList.Add(col);
                    boxCheckList.AddRange(Physics.OverlapBox(child.transform.position + child.transform.up * 0.1f,
                        (child.transform.localScale / 2) - negativeEnds, child.transform.rotation));
                }
            }

            boxCheckList.RemoveAll(item => colList.Contains(item));
            boxCheckList.RemoveAll(item => item.GetComponent<NavMeshSurface>());


            //ignore collisions with certain objects
            if (obj.GetComponent<HoldConnectedWalls>())
            {
                boxCheckList.RemoveAll(item => item.GetComponent<ScaleWall>());
                boxCheckList.RemoveAll(item => item.GetComponent<HoldConnectedWalls>());
            }

            if (interactions.objectSelector == UIInteraction.OBJECTSELECTOR.door)
            {
                boxCheckList.RemoveAll(item => item.GetComponent<ScaleWall>());
            }

            if (obj.GetComponent<ScaleWall>())
            {
                boxCheckList.RemoveAll(item => item.GetComponent<HoldConnectedWalls>());
            }


            /*     foreach (Collider col in boxCheckList)
                 {
                     if (!col.gameObject.GetComponent<NavMeshSurface>())
                     {
                         finalList.Add(col);
                     }
                 }*/


            return (boxCheckList.Count == 0);
        }
        return (false);

    }

    private void objectFollowMouse(Vector3 position)
    {

        placementObject.transform.position = position + new Vector3(0, placementObject.transform.localScale.y / 2, 0);

    }

    private Vector3 PutOnTheFloor(Vector3 pos, Vector3 size)
    {
        Vector3 newpos = new Vector3(pos.x, floorHeight, pos.z);
        return newpos;
    }

    private void ShelfUI()
    {
        if (interactions.objectSelector == UIInteraction.OBJECTSELECTOR.none && inputs.leftClick)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.parent != null)
                {
                    if (hit.transform.parent.TryGetComponent<ShelfHandler>(out ShelfHandler obj) && obj.UI == null)
                    {
                        GameObject temp = Resources.Load<GameObject>("Prefabs/UI/ShelfMenu");
                        obj.UI = Instantiate(temp, GameObject.FindObjectOfType<Canvas>().transform);
                        //Vector3 pos = Input.mousePosition / GameObject.FindObjectOfType<Canvas>().scaleFactor;
                        Vector3 pos = Camera.main.WorldToScreenPoint(obj.transform.position);
                        pos = pos / GameObject.FindObjectOfType<Canvas>().scaleFactor; 
                        obj.UI.transform.localPosition = pos;
                    }
                }
            }
        }
    }

    private void placeObject()
    {
        if (inputs.leftClick)
        {
            if (placementObject.TryGetComponent<NavAgentSpawner>(out NavAgentSpawner spawner))
            {
                spawner.enabled = true;
            }
            if ((IsValidToPlace() && interactions.objectSelector != UIInteraction.OBJECTSELECTOR.door) || (interactions.objectSelector == UIInteraction.OBJECTSELECTOR.door && DoorValidPlace()))
            {
                switch (interactions.objectSelector)
                {
                    case UIInteraction.OBJECTSELECTOR.none:
                        break;
                    case UIInteraction.OBJECTSELECTOR.wall:
                        money.currMoney -= wallPrice;
                        break;
                    case UIInteraction.OBJECTSELECTOR.door:
                        money.currMoney -= doorPrice;
                        break;
                    case UIInteraction.OBJECTSELECTOR.genericObject:

                        if (placementObject.GetComponent<ShelfHandler>())
                        {
                            money.currMoney -= shelfPrice;
                        }
                        else if (placementObject.GetComponent<RegisterHolder>())
                        {
                            money.currMoney -= registerPrice;
                        }
                        else if (placementObject.GetComponent<ChairHolder>())
                        {
                            money.currMoney -= chairPrice;
                        }
                        else if (placementObject.GetComponent<LandmarkHandler>())
                        {
                            placementObject.GetComponent<LandmarkHandler>().enabled = true; 
                        }
                        break;
                    case UIInteraction.OBJECTSELECTOR.deletion:
                        break;
                }

                if (placementObject.TryGetComponent(out Collider col))
                {
                    col.enabled = true;
                }
                if (placementObject.TryGetComponent(out NavMeshObstacle obs))
                {
                    obs.enabled = true;
                }
                if (!placementObject.GetComponent<LandmarkHandler>() && !placementObject.GetComponent<AgentSpawner>() && !placementObject.GetComponent<AgentKiller>())
                {
                    if (placementObject.transform.childCount > 0)
                    {
                        foreach (Transform child in placementObject.transform)
                        {
                            if (child.TryGetComponent(out NavMeshObstacle obs2))
                            {
                                obs2.enabled = true;
                            }
                            if (child.TryGetComponent(out Collider col2))
                            {
                                col2.enabled = true;
                            }
                            child.gameObject.layer = 0;
                        }
                    }
                    placementObject.layer = 0;
                }
                if (interactions.objectSelector == UIInteraction.OBJECTSELECTOR.wall)
                {
                    if (placementObject.transform.parent != null)
                    {
                        placementObject.transform.parent.gameObject.layer = 0;
                        placementObject.transform.parent.GetComponent<ScaleWall>().enabled = false;
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit))
                        {
                            if (hit.collider.gameObject.GetComponent<HoldConnectedWalls>())
                            {
                                GameObject obj = null;

                                if (roofEnabled && roof)
                                {
                                    bool closed;
                                    closed = roof.GetComponent<EditMesh>().AddPoint(placementObject.transform);
                                    floor.GetComponent<EditMesh>().AddPoint(placementObject.transform);
                                    roof.GetComponent<EditMesh>().SetMesh();
                                    floor.GetComponent<EditMesh>().SetMesh();
                                    if (closed)
                                    {
                                        floor.AddComponent<FloorHandler>();
                                        floor.GetComponent<FloorHandler>().roomType = FloorHandler.ROOMTYPE.emptyRoom;
                                        roof = null;
                                        floor = null;

                                        interactions.objectSelector = UIInteraction.OBJECTSELECTOR.none;
                                        placementObject = null;
                                        return;
                                    }
                                }

                                obj = Resources.Load<GameObject>("Prefabs/WallTray/BrickWallNoPost");
                                GameObject temp = Instantiate(obj);
                                temp.GetComponent<ScaleWall>().post = hit.collider.gameObject;
                                placementObject = temp.transform.GetChild(0).gameObject;



                            }
                            else
                            {
                                GameObject obj = Resources.Load<GameObject>("Prefabs/WallTray/BrickWall");
                                Vector3 pos = placementObject.transform.position;
                                placementObject = Instantiate(obj);
                                objectFollowMouse(PutOnTheFloor(pos, placementObject.transform.localScale));
                                if (roofEnabled && roof)
                                {
                                    bool closed;
                                    closed = roof.GetComponent<EditMesh>().AddPoint(placementObject.transform);
                                    floor.GetComponent<EditMesh>().AddPoint(placementObject.transform);
                                    roof.GetComponent<EditMesh>().SetMesh();
                                    floor.GetComponent<EditMesh>().SetMesh();
                                    if (closed)
                                    {
                                        roof = null;
                                        floor = null;
                                    }
                                }
                                placeObject();
                            }

                        }

                    }
                    else
                    {

                        GameObject roofObj;
                        GameObject floorObj;
                        if (roofEnabled && !roof && placementObject.GetComponent<HoldConnectedWalls>())
                        {
                            roofObj = Resources.Load<GameObject>("Prefabs/RoofAndFloor/Roof");
                            floorObj = Resources.Load<GameObject>("Prefabs/RoofAndFloor/Floor");
                            roof = Instantiate(roofObj);
                            floor = Instantiate(floorObj);
                            roof.GetComponent<EditMesh>().AddPoint(placementObject.transform);
                            floor.GetComponent<EditMesh>().AddPoint(placementObject.transform);
                            roof.GetComponent<EditMesh>().SetMesh();
                            floor.GetComponent<EditMesh>().SetMesh();

                        }

                        GameObject obj = Resources.Load<GameObject>("Prefabs/WallTray/BrickWallNoPost");
                        GameObject temp = Instantiate(obj);
                        temp.GetComponent<ScaleWall>().post = placementObject;
                        placementObject = temp.transform.GetChild(0).gameObject;
                    }
                }
                else if (interactions.objectSelector == UIInteraction.OBJECTSELECTOR.door)
                {
                    placementObject.layer = 2;
                    foreach (Transform child in placementObject.transform)
                    {
                        child.gameObject.layer = 2;
                    }

                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Physics.Raycast(ray, out hit);
                    GameObject originalWall = hit.transform.gameObject;
                    ScaleWall script = originalWall.GetComponent<ScaleWall>();
                    GameObject post = script.post;
                    GameObject wallEnd = script.wallEnd;
                    placementObject.transform.rotation = originalWall.transform.rotation;
                    placementObject.transform.Rotate(0, 90, 0);

                    Vector3 pos = hit.point - hit.normal.normalized * (hit.transform.localScale.x / 2);
                    placementObject.transform.position = PutOnTheFloor(pos + Vector3.down * 0.5f, placementObject.transform.localScale);
                    originalWall.transform.GetChild(0).parent = null;
                    GameObject obj = Resources.Load<GameObject>("Prefabs/WallTray/BrickWallNoPost");
                    GameObject wall1 = originalWall;
                    wall1.GetComponent<ScaleWall>().enabled = true;
                    GameObject wall2 = Instantiate(obj);
                    wall1.GetComponent<ScaleWall>().post = post;
                    wall2.GetComponent<ScaleWall>().post = wallEnd;
                    wall1.transform.position = post.transform.position;
                    wall2.transform.position = wallEnd.transform.position;

                    wall1.GetComponent<NavMeshObstacle>().enabled = true;
                    wall1.GetComponent<Collider>().enabled = true;
                    wall1.layer = 0;
                    wall2.GetComponent<NavMeshObstacle>().enabled = true;
                    wall2.GetComponent<Collider>().enabled = true;
                    wall2.layer = 0;

                    placementObject.GetComponent<DoorHandler>().wall1 = wall1;
                    placementObject.GetComponent<DoorHandler>().wall2 = wall2;



                    if (Vector3.Distance(post.transform.position, placementObject.transform.Find("LeftSide").position) >
                        Vector3.Distance(post.transform.position, placementObject.transform.Find("RightSide").position))
                    {
                        wall1.GetComponent<ScaleWall>().wallEnd = placementObject.transform.Find("RightSide").gameObject;
                        wall2.GetComponent<ScaleWall>().wallEnd = placementObject.transform.Find("LeftSide").gameObject;
                    }
                    else
                    {
                        wall1.GetComponent<ScaleWall>().wallEnd = placementObject.transform.Find("LeftSide").gameObject;
                        wall2.GetComponent<ScaleWall>().wallEnd = placementObject.transform.Find("RightSide").gameObject;
                    }

                    placementObject.layer = 0;
                    foreach (Transform child in placementObject.transform)
                    {
                        child.gameObject.layer = 0;
                    }
                    placementObject = null;
                }
                else
                {
                    placementObject = null;
                }
                StartCoroutine(navManager.UpdateObstacles()); 
            }
        }
    }

    private void PickObject()
    {
        if (interactions.changedObject && placementObject != null)
        {
            roof = null;
            floor = null;
            interactions.changedObject = false;
            if (placementObject.transform.parent == null)
            {
                Destroy(placementObject);
            }
            else
            {
                Destroy(placementObject.transform.parent.gameObject);
            }
        }


        if (interactions.objectSelector != UIInteraction.OBJECTSELECTOR.none && interactions.objectSelector != UIInteraction.OBJECTSELECTOR.deletion && placementObject == null)
        {
            GameObject obj = Resources.Load<GameObject>("Prefabs/" + interactions.selectedTray + "Tray/" + interactions.selectedObject);
            placementObject = Instantiate(obj);
        }

        if ((interactions.objectSelector == UIInteraction.OBJECTSELECTOR.none || interactions.objectSelector == UIInteraction.OBJECTSELECTOR.deletion) && placementObject != null)
        {
            roof = null;
            floor = null;
            interactions.changedObject = false;
            if (placementObject.transform.parent == null)
            {
                Destroy(placementObject);
            }
            else
            {
                Destroy(placementObject.transform.parent.gameObject);
            }
            placementObject = null;
        }
    }

}
