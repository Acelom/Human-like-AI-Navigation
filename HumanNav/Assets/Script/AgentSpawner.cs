using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoneyMaster;
using CommonItems;
using System.Linq;

public class AgentSpawner : MonoBehaviour
{
    private GameObject customer;
    private GameObject employee;
    private int totalAgents;
    private MoneyManager money;
    private CommonMaster com;

    [SerializeField] private int maxCustomerCount;

    [SerializeField] private float vegDraw;
    [SerializeField] private float canDraw;
    [SerializeField] private float breadDraw;
    [SerializeField] private float totalDraw;
    private float defaultDraw;

    private float vegValue;
    private float canValue;
    private float breadValue;
    private float defaultValue;

    public int maxEmployeeCount;

    private List<GameObject> employees;
    public List<GameObject> customers;

    [SerializeField] private int totalStock;
    [SerializeField] private int breadStock;
    [SerializeField] private int vegStock;
    [SerializeField] private int canStock;

    private void Awake()
    {
        customer = Resources.Load<GameObject>("Prefabs/AIAgents/Customer");
        customers = new List<GameObject>();
        employee = Resources.Load<GameObject>("Prefabs/AIAgents/Employee");
        employees = new List<GameObject>();
        com = GameObject.FindGameObjectWithTag("GameController").GetComponent<CommonMaster>();
        money = GameObject.FindGameObjectWithTag("GameController").GetComponent<MoneyManager>();
        defaultValue = (int)MoneyManager.STOCKQUALITY.average * 0.7f;
        defaultDraw = defaultValue * 30;
    }

    private void Update()
    {
        if (com.shelfList.Where(item => item.GetComponent<ShelfHandler>().stockType == ShelfHandler.STOCKTYPE.vegetables && item.GetComponent<ShelfHandler>().currStock > 0)
                         .Count() > 0)
        {

            vegStock = com.shelfList.Where(item => item.GetComponent<ShelfHandler>().stockType == ShelfHandler.STOCKTYPE.vegetables)
                                    .Select(item => item.GetComponent<ShelfHandler>().currStock)
                                    .Sum();
        }
        else
        {
            vegStock = 0;
        }

        if (com.shelfList.Where(item => item.GetComponent<ShelfHandler>().stockType == ShelfHandler.STOCKTYPE.cans && item.GetComponent<ShelfHandler>().currStock > 0)
                        .Count() > 0)
        {
            canStock = com.shelfList.Where(item => item.GetComponent<ShelfHandler>().stockType == ShelfHandler.STOCKTYPE.cans)
                                .Select(item => item.GetComponent<ShelfHandler>().currStock)
                                .Sum();
        }
        else
        {
            canStock = 0;
        }

        if (com.shelfList.Where(item => item.GetComponent<ShelfHandler>().stockType == ShelfHandler.STOCKTYPE.bread && item.GetComponent<ShelfHandler>().currStock > 0)
                                .Count() > 0)
        {
            breadStock = com.shelfList.Where(item => item.GetComponent<ShelfHandler>().stockType == ShelfHandler.STOCKTYPE.bread)
                              .Select(item => item.GetComponent<ShelfHandler>().currStock)
                              .Sum();
        }
        else
        {
            breadStock = 0;
        }

        totalStock = vegStock + canStock + vegStock;

        vegDraw = vegStock * (float)money.vegQuality * ((100 - money.vegProfitPercentage) / 100);
        breadDraw = breadStock * (float)money.breadQuality * ((100 - money.breadProfitPercentage) / 100);
        canDraw = canStock * (float)money.cansQuality * ((100 - money.canProfitPercentage) / 100);
        totalDraw = vegDraw + breadDraw + canDraw;

        vegValue = (float)money.vegQuality * ((100 - money.vegProfitPercentage) / 100);
        breadValue = (float)money.breadQuality * ((100 - money.breadProfitPercentage) / 100);
        canValue = (float)money.cansQuality * ((100 - money.canProfitPercentage) / 100);

        HandleCustomerSpawning();
        HandleEmployeeSpawning();
    }

    private void HandleCustomerSpawning()
    {
        if (com.totalCustomers < maxCustomerCount)
        {
            if (totalDraw > 0)
            {
                if (com.totalCustomers < totalDraw / defaultDraw)
                {
                    GameObject go = Instantiate(customer, position: transform.position, transform.rotation);
                    CustomerAI cus = go.GetComponent<CustomerAI>();
                    if (breadStock > 0)
                    {
                        cus.breadNeeded = Mathf.RoundToInt(Random.Range(2 * (breadValue / 3), 4 * (breadValue / 3)));
                    }
                    else
                    {
                        cus.breadNeeded = 0;
                    }

                    if (vegStock > 0)
                    {
                        cus.vegNeeded = Mathf.RoundToInt(Random.Range(2 * (vegValue / 3), 4 * (vegValue / 3)));
                    }
                    else
                    {
                        cus.vegNeeded = 0;
                    }

                    if (canStock > 0)
                    {
                        cus.canNeeded = Mathf.RoundToInt(Random.Range(2 * (canValue / 3), 4 * (canValue / 3)));
                    }
                    else
                    {
                        cus.canNeeded = 0;
                    }
                }
            }
        }
    }

    private void HandleEmployeeSpawning()
    {
        if (com.totalEmployees < maxEmployeeCount)
        {
            employees.Add(Instantiate(employee, position: transform.position, transform.rotation));
        }
        else if (com.totalEmployees > maxEmployeeCount)
        {
            //GameObject go = null; 
            GameObject go = employees.FirstOrDefault(item => item.GetComponent<EmployeeAI>().employeeState == EmployeeAI.EMPLOYEESTATE.idle);
            if (go != null)
            {
                employees.Remove(go);
                Destroy(go);
            }
        }
    }

}
