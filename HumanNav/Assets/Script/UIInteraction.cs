using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayerController;
using System.Linq;
using CommonItems;

namespace UIInteractions
{
    public class UIInteraction : MonoBehaviour
    {
        private ToggleGroup topGroup;
        private ToggleGroup trayGroup;
        private List<GameObject> allToggles;
        private List<Toggle> topToggles;
        private List<Toggle> trayToggles;
        private List<Button> buttons;
        private Inputs inputs;
        private bool menuActivated;
        private List<GameObject> menuObjects;
        private CommonMaster com;

        private Vector3 drawerDefault;
        private Vector3 drawerUp;
        private ColorBlock colorDefault;
        private ColorBlock colorUp;

        public bool changedObject;
        public string selectedTray;
        public string selectedObject;

        public enum OBJECTSELECTOR
        {
            none,
            wall,
            door,
            genericObject,
            deletion,
        };

        public OBJECTSELECTOR objectSelector = OBJECTSELECTOR.none;


        [SerializeField] private float drawerSpeed;

        private void Awake()
        {
            com = GameObject.FindObjectOfType<CommonMaster>();
            inputs = Camera.main.gameObject.GetComponent<Inputs>();
            menuObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Menu"));
            allToggles = new List<GameObject>();
            foreach (Toggle tog in FindObjectsOfType<Toggle>())
            {
                allToggles.Add(tog.gameObject);
            }

            topToggles = new List<Toggle>();
            trayToggles = new List<Toggle>();

            foreach (GameObject go in allToggles)
            {
                if (go.tag == "TopGroup")
                {
                    topGroup = go.GetComponentInParent<ToggleGroup>();
                    topGroup.RegisterToggle(go.GetComponent<Toggle>());
                    go.GetComponent<Toggle>().group = topGroup;
                    topToggles.Add(go.GetComponent<Toggle>());
                }
                else if (go.tag == "TrayGroup")
                {
                    trayGroup = go.transform.parent.GetComponentInParent<ToggleGroup>();
                    trayGroup.RegisterToggle(go.GetComponent<Toggle>());
                    go.GetComponent<Toggle>().group = trayGroup;
                    trayToggles.Add(go.GetComponent<Toggle>());
                }
            }

            //buttons = new List<Button>(GameObject.FindObjectsOfType<Button>());

            foreach (Toggle tog in topToggles)
            {
                colorDefault = topToggles[0].colors;
                colorUp = colorDefault;
                colorUp.normalColor = new Color32(150, 150, 150, 252);
            }

            drawerDefault = topGroup.transform.localPosition;
            drawerUp = drawerDefault + new Vector3(0, 200, 0);
        }

        public void DestroyShelfUI()
        {
            foreach (GameObject ui in com.shelfList.Select(item => item.GetComponent<ShelfHandler>().UI).ToList())
            {
                Destroy(ui);
            }
            com.shelfList.All(item => item.GetComponent<ShelfHandler>().UI = null);
        }

        private void Update()
        {
            if (menuActivated)
            {
                GameObject go = menuObjects.First(item => item.GetComponent<MenuManager>());
                go.GetComponent<MenuManager>().enabled = true;
                go.transform.localPosition = Vector3.Lerp(go.transform.localPosition, new Vector3(0, 0, 0), Time.deltaTime * drawerSpeed);
            }
            else
            {
                GameObject go = menuObjects.First(item => item.GetComponent<MenuManager>());
                go.GetComponent<MenuManager>().enabled = false;
                go.transform.localPosition = Vector3.Lerp(go.transform.localPosition, new Vector3(0, 900, 0), Time.deltaTime * drawerSpeed);
            }

            MoveDrawer(topGroup.AnyTogglesOn() && !topToggles.Any(item => (item.name == "Delete" || item.name == "MenuButton") && item.isOn));
            AssignObject();

            if (inputs.rightClick)
            {
                if (com.shelfList.Any(item => item.GetComponent<ShelfHandler>().UI != null))
                {
                    DestroyShelfUI();
                }
                if (trayGroup.AnyTogglesOn())
                {
                    objectSelector = OBJECTSELECTOR.none;
                    trayGroup.SetAllTogglesOff();
                }
                else
                {
                    topGroup.SetAllTogglesOff();
                }
                menuActivated = false;
            }
        }

        private void AssignObject()
        {
            if (topGroup.AnyTogglesOn())
            {
                GameObject go = topGroup.ActiveToggles().ToList()[0].gameObject;
                if (selectedTray != go.name)
                {
                    trayGroup.SetAllTogglesOff();
                    changedObject = true;
                }
                selectedTray = go.name;
            }

            if (trayGroup.AnyTogglesOn())
            {
                GameObject go = trayGroup.ActiveToggles().ToList()[0].gameObject;
                if (selectedObject != go.name)
                {
                    changedObject = true;
                }
                selectedObject = go.name;
            }

            foreach (Toggle tog in trayToggles)
            {
                if (tog.transform.parent.name == selectedTray + "Tray")
                {
                    tog.gameObject.GetComponent<Image>().enabled = true;
                    tog.transform.GetChild(0).GetComponent<Text>().enabled = true;
                    tog.enabled = true;
                }
                else
                {
                    tog.gameObject.GetComponent<Image>().enabled = false;
                    tog.transform.GetChild(0).GetComponent<Text>().enabled = false;
                    tog.enabled = false;
                }
            }

            if (!trayGroup.AnyTogglesOn())
            {
                objectSelector = OBJECTSELECTOR.none;
            }
            else
            {
                switch (selectedTray)
                {
                    case "Wall":
                        objectSelector = OBJECTSELECTOR.wall;
                        break;
                    case "Door":
                        objectSelector = OBJECTSELECTOR.door;
                        break;
                    case "Object":
                        objectSelector = OBJECTSELECTOR.genericObject;
                        break;
                }
            }
        }

        private void LateUpdate()
        {
            Toggle name = topToggles.Where(item => item.isOn).FirstOrDefault();
            if (name != null)
            {
                if (name.name == "Delete")
                {
                    objectSelector = OBJECTSELECTOR.deletion;
                }

                if (name.name == "MenuButton")
                {
                    menuActivated = true;
                    objectSelector = OBJECTSELECTOR.none;
                }
            }

            name = topToggles.Where(item => !item.isOn && item.name == "MenuButton").FirstOrDefault();
            if (name != null)
            {
                menuActivated = false;
            }
        }

        private void MoveDrawer(bool movingUp)
        {
            if (movingUp)
            {
                if (Vector3.Distance(drawerUp, topGroup.transform.localPosition) < 5)
                {
                    topGroup.transform.localPosition = drawerUp;
                }
                else
                {
                    topGroup.transform.localPosition = Vector3.Lerp(topGroup.transform.localPosition, drawerUp, Time.deltaTime * drawerSpeed);
                }

                foreach (Toggle tog in topToggles)
                {
                    tog.colors = colorUp;
                }
            }
            else
            {
                if (Vector3.Distance(drawerDefault, topGroup.transform.localPosition) < 5)
                {
                    topGroup.transform.localPosition = drawerDefault;
                }
                else
                {
                    topGroup.transform.localPosition = Vector3.Lerp(topGroup.transform.localPosition, drawerDefault, Time.deltaTime * drawerSpeed);
                }
                foreach (Toggle tog in topToggles)
                {
                    tog.colors = colorDefault;
                }
            }
        }
    }
}
