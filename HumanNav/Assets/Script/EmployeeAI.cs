using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmployeeItems;
using CommonItems;
using MoneyMaster;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Linq;

public class EmployeeAI : MonoBehaviour
{
    public enum EMPLOYEESTATE
    {
        idle,
        rest,
        stack,
        register,
        walk,
    }
    public EMPLOYEESTATE employeeState = EMPLOYEESTATE.idle;
    [SerializeField] private float walkDistance;

    [SerializeField] private float energy;
    [SerializeField] private float energyRestGain;
    [SerializeField] private float walkingEnergyLoss;
    [SerializeField] private float registerEnergyLoss;
    [SerializeField] private float stackEnergyLoss;
    [SerializeField] private float energyMax;
    [SerializeField] private float minEnergyToWork;

    private float restingTimer;
    [SerializeField] private float restingTimerLimit;
    [SerializeField] private float restingGain;

    private float energyCooldown;
    [SerializeField] private float energyCooldownDefault;

    private float idleTimer;
    [SerializeField] private float idleTimerLimit;

    private float stackTimer;
    [SerializeField] private float stackTimerLimit;

    private float registerTimer;
    [SerializeField] private float registerTimerLimit;


    private enum CURRACTIVE
    {
        none,
        register,
        chair,
        shelf,
    };
    private CURRACTIVE currActive = CURRACTIVE.none;
    private GameObject activeObject;

    private EmployeeMaster empMaster;
    private CommonMaster comMaster;
    private MoneyManager moneyManager;

    private NavMeshAgent agent;
    private NavMeshPath path;

    private void Awake()
    {
        empMaster = GameObject.FindGameObjectWithTag("GameController").GetComponent<EmployeeMaster>();
        comMaster = GameObject.FindGameObjectWithTag("GameController").GetComponent<CommonMaster>();
        moneyManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<MoneyManager>();
        energy = energyMax;
        energyCooldown = energyCooldownDefault;
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
    }


    private void Update()
    {
        if (energy < 0)
        {
            energy = 0;
        }

        switch (employeeState)
        {
            case EMPLOYEESTATE.idle:
                EmployeeIdle();
                break;
            case EMPLOYEESTATE.rest:
                EmployeeRest();
                break;
            case EMPLOYEESTATE.stack:
                EmployeeStack();
                break;
            case EMPLOYEESTATE.register:
                EmployeeRegister();
                break;
            case EMPLOYEESTATE.walk:
                EmployeeWalk();
                break;
        }
    }

    private Vector3 RouteToRegister()
    {

        if (comMaster.GetEmpRegisterAnyFree())
        {
            List<GameObject> registers = new List<GameObject>(comMaster.GetFreeEmpRegisters());
            GameObject chosenRegister = null;
            foreach (GameObject register in registers)
            {
                float distance = Mathf.Infinity;
                if (Vector3.Distance(transform.position, register.transform.position) < distance)
                {
                    distance = Vector3.Distance(transform.position, register.transform.position);
                    chosenRegister = register;
                }
            }

            if (chosenRegister != null)
            {
                activeObject = chosenRegister;
                activeObject.GetComponent<RegisterHolder>().employeeFilled = true;
                currActive = CURRACTIVE.register;
                return activeObject.GetComponent<RegisterHolder>().GetEmployeeSpot().position;
            }


        }
        return Vector3.positiveInfinity;

    }

    private Vector3 RouteToShelf()
    {
        if (comMaster.GetAnyShelfFree())
        {
            List<GameObject> shelfs = new List<GameObject>(comMaster.GetFreeShelfs());
            List<GameObject> shelfsNeedStocking = new List<GameObject>(shelfs.Where(item => item.GetComponent<ShelfHandler>().currStock < 5
            && item.GetComponent<ShelfHandler>().stockType != ShelfHandler.STOCKTYPE.none));
            GameObject chosenShelf = null;
            foreach (GameObject shelf in shelfsNeedStocking)
            {
                float distance = Mathf.Infinity;
                if (Vector3.Distance(transform.position, shelf.transform.position) < distance)
                {
                    distance = Vector3.Distance(transform.position, shelf.transform.position);
                    chosenShelf = shelf;
                }
            }

            if (chosenShelf != null)
            {
                activeObject = chosenShelf;
                activeObject.GetComponent<ShelfHandler>().spotFilled = true;
                currActive = CURRACTIVE.shelf;
                return activeObject.GetComponent<ShelfHandler>().GetStandingSpot().position;
            }


        }
        return Vector3.positiveInfinity;
    }

