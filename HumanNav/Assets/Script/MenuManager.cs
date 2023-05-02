using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using MoneyMaster;
using CommonItems;


public class MenuManager : MonoBehaviour
{
    private AgentSpawner spawner;
    private CommonMaster com;


    [SerializeField] private GameObject veg;
    [SerializeField] private TMP_Dropdown vegQuality;
    [SerializeField] private GameObject vegProfit;


    [SerializeField] private GameObject bread;
    [SerializeField] private TMP_Dropdown breadQuality;
    [SerializeField] private GameObject breadProfit;

    [SerializeField] private GameObject cans;
    [SerializeField] private TMP_Dropdown canQuality;
    [SerializeField] private GameObject canProfit;

    private MoneyManager money;


    [SerializeField] private GameObject employees;
    private Button employeePlus;
    private Button employeeMinus;
    [SerializeField] private TextMeshProUGUI employeeText;
    private int employeeCount = 0;


    private void Awake()
    {
        vegQuality.value = 1;
        canQuality.value = 1;
        breadQuality.value = 1;

        money = GameObject.FindObjectOfType<MoneyManager>();
        spawner = GameObject.FindObjectOfType<AgentSpawner>();
        com = GameObject.FindObjectOfType<CommonMaster>();

        foreach (Transform child in employees.transform)
        {
            if (child.name == "Plus")
            {
                employeePlus = child.GetComponent<Button>();
                employeePlus.onClick.AddListener(IncreaseEmployeeCount);
            }
            if (child.name == "Minus")
            {
                employeeMinus = child.GetComponent<Button>();
                employeeMinus.onClick.AddListener(DecreaseEmployeeCount);
            }
        }

        canQuality.onValueChanged.AddListener(delegate { ResetCanShelfs(); });
        breadQuality.onValueChanged.AddListener(delegate { ResetBreadShelfs(); });
        vegQuality.onValueChanged.AddListener(delegate { ResetVegShelfs(); });
    }


    private void ResetVegShelfs()
    {
        List<GameObject> list = new List<GameObject>(com.shelfList.Where(item =>
        item.GetComponent<ShelfHandler>().stockType == ShelfHandler.STOCKTYPE.vegetables && item.GetComponent<ShelfHandler>().currStock > 0));

        foreach (GameObject go in list)
        {
            money.currMoney -= go.GetComponent<ShelfHandler>().currStock * 20;
            go.GetComponent<ShelfHandler>().currStock = 0;
        }
    }
    private void ResetBreadShelfs()
    {
        List<GameObject> list = new List<GameObject>(com.shelfList.Where(item =>
       item.GetComponent<ShelfHandler>().stockType == ShelfHandler.STOCKTYPE.bread && item.GetComponent<ShelfHandler>().currStock > 0));

        foreach (GameObject go in list)
        {
            money.currMoney -= go.GetComponent<ShelfHandler>().currStock * 20;
            go.GetComponent<ShelfHandler>().currStock = 0;
        }
    }
    private void ResetCanShelfs()
    {
        List<GameObject> list = new List<GameObject>(com.shelfList.Where(item =>
               item.GetComponent<ShelfHandler>().stockType == ShelfHandler.STOCKTYPE.cans && item.GetComponent<ShelfHandler>().currStock > 0));

        foreach (GameObject go in list)
        {
            money.currMoney -= go.GetComponent<ShelfHandler>().currStock * 20;
            go.GetComponent<ShelfHandler>().currStock = 0;
        }
    }

    private void IncreaseEmployeeCount()
    {
        employeeCount += 1;
    }
    private void DecreaseEmployeeCount()
    {
        employeeCount -= 1;
    }

    private void Update()
    {
        spawner.maxEmployeeCount = employeeCount;
        employeeText.text = "Employees: " + employeeCount.ToString();

        money.breadQuality = (MoneyManager.STOCKQUALITY)((breadQuality.value + 1) * 5);
        money.vegQuality = (MoneyManager.STOCKQUALITY)((vegQuality.value + 1) * 5);
        money.cansQuality = (MoneyManager.STOCKQUALITY)((canQuality.value + 1) * 5);

        foreach (Transform child in vegProfit.transform)
        {
            if (child.name == "Placeholder")
            {
                child.GetComponent<TextMeshProUGUI>().text = money.vegProfitPercentage.ToString();
            }
        }
        foreach (Transform child in breadProfit.transform)
        {
            if (child.name == "Placeholder")
            {
                child.GetComponent<TextMeshProUGUI>().text = money.breadProfitPercentage.ToString();
            }
        }
        foreach (Transform child in canProfit.transform)
        {
            if (child.name == "Placeholder")
            {
                child.GetComponent<TextMeshProUGUI>().text = money.canProfitPercentage.ToString();
            }
        }


        if (vegProfit.transform.parent.GetComponent<TMP_InputField>().text.Length > 0)
        {
            if (!float.TryParse(vegProfit.transform.parent.GetComponent<TMP_InputField>().text, out float vegTemp))
            {
                money.vegProfitPercentage = 30;
            }
            else
            {
                vegTemp = Mathf.Clamp(vegTemp, -100, 100);
                money.vegProfitPercentage = vegTemp;
                vegProfit.transform.parent.GetComponent<TMP_InputField>().text = vegProfit.ToString(); 
            }
        }
        if (canProfit.transform.parent.GetComponent<TMP_InputField>().text.Length > 0)
        {
            if (!float.TryParse(canProfit.transform.parent.GetComponent<TMP_InputField>().text, out float canTemp))
            {
                money.canProfitPercentage = 30;
            }
            else
            {
                canTemp = Mathf.Clamp(canTemp, -100, 100);
                money.canProfitPercentage = canTemp;
                canProfit.transform.parent.GetComponent<TMP_InputField>().text = canTemp.ToString() ; 
            }
        }
        if (breadProfit.transform.parent.GetComponent<TMP_InputField>().text.Length > 0)
        {
            if (!float.TryParse(breadProfit.transform.parent.GetComponent<TMP_InputField>().text, out float breadTemp))
            {
                money.breadProfitPercentage = 30;
            }
            else
            {
                breadTemp = Mathf.Clamp(breadTemp, -100, 100); 
                money.breadProfitPercentage = breadTemp;
                breadProfit.transform.parent.GetComponent<TMP_InputField>().text = breadTemp.ToString(); 
            }
        }
    }


}
