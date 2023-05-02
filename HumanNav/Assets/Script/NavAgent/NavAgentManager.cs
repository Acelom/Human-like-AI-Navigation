using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;


namespace NavManager
{
    public class Cell
    {
        public Vector2 position = new();
        public Vector3 position3;
        public float weight;
        public Vector2Int cellPos = new();

        public Cell(Vector2 position, int weight, Vector2Int cellPos)
        {
            this.position = position;
            position3 = new Vector3(position.x, 0, position.y);
            this.weight = weight;
            this.cellPos = cellPos;
        }
        public Cell(Cell cell)
        {
            this.position = cell.position;
            this.position3 = cell.position3;
            this.weight = cell.weight;
            this.cellPos = cell.cellPos;
        }

    }

    public class MemoryRetention
    {
        public int defaultMemory;

        public int memoryDecrease;

        public int decreaseCountdown;
        public int decreaseCountdownDefault;

        NavAgentManager nav = GameObject.FindObjectOfType<NavAgentManager>();


        public Vector2Int cellLocation = new();

        public MemoryRetention() { }

        public MemoryRetention(int defaultMemory, int memoryDecrease, int decreaseCountdownDefault, Vector2Int cellLocation)
        {
            this.defaultMemory = defaultMemory;
            this.memoryDecrease = memoryDecrease;
            this.decreaseCountdownDefault = decreaseCountdownDefault;
            this.decreaseCountdown = decreaseCountdownDefault;
            this.cellLocation = cellLocation;
        }

        public int viewedCell()
        {
            return Mathf.Clamp(defaultMemory + nav.additionalRetentions[cellLocation.x, cellLocation.y].defaultMemory, 0, 100);
        }

        public int incrementMemory()
        {
            if (decreaseCountdown <= 0)
            {
                decreaseCountdown = decreaseCountdownDefault + nav.additionalRetentions[cellLocation.x, cellLocation.y].decreaseCountdownDefault;
                return memoryDecrease;
            }
            else
            {
                decreaseCountdown -= 1;
                return 0;
            }
        }
    }

    public class Landmark
    {
        public Vector2 position;
        public GameObject obj;
        public Cell[,] cells;
        public Cell[,] cellsForDistricts;
        public Cell[,] smallerCellsForDistricts;
        public Vector2Int cellPos;
        NavAgentManager nav = GameObject.FindObjectOfType<NavAgentManager>();
        public List<Cell> affectedCells = new();
        public List<Cell> affectedCellsForDistricts = new();
        public List<Cell> smallerAffectedCellsForDistricts = new();


        public Landmark(GameObject obj)
        {
            this.obj = obj;
            position = new Vector2(obj.transform.position.x, obj.transform.position.z);
            cellPos = nav.ClosestCellToPosition(this.position).cellPos;
        }

        public void UpdateCells(int range)
        {
            cells = nav.CreepingWave(position, range, 1, false);

            affectedCells = new();

            for (int i = 0; i < nav.cellCount.x; i++)
            {
                for (int j = 0; j < nav.cellCount.y; j++)
                {
                    if (cells[i, j].weight > 0)
                    {
                        affectedCells.Add(cells[i, j]);
                    }
                }
            }
            affectedCells = affectedCells.Distinct().ToList(); 
        }

        public void UpdateCellsForDistricts(int range)
        {
            cellsForDistricts = nav.CreepingWave(position, range, 1, true);
            smallerCellsForDistricts = nav.CreepingWave(position, range, 2, true);
            affectedCellsForDistricts = new();
            smallerAffectedCellsForDistricts = new();
            for (int i = 0; i < nav.cellCount.x; i++)
            {
                for (int j = 0; j < nav.cellCount.y; j++)
                {
                    if (cellsForDistricts[i, j].weight > 0)
                    {
                        affectedCellsForDistricts.Add(cellsForDistricts[i, j]);
                    }
                    if (smallerCellsForDistricts[i, j].weight > 0)
                    {
                        smallerAffectedCellsForDistricts.Add(smallerCellsForDistricts[i, j]);
                    }
                }
            }

            affectedCellsForDistricts = affectedCellsForDistricts.Distinct().ToList();
            smallerAffectedCellsForDistricts = smallerAffectedCellsForDistricts.Distinct().ToList();
        }
    }
    public class Edge
    {
        public Vector2 center;
        public Vector2Int cellPos;
        public float rotation;
        public float length;
        public GameObject obj;
        public int range;
        private NavAgentManager nav = GameObject.FindObjectOfType<NavAgentManager>();
        public Vector2 edgeEnd;
        public Vector2 edgeStart;
        public List<Cell> edgeCells = new();
        public Cell[,] side1;
        public Cell[,] side2;
        public Vector2 edgeDirection = new();
        public List<Cell> side1AffectedCells = new();
        public List<Cell> side2AffectedCells = new();

