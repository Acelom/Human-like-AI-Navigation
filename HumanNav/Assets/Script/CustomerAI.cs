using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomerItems;
using CommonItems;
using UnityEngine.AI;
using System.Linq;

public class CustomerAI : MonoBehaviour
{
    public enum CUSTOMERSTATE
    {
        idle,
        rest,
        register,
        take,
        walk,
    }
    public CUSTOMERSTATE customerState = CUSTOMERSTATE.idle;
    [SerializeField] private float walkDistance;
    [SerializeField] private float energy;
    [SerializeField] private float energyLoss;
    [SerializeField] private float energyMax;

    private float restingTimer;
    [SerializeField] private float restingTimerLimit;
    [SerializeField] private float restingGain;

    private float energyCooldown;
    [SerializeField] private float energyCooldownDefault;

    private float idleTimer;
    [SerializeField] private float idleTimerLimit;

    private float takeTimer;
    [SerializeField] private float takeTimerLimit;

    private bool leaving;

    public int vegNeeded = 0;
    public int canNeeded = 0;
    public int breadNeeded = 0;
    public int totalNeed;

    public int vegGotten = 0;
    public int canGotten = 0;
    public int breadGotten = 0;
    public int totalGotten;

    private enum CURRACTIVE
    {
        none,
        register,
        shelf,
        exit,
    };
    private CURRACTIVE currActive = CURRACTIVE.none;
    private GameObject activeObject;
    private CustomerMaster cusMaster;
    private CommonMaster comMaster;

    private NavMeshAgent agent;
    private NavMeshPath path;

    private void Awake()
    {
        cusMaster = GameObject.FindGameObjectWithTag("GameController").GetComponent<CustomerMaster>();
        comMaster = GameObject.FindGameObjectWithTag("GameController").GetComponent<CommonMaster>();
        energy = energyMax;
        energyCooldown = energyCooldownDefault;
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
    }

    private void Update()
    {
        totalNeed = breadNeeded + canNeeded + vegNeeded;
        totalGotten = breadGotten + canGotten + vegGotten;


        switch (customerState)
        {
            case CUSTOMERSTATE.idle:
                CustomerIdle();
                break;
            case CUSTOMERSTATE.register:
                CustomerRegister();
                break;
            case CUSTOMERSTATE.take:
                CustomerTake();
                break;
            case CUSTOMERSTATE.walk:
                CustomerWalk();
                break;
        }
    }

    private void CustomerIdle()
    {
        if (currActive == CURRACTIVE.none)
        {
            Vector3 pos = Vector3.positiveInfinity;
            if (energy < 5)
            {
                FinishedShopping();
            }
            else
            {
                if (comMaster.GetCusRegisterAnyFree() &&
                    (totalGotten >= totalNeed * (energy / energyMax) || totalNeed == totalGotten))
                {
                    pos = RouteToRegister();
                }
                else if (totalNeed > totalGotten)
                {
                    pos = RouteToShelf();
                }
            }
            if (pos.magnitude < new Vector3(500, 500, 500).magnitude)
            {
                agent.CalculatePath(pos, path);
            }
            if (path.status != NavMeshPathStatus.PathComplete || pos.magnitude > new Vector3(500, 500, 500).magnitude)
            {
                switch (currActive)
                {
                    case CURRACTIVE.none:
                        break;
                    case CURRACTIVE.register:
                        activeObject.GetComponent<RegisterHolder>().customerFilled = false;
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
                customerState = CUSTOMERSTATE.walk;
                idleTimer = 0;
                return;
            }
        }

        if (idleTimerLimit < idleTimer)
        {
            Vector3 pos = Vector3.zero;
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
            } while (path.status == NavMeshPathStatus.PathPartial || pos.magnitude > new Vector3(500, 500, 500).magnitude);
            agent.SetPath(path);
            idleTimer = 0;
            customerState = CUSTOMERSTATE.walk;
        }
        else
        {
            idleTimer += Time.deltaTime;
        }
    }

    private Vector3 RouteToRegister()
    {
        List<GameObject> registers = new List<GameObject>(comMaster.GetFreeCusRegisters());
        float distance = Mathf.Infinity;
        GameObject chosenRegister = null;
        foreach (GameObject register in registers)
        {
            if (Vector3.Distance(transform.position, register.transform.position) < distance)
            {
                distance = Vector3.Distance(transform.position, register.transform.position);
                chosenRegister = register;
            }
        }
        if (chosenRegister != null)
        {
            activeObject = chosenRegister;
            activeObject.GetComponent<RegisterHolder>().customerFilled = true;
            currActive = CURRACTIVE.register;
            return activeObject.GetComponent<RegisterHolder>().GetCustomerSpot().position;
        }
        return Vector3.positiveInfinity;
    }

