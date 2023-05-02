using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq; 

public class Shelf
{
    public GameObject shelf;
    public List<GameObject> shelfItems;


    public Shelf(GameObject mainPart, List<GameObject> shelfThings)
    {
        shelf = mainPart;
        shelfItems = new List<GameObject>(shelfThings);
    }
}




public class ShelfHandler : MonoBehaviour
{
    private Shelf topShelf;
    private Shelf bottomShelf;
    [SerializeField] private GameObject topShelfObject;
    [SerializeField] private GameObject bottomShelfObject;
    [SerializeField] private Transform standingSpot;
    public bool spotFilled = false;
    public int maxStock = 30;
    public int currStock = 0;

    [SerializeField] private GameObject crate;
    [SerializeField] private GameObject veg;
    [SerializeField] private GameObject can;
    [SerializeField] private GameObject bread;
    private List<GameObject> crateList;
    private List<Transform> placeList;
    private List<GameObject> allItems;
    private bool setUpUI;
    public GameObject UI;

    public enum STOCKTYPE
    {
        none,
        vegetables,
        cans,
        bread,
    }
    public STOCKTYPE stockType = STOCKTYPE.none;

    private void Awake()
    {
        placeList = new List<Transform>();
        allItems = new List<GameObject>();
        crateList = new List<GameObject>();
        List<GameObject> itemList = new List<GameObject>();
        foreach (Transform shelfItem in topShelfObject.transform)
        {
            itemList.Add(shelfItem.gameObject);
        }
        topShelf = new Shelf(topShelfObject, itemList);
        itemList = new List<GameObject>();
        foreach (Transform shelfItem in bottomShelfObject.transform)
        {
            itemList.Add(shelfItem.gameObject);
        }
        bottomShelf = new Shelf(bottomShelfObject, itemList);
        setUpUI = true; 



    }

    private void ButtonPressed(STOCKTYPE button)
    {
        setUpUI = true; 
        foreach (Transform child in UI.transform)
        {
            if (child.GetComponent<Button>())
            {
                child.GetComponent<Button>().onClick.RemoveAllListeners();
            }
        }

        currStock = 0;
        stockType = button;
        Destroy(UI);
        UI = null;
    }
      


    private void Update()
    {
        if (UI)
        {
            foreach (Transform child in UI.transform)
            {
                if (child.GetComponent<Button>() && setUpUI)
                {
                    switch (child.GetChild(0).GetComponent<TextMeshProUGUI>().text)
                    {
                        case "None":
                            child.GetComponent<Button>().onClick.AddListener(() => ButtonPressed(STOCKTYPE.none));
                            break;
                        case "Cans":
                            child.GetComponent<Button>().onClick.AddListener(() => ButtonPressed(STOCKTYPE.cans));
                            break;
                        case "Bread":
                            child.GetComponent<Button>().onClick.AddListener(() => ButtonPressed(STOCKTYPE.bread));
                            break;
                        case "Vegetables":
                            child.GetComponent<Button>().onClick.AddListener(() => ButtonPressed(STOCKTYPE.vegetables));
                            break;
                    }
                }
            }
            setUpUI = false; 

            Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
            pos = pos / GameObject.FindObjectOfType<Canvas>().scaleFactor;
            pos = pos - ((new Vector3(Screen.currentResolution.width, Screen.currentResolution.height, 0) / 4) 
                / GameObject.FindObjectOfType<Canvas>().scaleFactor);  
            UI.transform.localPosition = pos;
        }


        if (stockType != STOCKTYPE.none && crateList.Count < (bottomShelf.shelfItems.Count + topShelf.shelfItems.Count))
        {
            foreach (GameObject obj in bottomShelf.shelfItems)
            {
                GameObject newObject = Instantiate(crate, obj.transform);
                crateList.Add(newObject);
                placeList.AddRange(newObject.GetComponent<CrateHandler>().objPositions);
            }
            foreach (GameObject obj in topShelf.shelfItems)
            {
                GameObject newObject = Instantiate(crate, obj.transform);
                crateList.Add(newObject);
                placeList.AddRange(newObject.GetComponent<CrateHandler>().objPositions);
            }
        }
        if (stockType == STOCKTYPE.none && crateList.Count > 0)
        {
            int length = crateList.Count; 
            for (int i = 0; i < length; i++)
            {
                Destroy(crateList[0]);
                crateList.RemoveAt(0); 
            }
        }

        if (currStock > allItems.Count)
        {
            GameObject go = null; 
            switch (stockType)
            {
                case STOCKTYPE.none:
                    break;
                case STOCKTYPE.vegetables:
                    go = veg;
                    break;
                case STOCKTYPE.cans:
                    go = can; 
                    break;
                case STOCKTYPE.bread:
                    go = bread; 
                    break;
            }
            allItems.Add(Instantiate(go, placeList[currStock - 1]));
        }
        if (currStock < allItems.Count)
        {
            GameObject go = null;
            go = allItems[allItems.Count - 1];
            allItems.RemoveAt(allItems.Count - 1); 
            Destroy(go); 
        }
    }

    public Shelf GetTopShelf()
    {
        return topShelf;
    }

    public Shelf getBottomShelf()
    {
        return bottomShelf;
    }

    public Transform GetStandingSpot()
    {
        return standingSpot;
    }
}