        public Edge(GameObject obj, int range)
        {
            this.obj = obj;
            this.center = new Vector2(obj.transform.position.x, obj.transform.position.z);
            this.rotation = Mathf.Repeat(obj.transform.eulerAngles.y, 360) - 180;
            this.length = obj.transform.localScale.z;
            this.range = range + Mathf.RoundToInt(obj.transform.localScale.x);
            Cell temp = nav.ClosestCellToPosition(this.center);
            this.cellPos = new Vector2Int(temp.cellPos.x, temp.cellPos.y);
        }

        public void UpdateSides()
        {
            side1 = nav.NewCellArray();
            side2 = nav.NewCellArray();
            side1AffectedCells = new();
            side2AffectedCells = new();

            edgeDirection = new Vector2(obj.transform.forward.x, obj.transform.forward.z);
            Vector2 edgeNormal = Vector2.Perpendicular(edgeDirection);

            edgeEnd = center + edgeDirection * (length / 2);
            edgeStart = edgeEnd - edgeDirection * length;

            Cell startCell = nav.ClosestCellToPosition(edgeStart);
            edgeCells = new();
            GameObject go = Resources.Load<GameObject>("Prefabs/DebugCube");
            for (int i = 0; i < length; i++)
            {
                Cell temp = nav.cells[startCell.cellPos.x + Mathf.RoundToInt(edgeDirection.x * i), startCell.cellPos.y + Mathf.RoundToInt(edgeDirection.y * i)];
                edgeCells.Add(temp);
                side1[temp.cellPos.x, temp.cellPos.y].weight = range;
                side2[temp.cellPos.x, temp.cellPos.y].weight = range;
            }
            bool checkOnce = false;

            foreach (Cell cell in edgeCells)
            {
                checkOnce = false;
                for (int i = 0; i < range; i++)
                {
                    Cell temp = nav.cells[cell.cellPos.x + Mathf.FloorToInt(edgeNormal.x * i), cell.cellPos.y + Mathf.FloorToInt(edgeNormal.y * i)];
                    Cell temp2 = nav.cells[cell.cellPos.x + Mathf.CeilToInt(edgeNormal.x * i), cell.cellPos.y + Mathf.CeilToInt(edgeNormal.y * i)];
                    if (nav.obstacles[temp.cellPos.x, temp.cellPos.y].weight == 100)
                    {
                        side1[temp.cellPos.x, temp.cellPos.y].weight = range - i;
                        side1[temp2.cellPos.x, temp2.cellPos.y].weight = range - i;
                        side1AffectedCells.Add(side1[temp.cellPos.x, temp.cellPos.y]);
                        side2AffectedCells.Add(side1[temp2.cellPos.x, temp2.cellPos.y]);

                        // GameObject.Instantiate(go, temp.position3 + Vector3.up * side1[temp.cellPos.x, temp.cellPos.y].weight, Quaternion.identity);
                        //GameObject.Instantiate(go, temp2.position3 + Vector3.up * side1[temp.cellPos.x, temp.cellPos.y].weight, Quaternion.identity);
                    }
                    else
                    {

                        if (checkOnce)
                        {
                            break;
                        }
                        checkOnce = true;
                    }
                }
                checkOnce = false;
                for (int i = 0; i < range; i++)
                {
                    Cell temp = nav.cells[cell.cellPos.x - Mathf.FloorToInt(edgeNormal.x * i), cell.cellPos.y - Mathf.FloorToInt(edgeNormal.y * i)];
                    Cell temp2 = nav.cells[cell.cellPos.x - Mathf.CeilToInt(edgeNormal.x * i), cell.cellPos.y - Mathf.CeilToInt(edgeNormal.y * i)];
                    if (nav.obstacles[temp.cellPos.x, temp.cellPos.y].weight == 100)
                    {
                        side2[temp.cellPos.x, temp.cellPos.y].weight = range - i;
                        side2[temp2.cellPos.x, temp2.cellPos.y].weight = range - i;
                        side2AffectedCells.Add(side2[temp.cellPos.x, temp.cellPos.y]);
                        side2AffectedCells.Add(side2[temp2.cellPos.x, temp2.cellPos.y]);
                        //GameObject.Instantiate(go, temp.position3 + Vector3.up * side2[temp.cellPos.x, temp.cellPos.y].weight, Quaternion.identity);
                        //GameObject.Instantiate(go, temp2.position3 + Vector3.up * side2[temp.cellPos.x, temp.cellPos.y].weight, Quaternion.identity);
                    }
                    else
                    {
                        if (checkOnce)
                        {
                            break;
                        }
                        checkOnce = true;
                    }
                }
            }
            side1AffectedCells = side1AffectedCells.Distinct().ToList();
            side2AffectedCells = side2AffectedCells.Distinct().ToList(); 
        }
    }