    private Vector3 RouteToShelf()
    {

        List<GameObject> shelfs = new List<GameObject>(comMaster.GetFreeShelfs());
        List<GameObject> validShelfs = new List<GameObject>(shelfs.Where(item => item.GetComponent<ShelfHandler>().currStock != 0));


        List<GameObject> breadShelfs = new List<GameObject>();
        List<GameObject> canShelfs = new List<GameObject>();
        List<GameObject> vegShelfs = new List<GameObject>();
        List<GameObject> finalShelfList = new List<GameObject>();


        if (breadNeeded > breadGotten)
        {
            breadShelfs = validShelfs.Where(item => item.GetComponent<ShelfHandler>().stockType == ShelfHandler.STOCKTYPE.bread).ToList();
            finalShelfList.AddRange(breadShelfs);
        }
        if (vegNeeded > vegGotten)
        {
            vegShelfs = validShelfs.Where(item => item.GetComponent<ShelfHandler>().stockType == ShelfHandler.STOCKTYPE.vegetables).ToList();
            finalShelfList.AddRange(vegShelfs);
        }
        if (canNeeded > canGotten)
        {
            canShelfs = validShelfs.Where(item => item.GetComponent<ShelfHandler>().stockType == ShelfHandler.STOCKTYPE.cans).ToList();
            finalShelfList.AddRange(canShelfs);
        }


        if (finalShelfList.Count > 0)
        {
            float distance = Mathf.Infinity;
            GameObject chosenShelf = null;

            foreach (GameObject shelf in finalShelfList)
            {
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

    private void CustomerRest()
    {

    }

    private void CustomerTake()
    {
        ShelfHandler currShelf = activeObject.GetComponent<ShelfHandler>();
        bool done = false;

        if (takeTimer > takeTimerLimit)
        {
            switch (currShelf.stockType)
            {
                case ShelfHandler.STOCKTYPE.none:
                    break;
                case ShelfHandler.STOCKTYPE.vegetables:
                    vegGotten += 1;
                    if (vegGotten >= vegNeeded)
                    {
                        done = true;
                    }
                    break;
                case ShelfHandler.STOCKTYPE.cans:
                    canGotten += 1;
                    if (canGotten >= canNeeded)
                    {
                        done = true;
                    }
                    break;
                case ShelfHandler.STOCKTYPE.bread:
                    breadGotten += 1;
                    if (breadGotten >= breadNeeded)
                    {
                        done = true;
                    }
                    break;
            }
            currShelf.currStock -= 1;
            takeTimer = 0;
        }
        else
        {
            takeTimer += Time.deltaTime;
        }

        if (Vector3.Distance(transform.position, currShelf.GetStandingSpot().position) > 3)
        {
            done = true;
        }

        if (currShelf.currStock == 0)
        {
            done = true;
        }

        if (done)
        {
            currActive = CURRACTIVE.none;
            customerState = CUSTOMERSTATE.idle;
            activeObject = null;
            currShelf.spotFilled = false;
        }

    }

    private void CustomerRegister()
    {
        activeObject.GetComponent<RegisterHolder>().customer = gameObject;
    }

    private void CustomerWalk()
    {
        if (path.status != NavMeshPathStatus.PathComplete)
        {
            customerState = CUSTOMERSTATE.idle;
            return;
        }
        if (Vector3.Distance(transform.position, agent.destination) < 1.1f)
        {
            switch (currActive)
            {
                case CURRACTIVE.none:
                    customerState = CUSTOMERSTATE.idle;
                    break;
                case CURRACTIVE.register:
                    customerState = CUSTOMERSTATE.register;
                    break;
                case CURRACTIVE.shelf:
                    customerState = CUSTOMERSTATE.take;
                    break;
                case CURRACTIVE.exit:
                    GameObject.FindObjectOfType<AgentSpawner>().customers.Remove(gameObject);
                    Destroy(gameObject);
                    break;
            }

        }

        if (energyCooldown < 0)
        {
            energy -= energyLoss;
            energyCooldown = energyCooldownDefault;
        }
        else
        {
            energyCooldown -= Time.deltaTime;
        }
    }

    public void FinishedShopping()
    {
        if (activeObject != null)
        {
            if (activeObject.TryGetComponent(out ShelfHandler oldShelf))
            {
                oldShelf.spotFilled = false;
            }
            else if (activeObject.TryGetComponent(out RegisterHolder oldRegister))
            {
                oldRegister.customerFilled = false;
                oldRegister.customer = null;
            }
        }


        Vector3 pos = GameObject.FindGameObjectWithTag("Exit").transform.position;
        currActive = CURRACTIVE.exit;
        agent.CalculatePath(pos, path);
        agent.SetPath(path);
        customerState = CUSTOMERSTATE.walk;
    }

}
