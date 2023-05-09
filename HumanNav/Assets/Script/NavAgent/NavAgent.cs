using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavManager;
using UnityEngine.AI;
using System.Linq;

public class NavAgent : MonoBehaviour
{

    private NavAgentManager nav;
    private NavMeshAgent agent;
    private NavMeshPath path;
    private Cell[,] memory;
    private Cell[,] alreadyVisited;
    private Cell[,] weightsBeforeMemory;
    private Cell[,] finalWeights;
    private float averageWeight;
    public GameObject destination;
    private Cell destinationCell;
    private bool destinationFound;
    private Cell dummyCell;
    public Vector3 startPos; 

    [SerializeField] private float memoryUpdatePeriod;
    [SerializeField] private float visionAngle;
    [SerializeField] private float visionDistance;

    private Vector2 subDestination;
    private Cell subDestinationCell;

    private void Start()
    {
        startPos = transform.position; 
        subDestinationCell = dummyCell = new Cell(new Vector2(-Mathf.Infinity, -Mathf.Infinity), -100, new Vector2Int(-100, -100));
        nav = GameObject.FindObjectOfType<NavAgentManager>();
        memory = nav.NewCellArray();
        weightsBeforeMemory = nav.NewCellArray();
        agent = GetComponent<NavMeshAgent>();
        alreadyVisited = nav.NewCellArray();

        finalWeights = nav.NewCellArray();
        for (int i = 0; i < nav.cellCount.x; i++)
        {
            for (int j = 0; j < nav.cellCount.y; j++)
            {
                alreadyVisited[i, j].weight = 100;
            }
        }
        if (destination == null && GameObject.FindGameObjectWithTag("Destination") != null)
        {
            destination = GameObject.FindGameObjectWithTag("Destination");
            destinationCell = nav.ClosestCellToPosition(new Vector2(destination.transform.position.x, destination.transform.position.z));
        }
        path = new NavMeshPath();

        StartCoroutine(UpdateMemory());
    }

    public void ResetAgent()
    {
        for (int i = 0; i < nav.cellCount.x; i++)
        {
            for (int j = 0; j < nav.cellCount.y; j++)
            {
                alreadyVisited[i, j].weight = 100;
            }
        }
        destinationFound = false; 

        subDestinationCell = dummyCell;
        subDestination = Vector2.positiveInfinity;

    }

    IEnumerator UpdateMemory()
    {
        StartCoroutine(SetUpWeights());

        for (int i = 0; i < nav.cellCount.x; i++)
        {
            for (int j = 0; j < nav.cellCount.y; j++)
            {
                if (memory[i, j].weight > 0)
                {
                    memory[i, j].weight -= nav.retentions[i, j].incrementMemory();
                }
                if (Vector2.Distance(memory[i, j].position, new Vector2(transform.position.x, transform.position.z)) < visionDistance)
                {
                    if (InsideVisionCone(memory[i, j].position3))
                    {
                        if (!BehindObstacle(memory[i, j].position3))
                        {
                            memory[i, j].weight = nav.retentions[i, j].viewedCell();
                            if (destinationCell.cellPos == new Vector2(i, j))
                            {
                                agent.CalculatePath(destination.transform.position, path);
                                agent.SetPath(path);
                                destinationFound = true;
                            }
                        }
                    }
                }
            }
        }

        if (!destinationFound)
        {
           CheckSubdestination();
        }
        yield return new WaitForSeconds(memoryUpdatePeriod);
        StartCoroutine(UpdateMemory());
    }

    private bool InsideVisionCone(Vector3 point)
    {
        return Mathf.Abs(Vector3.SignedAngle(transform.forward, point - transform.position, Vector3.up)) < visionAngle;
    }

    private bool BehindObstacle(Vector3 point)
    {
        LayerMask layerMask = 1 << LayerMask.NameToLayer("Deletion Ignore");

        Ray ray = new Ray(new Vector3(transform.position.x, 1, transform.position.z),
            new Vector3(point.x, 1, point.z) - new Vector3(transform.position.x, 1, transform.position.z));

        return Physics.Raycast(ray, Vector3.Distance(point, transform.position), ~layerMask);
    }