    public class District
    {
        public Landmark centerLandmark;
        public List<Landmark> containingLandmarks = new List<Landmark>();
        NavAgentManager nav = GameObject.FindObjectOfType<NavAgentManager>();

        public Cell[,] cells;
        public List<Cell> affectedCells = new();

        public District(Landmark landmark)
        {
            this.centerLandmark = landmark;
            UpdateDistrict();
        }

        public void UpdateDistrict()
        {
            cells = nav.NewCellArray();
            centerLandmark.UpdateCellsForDistricts(30 / nav.landmarks.Where(item => item.obj.GetComponent<LandmarkHandler>().landmarkType
                == centerLandmark.obj.GetComponent<LandmarkHandler>().landmarkType).Count() + 10);
            cells = centerLandmark.cellsForDistricts;
            containingLandmarks = new List<Landmark>();
            affectedCells = new List<Cell>(centerLandmark.affectedCellsForDistricts);
            foreach (Landmark landmark in nav.landmarks)
            {
                if (!nav.districts.Any(item => item.containingLandmarks.Contains(landmark)))
                {
                    if (landmark != centerLandmark && centerLandmark.cellsForDistricts[landmark.cellPos.x, landmark.cellPos.y].weight > 0 && !containingLandmarks.Contains(landmark))
                    {
                        containingLandmarks.Add(landmark);
                        landmark.UpdateCellsForDistricts((int)centerLandmark.cellsForDistricts[landmark.cellPos.x, landmark.cellPos.y].weight);
                        affectedCells.AddRange(landmark.smallerAffectedCellsForDistricts);
                    }
                }
            }
            affectedCells = affectedCells.Distinct().ToList(); 

            foreach (Cell cell in affectedCells)
            {
                cells[cell.cellPos.x, cell.cellPos.y].weight += cell.weight;
            }
        }

    }


    public class NavAgentManager : MonoBehaviour
    {
        public GameObject floor;
        private Vector2 min;
        private Vector2 max;
        private Vector2 size;

        public Cell[,] cells;
        public MemoryRetention[,] retentions;
        public MemoryRetention[,] additionalRetentions;

        public Cell[,] obstacles;
        public List<Landmark> landmarks;
        public List<Edge> edges;
        public List<District> districts;


        [SerializeField] private float obstacleUpdatePeriod;

        private enum QUALITY
        {
            low,
            medium,
            high,
            max,
        }


        [SerializeField] private QUALITY cellSizeQuality;
        public Vector2 cellSize;
        public Vector2Int cellCount;

        private void Awake()
        {
            floor = GameObject.FindObjectOfType<NavMeshSurface>().gameObject;
            max = new Vector2(floor.GetComponent<BoxCollider>().bounds.max.x, floor.GetComponent<BoxCollider>().bounds.max.z);
            min = new Vector2(floor.GetComponent<BoxCollider>().bounds.min.x, floor.GetComponent<BoxCollider>().bounds.min.z);
            size = new Vector2(max.x - min.x, max.y - min.y);
            landmarks = new List<Landmark>();
            edges = new();
            districts = new();

            cellCount = new Vector2Int(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));
            cellSize = new Vector2(1, 1);
            switch (cellSizeQuality)
            {
                case QUALITY.low:
                    cellCount = cellCount / 8;
                    cellSize = cellSize * 8;
                    break;
                case QUALITY.medium:
                    cellCount = cellCount / 4;
                    cellSize = cellSize * 4;
                    break;
                case QUALITY.high:
                    cellCount = cellCount / 2;
                    cellSize = cellSize * 2;
                    break;
                case QUALITY.max:
                    break;
            }