    private Vector3 RouteToChair()
    {
        if (empMaster.GetAnyChairFree())
        {
            List<GameObject> chairs = new List<GameObject>(empMaster.GetFreeChairs());
            GameObject chosenChair = null;
            foreach (GameObject chair in chairs)
            {
                float distance = Mathf.Infinity;
                if (Vector3.Distance(transform.position, chair.transform.position) < distance)
                {
                    distance = Vector3.Distance(transform.position, chair.transform.position);
                    chosenChair = chair;
                }
            }

            if (chosenChair != null)
            {
                activeObject = chosenChair;
                activeObject.GetComponent<ChairHolder>().spotFilled = true;
                currActive = CURRACTIVE.chair;
                return activeObject.GetComponent<ChairHolder>().GetSitSpot().position;
            }


        }
        return Vector3.positiveInfinity;
    }


    private void EmployeeIdle()
    {
        if (currActive == CURRACTIVE.none)
        {
            Vector3 pos = Vector3.positiveInfinity;

            if (energy < minEnergyToWork)
            {
                pos = RouteToChair();
            }
            else
            {
                pos = RouteToShelf();
                if (comMaster.GetEmpRegisterAnyFree() && currActive == CURRACTIVE.none && comMaster.totalCustomers > comMaster.GetFreeCusRegisters().Count)
                {
                    pos = RouteToRegister();
                }
            }
            if (pos.magnitude < new Vector3(500, 500, 500).magnitude)
            {
                agent.CalculatePath(pos, path);
            }
            if (path.status != NavMeshPathStatus.PathComplete && !(currActive == CURRACTIVE.chair && path.status == NavMeshPathStatus.PathPartial) || pos.magnitude > new Vector3(500, 500, 500).magnitude)
            {
                switch (currActive)
                {
                    case CURRACTIVE.none:
                        break;
                    case CURRACTIVE.register:
                        activeObject.GetComponent<RegisterHolder>().employeeFilled = false; 
                        break;
                    case CURRACTIVE.chair:
                        activeObject.GetComponent<ChairHolder>().spotFilled = false;
                        break;
                    case CURRACTIVE.shelf:
                        activeObject.GetComponent<ShelfHandler>().spotFilled = false;
                        break;
                }
                currActive = CURRACTIVE.none;
                activeObject = null;
            }

            if (currActive != CURRACTIVE.none)
            {
                agent.CalculatePath(pos, path);
                agent.SetPath(path);
                employeeState = EMPLOYEESTATE.walk;
                idleTimer = 0;
                return;
            }
        }

        if (idleTimerLimit < idleTimer)
        {
            Vector3 pos = Vector3.positiveInfinity; 
            do
            {
                Vector3 randomDirection = Random.insideUnitSphere * walkDistance;
                randomDirection += transform.position;
                NavMeshHit hit;
                NavMesh.SamplePosition(randomDirection, out hit, walkDistance, 1);
                pos = hit.position;
                if (pos.magnitude < new Vector3(500, 500, 500).magnitude)
                {
                    agent.CalculatePath(pos, path);
                    agent.SetPath(path);
                }
            } while (path.status != NavMeshPathStatus.PathComplete || pos.magnitude > new Vector3(500, 500, 500).magnitude);
            agent.SetPath(path);
            idleTimer = 0;
            employeeState = EMPLOYEESTATE.walk;
        }
        else
        {
            idleTimer += Time.deltaTime;
        }
    }

    private void EmployeeRest()
    {
        if (energy < energyMax)
        {
            if (energyCooldown > energyCooldownDefault)
            {
                energy += energyRestGain;
                energyCooldown = 0;
            }
            else
            {
                energyCooldown += Time.deltaTime;
            }
        }
        else if (energy >= energyMax)
        {
            energy = energyMax;
            employeeState = EMPLOYEESTATE.idle;
        }


    }