    private void CheckSubdestination()
    {
        Vector2 pos2 = new Vector2(transform.position.x, transform.position.z);
        Vector2 forward2 = new Vector2(transform.forward.x, transform.forward.z); 


        for (int i = 0; i < nav.cellCount.x; i++)
        {
            for (int j = 0; j < nav.cellCount.y; j++)
            {
                finalWeights[i, j].weight = memory[i, j].weight * weightsBeforeMemory[i, j].weight * alreadyVisited[i, j].weight * 
                    Mathf.Clamp(Mathf.Cos(4 * Vector2.Angle(forward2, finalWeights[i, j].position - pos2) * Mathf.Deg2Rad), 0.5f, 1f) *
                      Mathf.Clamp(Mathf.Cos(Vector2.Angle(forward2, finalWeights[i, j].position - pos2) * Mathf.Deg2Rad), 0.5f, 1f) ;
            }
        }


        Cell currCell = nav.ClosestCellToPosition(new Vector2(transform.position.x, transform.position.z));
        float currWeight = finalWeights[currCell.cellPos.x, currCell.cellPos.y].weight;


        averageWeight = finalWeights.Cast<Cell>().Where(item => memory[item.cellPos.x, item.cellPos.y].weight > 0).Select(item => item.weight).Average();




        if (subDestinationCell == dummyCell)
        {
            SetSubdestination();

        }
        if (averageWeight > finalWeights[subDestinationCell.cellPos.x, subDestinationCell.cellPos.y].weight || agent.remainingDistance < 1)
        {
            SetSubdestination();
        }

    }


    private void SetSubdestination()
    {
        List<Cell> smallerWeightsList = new List<Cell>(finalWeights.Cast<Cell>().Where(item => item.weight >= averageWeight).ToList());

        subDestinationCell = smallerWeightsList[GetRandomWeightedIndex(smallerWeightsList.Select(item => (int)item.weight).ToArray())];
        subDestination = subDestinationCell.position3;

        agent.CalculatePath(subDestinationCell.position3, path);
        agent.SetPath(path);
    }


    // from https://forum.unity.com/threads/random-numbers-with-a-weighted-chance.442190/, from user AndyGainey 
    public int GetRandomWeightedIndex(int[] weights)
    {

        int weightSum = 0;
        for (int i = 0; i < weights.Count(); ++i)
        {
            weightSum += weights[i];
        }

        int index = 0;
        int lastIndex = weights.Count() - 1;
        while (index < lastIndex)
        {
 
            if (Random.Range(0, weightSum) < weights[index])
            {
                return index;
            }

            weightSum -= weights[index++];
        }
        return index;
    }

    public IEnumerator SetUpWeights()
    {
        Cell dest = nav.ClosestCellToPosition(new Vector2(destination.transform.position.x, destination.transform.position.z));

        for (int i = 0; i < nav.cellCount.x; i++)
        {
            for (int j = 0; j < nav.cellCount.y; j++)
            {
                weightsBeforeMemory[i, j].weight = 1;
                if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), alreadyVisited[i, j].position) < 2)
                {
                    alreadyVisited[i, j].weight /= 2;
                }
            }
        }

        foreach (Landmark landmark in nav.landmarks)
        {
            if (memory[landmark.cellPos.x, landmark.cellPos.y].weight > 0)
            {
                foreach (Cell cell in landmark.affectedCells)
                {
                    weightsBeforeMemory[cell.cellPos.x, cell.cellPos.y].weight += cell.weight * (landmark.cells[dest.cellPos.x, dest.cellPos.y].weight + 1) *
                       (nav.obstacles[cell.cellPos.x, cell.cellPos.y].weight / 100) ;
                }
            }
        }

        foreach (Edge edge in nav.edges)
        {
            foreach (Cell cell in edge.side1AffectedCells)
            {
                weightsBeforeMemory[cell.cellPos.x, cell.cellPos.y].weight += cell.weight * (edge.side1[dest.cellPos.x, dest.cellPos.y].weight + 1) *
                         (nav.obstacles[cell.cellPos.x, cell.cellPos.y].weight / 100);
            }
            foreach (Cell cell in edge.side2AffectedCells)
            {
                weightsBeforeMemory[cell.cellPos.x, cell.cellPos.y].weight += cell.weight * (edge.side2[dest.cellPos.x, dest.cellPos.y].weight + 1) *
                         (nav.obstacles[cell.cellPos.x, cell.cellPos.y].weight / 100);
            }
        }

        foreach (District district in nav.districts)
        {
            foreach (Cell cell in district.affectedCells)
            {
                weightsBeforeMemory[cell.cellPos.x, cell.cellPos.y].weight += cell.weight * (district.centerLandmark.cells[dest.cellPos.x, dest.cellPos.y].weight + 1) *
                        (nav.obstacles[cell.cellPos.x, cell.cellPos.y].weight / 100);
            }
        }

        /*
                 weightsBeforeMemory[i, j].weight = 1;
                 foreach (Landmark landmark in nav.landmarks)
                 {
                     if (memory[landmark.cellPos.x, landmark.cellPos.y].weight > 0)
                     {
                         weightsBeforeMemory[i, j].weight += Mathf.Pow(landmark.cells[i, j].weight, 1.2f) * Mathf.Pow(1 / Vector3.Distance(cell.position, landmark.position) + 10, 2);
                     }
                 }

                 foreach(Edge edge in nav.edges)
                 {

                 }

                 foreach (District district in nav.districts)
                 {

                 }

             }
         }*/

        yield return null;

    }
}