            cells = new Cell[cellCount.x, cellCount.y];
            retentions = new MemoryRetention[cellCount.x, cellCount.y];
            additionalRetentions = new MemoryRetention[cellCount.x, cellCount.y];
            obstacles = new Cell[cellCount.x, cellCount.y];

            cells = NewCellArray();

            for (int i = 0; i < cellCount.x; i++)
            {
                for (int j = 0; j < cellCount.y; j++)
                {
                    retentions[i, j] = new MemoryRetention(50, 1, 10, new Vector2Int(i, j));
                    additionalRetentions[i, j] = new MemoryRetention(0, 0, 0, new Vector2Int(i, j));
                }
            }

            obstacles = NewCellArray();

            StartCoroutine(UpdateObstacles());
        }

        public Cell[,] NewCellArray()
        {
            Cell[,] finalArray = new Cell[cellCount.x, cellCount.y];

            Vector2 centerCorrection = new Vector2(floor.transform.position.x - floor.transform.localScale.x / 2 + cellSize.x / 2,
            floor.transform.position.z - floor.transform.localScale.z / 2 + cellSize.y / 2);
            for (int i = 0; i < cellCount.x; i++)
            {
                for (int j = 0; j < cellCount.y; j++)
                {
                    finalArray[i, j] = new Cell(new Vector2(i * cellSize.x, j * cellSize.y) + centerCorrection, 0, new Vector2Int(i, j));
                }
            }

            return finalArray;
        }

        public IEnumerator UpdateObstacles()
        {
            for (int i = 0; i < cellCount.x; i++)
            {
                for (int j = 0; j < cellCount.y; j++)
                {
                    NavMeshHit hit;
                    obstacles[i, j].weight = 0;
                    if (NavMesh.SamplePosition(obstacles[i, j].position3, out hit, 0.1f, NavMesh.AllAreas))
                    {
                        obstacles[i, j].weight = 100;
                    }
                    /*  else
                      {
                     GameObject obj = Resources.Load<GameObject>("Prefabs/DebugCube");
                          obstacles[i, j].weight = 0;
                          GameObject go = Instantiate(obj);
                          go.transform.position = new Vector3(obstacles[i, j].position.x, 1, obstacles[i, j].position.y);
                      }*/
                }
            }
            yield return new WaitForSeconds(obstacleUpdatePeriod);
            StartCoroutine(UpdateLandmarks());
            yield return new WaitForSeconds(obstacleUpdatePeriod);
            StartCoroutine(UpdateEdges());
            yield return new WaitForSeconds(obstacleUpdatePeriod);
            StartCoroutine(UpdateDistricts());

        }

        public Cell[,] CreepingWave(Vector2 position, int weightStartValue, int weightDecreaseValue, bool goThroughObstacles)
        {
            Cell[,] wave = new Cell[cellCount.x, cellCount.y];
            wave = NewCellArray();

            Cell startingCell = ClosestCellToPosition(position);
            wave[startingCell.cellPos.x, startingCell.cellPos.y].weight = weightStartValue;
            List<Cell> outsideList = new List<Cell>();
            outsideList.Add(startingCell);

            do
            {
                weightStartValue -= weightDecreaseValue;
                List<Cell> newOutsideList = new List<Cell>();
                foreach (Cell cell in outsideList)
                {
                    for (int i = -1; i < 2; i++)
                    {
                        if (cell.cellPos.x + i >= 0 && cell.cellPos.x + i <= cellCount.x - 1)
                        {
                            for (int j = -1; j < 2; j++)
                            {
                                if (cell.cellPos.y + j >= 0 && cell.cellPos.y + j <= cellCount.y - 1)
                                {
                                    if (wave[cell.cellPos.x + i, cell.cellPos.y + j].weight < weightStartValue && (
                                    obstacles[cell.cellPos.x + i, cell.cellPos.y + j].weight != 0 || goThroughObstacles == true))
                                    {
                                        wave[cell.cellPos.x + i, cell.cellPos.y + j].weight = weightStartValue;
                                        newOutsideList.Add(wave[cell.cellPos.x + i, cell.cellPos.y + j]);
                                    }
                                }
                            }
                        }
                    }
                }
                outsideList = new List<Cell>(newOutsideList);

            } while (weightStartValue > 0);
            return wave;
        }