    private void EmployeeStack()
    {
        ShelfHandler stockingObject = activeObject.GetComponent<ShelfHandler>();
        if (stockingObject.currStock < stockingObject.maxStock)
        {
            if (stackTimer > stackTimerLimit)
            {
                stockingObject.currStock += 1;
                switch (stockingObject.stockType)
                {
                    case ShelfHandler.STOCKTYPE.none:
                        break;
                    case ShelfHandler.STOCKTYPE.vegetables:
                        moneyManager.currMoney -= (int)moneyManager.vegQuality;
                        break;
                    case ShelfHandler.STOCKTYPE.cans:
                        moneyManager.currMoney -= (int)moneyManager.cansQuality;
                        break;
                    case ShelfHandler.STOCKTYPE.bread:
                        moneyManager.currMoney -= (int)moneyManager.breadQuality;
                        break;
                }

                energy -= stackEnergyLoss;
                stackTimer = 0;
            }
            else
            {
                stackTimer += Time.deltaTime;
            }
        }
        else
        {
            stockingObject.spotFilled = ResetToIdle();
        }

        if (energy < minEnergyToWork || Vector3.Distance(transform.position, stockingObject.GetStandingSpot().position) > 3)
        {
            stockingObject.spotFilled = ResetToIdle();
        }
    }

    private bool ResetToIdle()
    {
        employeeState = EMPLOYEESTATE.idle;
        currActive = CURRACTIVE.none;
        activeObject = null;
        return false;
    }

    private void EmployeeRegister()
    {
        RegisterHolder register = activeObject.GetComponent<RegisterHolder>();
        register.employee = gameObject;

        if (register.customer != null)
        {
            CustomerAI customer = register.customer.GetComponent<CustomerAI>();
            if (customer.totalGotten > 0)
            {
                if (registerTimer > registerTimerLimit)
                {
                    if (customer.breadGotten > 0)
                    {
                        customer.breadGotten -= 1;
                        moneyManager.currMoney += (int)moneyManager.breadQuality + ((int)moneyManager.breadQuality * (moneyManager.breadProfitPercentage / 100));
                    }
                    else if (customer.vegGotten > 0)
                    {
                        customer.vegGotten -= 1;
                        moneyManager.currMoney += (int)moneyManager.vegQuality + ((int)moneyManager.vegProfitPercentage * (moneyManager.vegProfitPercentage / 100));
                    }
                    else if (customer.canGotten > 0)
                    {
                        customer.canGotten -= 1;
                        moneyManager.currMoney += (int)moneyManager.vegQuality + ((int)moneyManager.vegQuality * (moneyManager.vegProfitPercentage / 100));
                    }
                    energy -= registerEnergyLoss;
                    registerTimer = 0;
                }
                else
                {
                    registerTimer += Time.deltaTime;
                }
            }
            else
            {
                customer.FinishedShopping();
            }
        }
        if (energy < minEnergyToWork || comMaster.totalCustomers < comMaster.GetFreeCusRegisters().Count || Vector3.Distance(transform.position, register.GetEmployeeSpot().position) > 3)
        {
            register.employeeFilled = ResetToIdle();
        }
    }

    private void EmployeeClean()
    {

    }

    private void EmployeeWalk()
    {
        if (path.status != NavMeshPathStatus.PathComplete)
        {
            employeeState = EMPLOYEESTATE.idle;
            return;
        }
        if (Vector3.Distance(transform.position, agent.destination) < 1.1f)
        {
            switch (currActive)
            {
                case CURRACTIVE.none:
                    employeeState = EMPLOYEESTATE.idle;
                    break;
                case CURRACTIVE.register:
                    employeeState = EMPLOYEESTATE.register;
                    break;
                case CURRACTIVE.chair:
                    employeeState = EMPLOYEESTATE.rest;
                    break;
                case CURRACTIVE.shelf:
                    employeeState = EMPLOYEESTATE.stack;
                    break;
            }

        }

        if (energyCooldown < 0)
        {
            energy -= walkingEnergyLoss;
            energyCooldown = energyCooldownDefault;
        }
        else
        {
            energyCooldown -= Time.deltaTime;
        }
    }
}