        public Cell ClosestCellToPosition(Vector2 position)
        {
            Cell[] longList = new Cell[cellCount.x * cellCount.y];
            longList = NewCellArray().Cast<Cell>().ToArray();
            return longList.OrderBy(item => Vector2.Distance(item.position, position)).FirstOrDefault();
        }


        private IEnumerator UpdateLandmarks()
        {
            landmarks = new();

            List<LandmarkHandler> landmarksInLevel = new List<LandmarkHandler>(GameObject.FindObjectsOfType<LandmarkHandler>());
            foreach (LandmarkHandler landmark in landmarksInLevel)
            {
                if (landmark.enabled)
                {
                    Vector2 pos = new Vector2(landmark.transform.position.x, landmark.transform.position.z);
                    landmarks.Add(new Landmark(landmark.gameObject));
                }
            }


            foreach (Landmark landmark in landmarks)
            {
                landmark.UpdateCells(30 / landmarks.Where(item => item.obj.GetComponent<LandmarkHandler>().landmarkType
                == landmark.obj.GetComponent<LandmarkHandler>().landmarkType).Count() + 10);
                foreach (Cell cell in landmark.affectedCells)
                {
                    additionalRetentions[cell.cellPos.x, cell.cellPos.y].defaultMemory += (int)landmark.cells[cell.cellPos.x, cell.cellPos.y].weight;
                    additionalRetentions[cell.cellPos.x, cell.cellPos.y].decreaseCountdownDefault += Mathf.RoundToInt(landmark.cells[cell.cellPos.x, cell.cellPos.y].weight / 10);
                }


            }
            yield return null;
        }

        public IEnumerator UpdateEdges()
        {
            edges = new();
            foreach (ScaleWall wall in GameObject.FindObjectsOfType<ScaleWall>())
            {
                edges.Add(new Edge(wall.gameObject, Mathf.RoundToInt(Mathf.Pow(wall.transform.localScale.z, 0.8f))));
            }

            foreach (Edge edge in edges)
            {
                edge.UpdateSides();
                foreach (Cell cell in edge.side1AffectedCells)
                {
                    additionalRetentions[cell.cellPos.x, cell.cellPos.y].defaultMemory += (int)edge.side1[cell.cellPos.x, cell.cellPos.y].weight;
                    additionalRetentions[cell.cellPos.x, cell.cellPos.y].decreaseCountdownDefault += Mathf.RoundToInt(edge.side1[cell.cellPos.x, cell.cellPos.y].weight / 10);
                }
                foreach (Cell cell in edge.side2AffectedCells)
                {
                    additionalRetentions[cell.cellPos.x, cell.cellPos.y].defaultMemory += (int)edge.side2[cell.cellPos.x, cell.cellPos.y].weight;
                    additionalRetentions[cell.cellPos.x, cell.cellPos.y].decreaseCountdownDefault += Mathf.RoundToInt(edge.side2[cell.cellPos.x, cell.cellPos.y].weight / 10);
                }
            }
            yield return null;
        }

        public IEnumerator UpdateDistricts()
        {
            if (landmarks.Count > 1)
            {
                districts = new();

                foreach (Landmark landmark in landmarks)
                {
                    if (!districts.Any(item => item.containingLandmarks.Contains(landmark)))
                    {
                        districts.Add(new District(landmark));
                    }
                }


                foreach (District district in districts)
                {
                    foreach (Cell cell in district.affectedCells)
                    {
                        additionalRetentions[cell.cellPos.x, cell.cellPos.y].defaultMemory += (int)district.cells[cell.cellPos.x, cell.cellPos.y].weight;
                        additionalRetentions[cell.cellPos.x, cell.cellPos.y].defaultMemory += Mathf.RoundToInt(district.cells[cell.cellPos.x, cell.cellPos.y].weight / 10);
                        //Instantiate(go, cell.position3 + Vector3.up * cell.weight, Quaternion.identity);
                    }
                }
            }
            yield return null;
        }
    }
}
