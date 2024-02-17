using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Reflection;
using System.Drawing;
using Unity.VisualScripting;
using System.Linq;

public class GameView : MonoBehaviour
{
    public class Cell
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public string Type { get; set; }
        public List<Cell> Neighbors { get; set; }

        public bool isActive;

        public Cell(int row, int col, String type)
        {
            Row = row;
            Col = col;
            Type = type;
            Neighbors = new List<Cell>();
        }
    }

    private static GameView _instance;
    [System.NonSerialized] public LevelData levelData;

    private GridLayoutGroup gridLayout;
    public GameObject gridObject;

    public GameObject[] colorPrefabs;
    public GameObject boxTilePrefab;
    public GameObject stoneTilePrefab;

    [System.NonSerialized] public Transform[,] cellObjects;
    [System.NonSerialized] public String[,] objectType;

    private List<Cell> cells;
    private int totalGridSize;

    private int _boxCount;
    private int _stoneCount;
    private int _vaseCount;

    public static GameView Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameView>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(GameView).Name);
                    _instance = singletonObject.AddComponent<GameView>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }

    public Text levelText; // Seviye bilgisini göstermek için bir Text bileşeni

    // Model ile etkileşime geçerek oyunun durumunu güncelleyen metotlar buraya eklenebilir

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        totalGridSize = 100;
        cellObjects = new Transform[totalGridSize + 10, 30];
        objectType = new String[totalGridSize + 10, 30];
        cells = new List<Cell>();
    }

    // Seviye bilgisini güncelleyen metot
    public void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + level.ToString();
        }
    }

    // Diğer Game View metotları buraya eklenebilir

    public void LoadTheNextLevel(int level)
    {
        string levelsFolderPath = "Assets/Levels";

        string filePath = Path.Combine(levelsFolderPath, "level_" + level.ToString() + ".json");

        if (File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);
            levelData = JsonUtility.FromJson<LevelData>(jsonString);
            Debug.Log(levelData.level_number);
            _boxCount = 0; _stoneCount = 0; _vaseCount = 0;
            foreach(String tile in levelData.grid)
            {
                if(tile == "bo")
                {
                    _boxCount++;
                }else if(tile =="stone tile(Clone)")
                {
                    _stoneCount++;
                }else if (tile =="vase tile(Clone)")
                {
                    _vaseCount++;
                }
            }

            // Okunan verileri kullanarak işlemler yapabilirsiniz
            GenerateGrid(GetCellObjects());
        }
        else
        {
            Debug.LogError("Level file not found: " + filePath);
        }

    }

    private Transform[,] GetCellObjects()
    {
        return cellObjects;
    }

    private void GenerateGrid(Transform[,] cellObjects)
    {
       // cellObjects = new Transform[levelData.grid_height, levelData.grid_width];

        // Determining the edge sizes of cells in the grid
        gridLayout = gridObject.GetComponent<GridLayoutGroup>();
        RectTransform gridRectTransform = gridObject.GetComponent<RectTransform>();
        SpriteRenderer spriteRenderer = colorPrefabs[0].GetComponent<SpriteRenderer>();
        float originalCellWidth = spriteRenderer.sprite.bounds.size.x;

        float gridWidth = gridRectTransform.sizeDelta.x;
        float desiredCellEdge = gridWidth / levelData.grid_width;
        float scaleFactor = desiredCellEdge / originalCellWidth;

        gridRectTransform.sizeDelta = new Vector2(desiredCellEdge * levelData.grid_width, desiredCellEdge * totalGridSize);
        gridLayout.cellSize = new Vector2(desiredCellEdge, desiredCellEdge);
        Debug.Log("desired edge = " + desiredCellEdge + "scale = " + scaleFactor);
        // Önce gridi temizle
        foreach (Transform child in gridObject.transform)
        {
            Destroy(child.gameObject);
        }
        float startPositionX = -(desiredCellEdge * levelData.grid_width-1) / 4;
        float startPositionY = -(desiredCellEdge * levelData.grid_height-1) / 4;
        int index = 0;

        for (int row = 0; row < levelData.grid_height; row++)
        {
            for (int col = 0; col < levelData.grid_width; col++)
            {
                // Instantiate the tile prefab
                GameObject cellObject = ChooseTheNewCell(levelData.grid[index]);

                // Set gridObject as parent
                cellObject.transform.SetParent(gridObject.transform, false);

                // Adjust Tile's position
                Transform cellTransform = cellObject.GetComponent<Transform>();
                cellTransform.localPosition = new Vector2(col * desiredCellEdge/2 + startPositionX, row * desiredCellEdge/2 + startPositionY);
                cellTransform.localScale = new Vector2(scaleFactor/2, scaleFactor/2);

                // Debug.Log(cellRectTransform.anchoredPosition);

                cellObjects[row, col] = cellObject.transform;
                objectType[row, col] = cellObject.name;

                Cell newCell = new Cell(row, col, cellObject.name);
                newCell.isActive = true;
                cells.Add(newCell);
                
                index++;
            }
        }

        for(int row = levelData.grid_height; row < totalGridSize; row++)
        {
            for (int col = 0; col < levelData.grid_width; col++)
            {
                // Instantiate the tile prefab
                GameObject cellObject = ChooseTheNewCell("rand");

                // Set gridObject as parent
                cellObject.transform.SetParent(gridObject.transform, false);

                // Adjust Tile's position
                Transform cellTransform = cellObject.GetComponent<Transform>();
                cellTransform.localPosition = new Vector2(col * desiredCellEdge / 2 + startPositionX, row * desiredCellEdge / 2 + startPositionY);
                cellTransform.localScale = new Vector2(scaleFactor / 2, scaleFactor / 2);

                // Debug.Log(cellRectTransform.anchoredPosition);

                cellObjects[row, col] = cellObject.transform;
                objectType[row, col] = cellObject.name;

                Cell newCell = new Cell(row, col, cellObject.name);
                newCell.isActive = false;
                cells.Add(newCell);

                cellObject.SetActive(false);

                index++;
            }
        }

        foreach(Cell mainCell in cells)
        {
            List<Cell> neighborsOfNewCell = new List<Cell>();
            foreach (Cell cell in cells)
            {
                if (cell.Row == mainCell.Row)
                {
                    if (cell.Col == mainCell.Col - 1 || cell.Col == mainCell.Col + 1)
                    {
                        neighborsOfNewCell.Add(cell);
                    }
                }
                else if (cell.Col == mainCell.Col)
                {
                    if (cell.Row == mainCell.Row - 1 || cell.Row == mainCell.Row + 1)
                    {
                        neighborsOfNewCell.Add(cell);
                    }
                }
            }
            mainCell.Neighbors = neighborsOfNewCell;
        }
    }

    private GameObject ChooseTheNewCell(String cell)
    {
        if (cell == "r")
        {
            return Instantiate(colorPrefabs[0]);
        }
        else if (cell == "g")
        {
            return Instantiate(colorPrefabs[1]);
        }
        else if (cell == "b")
        {
            return Instantiate(colorPrefabs[2]);
        }
        else if (cell == "y")
        {
            return Instantiate(colorPrefabs[3]);
        }
        else if (cell == "rand")
        {
            int randomIndex = UnityEngine.Random.Range(0, colorPrefabs.Length);
            return Instantiate(colorPrefabs[randomIndex]);
        }
        else if (cell == "t")
        {
            return null;
        }
        else if (cell == "rov")
        {
            return null;
        }
        else if (cell == "roh")
        {
            return null;
        }
        else if (cell == "bo")
        {
            return Instantiate(boxTilePrefab);
        }
        else if (cell == "s")
        {
            return Instantiate(stoneTilePrefab);
        }
        else if (cell == "v")
        {
            return null;
        }
        return null;
    }

    [Serializable]
    public class LevelData
    {
        public int level_number;
        public int grid_width;
        public int grid_height;
        public int move_count;
        public List<string> grid;
    }

    public void MoveTheCell(Transform currentTransform,int moveAngle)
    {
        int row, col, newRow = -1, newCol = -1;
        LocationFinder(out row, out col,currentTransform);
        if (objectType[row,col]=="red tile(Clone)" || objectType[row, col] == "blue tile(Clone)" || objectType[row, col] == "green tile(Clone)" || objectType[row, col] == "yellow tile(Clone)")
        {
            if (moveAngle <= 135 && moveAngle >= 45)
            {
                if (row < levelData.grid_height - 1)
                {
                    newRow = row + 1;
                    newCol = col;
                    PositionSwap(row, col, newRow, newCol);
                }
            }
            else if (moveAngle <= -135 || moveAngle >= 135)
            {
                if (col > 0)
                {
                    newRow = row;
                    newCol = col - 1;
                    PositionSwap(row, col, newRow, newCol);
                }
            }
            else if (moveAngle <= 45 && moveAngle >= -45)
            {
                if (col < levelData.grid_width - 1)
                {
                    newRow = row;
                    newCol = col + 1;
                    PositionSwap(row, col, newRow, newCol);
                }
            }
        }
    }

    private void PositionSwap(int row1, int col1, int row2, int col2)
    {
        Transform tempTransform1 = cellObjects[row1, col1];
        Transform tempTransform2 = cellObjects[row2, col2];

        cellObjects[row1, col1] = tempTransform2;
        cellObjects[row2, col2] = tempTransform1;

        Vector3 tempPosition = tempTransform1.position;
        tempTransform1.position = tempTransform2.position;
        tempTransform2.position = tempPosition;

        String temp = objectType[row1, col1];
        objectType[row1, col1] = objectType[row2, col2];
        objectType[row2 , col2] = temp;

        Cell cell1 = FindCell(row1, col1, cells);
        Cell cell2 = FindCell(row2, col2, cells);
        temp = cell1.Type;
        cell1.Type = cell2.Type;
        cell2.Type = temp;
    }

    public void Blast(Transform currentTransform)
    {
        int row, col;
        LocationFinder(out row, out col, currentTransform);
        Cell currentCell = FindCell(row, col, cells);
        List<Cell> connectedCells = FindConnectedCells(currentCell);
        if(connectedCells.Count > 1)
        {
            foreach (Cell cell in connectedCells)
            {
                cellObjects[cell.Row, cell.Col].gameObject.SetActive(false);
                cell.Type = "empty";
                objectType[cell.Row, cell.Col] = "empty";
            }
            BlastBox(connectedCells);
            DropCellsIntoEmptyCells(connectedCells);
        }
    }

    private void BlastBox(List<Cell> connectedCells)
    {
        foreach(Cell cell in connectedCells)
        {
            foreach(Cell neighbor in cell.Neighbors)
            {
                if(neighbor.Type =="box tile(Clone)")
                {
                    cellObjects[neighbor.Row,neighbor.Col].gameObject.SetActive(false);
                    neighbor.Type = "empty";
                    objectType[neighbor.Row, neighbor.Col] = "empty";
                    _boxCount--;
                    ControlIfGameFinished();
                }
            }
        }
    }

    private void ControlIfGameFinished()
    {
        if(_boxCount==0 && _stoneCount==0 && _vaseCount == 0)
        {
            cells.Clear();
            foreach (Transform child in gridObject.transform)
            {
                Destroy(child.gameObject);
            }
            GameModel.Instance.currentLevel++;
            GameController.Instance.buttonObject.SetActive(true);
        }
    }

    private void DropCellsIntoEmptyCells(List<Cell> connectedCells)
    {
        List<Cell> cellListDownTop = new List<Cell>();
        cellListDownTop = cells.OrderBy(cell => cell.Row).ToList();

        foreach(Cell cell in cellListDownTop)
        {
            if (cell.Row < totalGridSize - 1 && objectType[cell.Row + 1,cell.Col] == "empty")
            {
                int movedCellCount = 0;
                for (int i = cell.Row+1; i < totalGridSize; i++)
                {
                    if (objectType[i, cell.Col] != "empty")
                    {
                        PositionSwap(i, cell.Col, cell.Row + 1 + movedCellCount, cell.Col);
                        if(i>=levelData.grid_height && cell.Row + 1 + movedCellCount < levelData.grid_height)
                        {
                            Cell ascendingCell = FindCell(i, cell.Col, cells);
                            ascendingCell.isActive = false;
                            cellObjects[i, cell.Col].gameObject.SetActive(false);

                            Cell fallenCell = FindCell(cell.Row+1+movedCellCount,cell.Col, cells);
                            fallenCell.isActive = true;
                            cellObjects[cell.Row + 1 + movedCellCount, cell.Col].gameObject.SetActive(true);
                        }
                        movedCellCount++;
                    }
                }
            }
        }
    }

    private List<Cell> FindConnectedCells(Cell startCell) // Finds connected cells by BFS algorithm
    {
        List<Cell> connectedCells = new List<Cell>();
        Queue<Cell> queue = new Queue<Cell>();
        HashSet<Cell> visited = new HashSet<Cell>();

        queue.Enqueue(startCell);
        visited.Add(startCell);

        while (queue.Count > 0)
        {
            Cell currentCell = queue.Dequeue();
            connectedCells.Add(currentCell);

            foreach (Cell neighbor in currentCell.Neighbors)
            {
                if (neighbor.Type == startCell.Type && !visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }
        return connectedCells;
    }

    private void LocationFinder(out int row, out int col, Transform currentTransform)
    {
        row = -1;
        col = -1;
        for (int i = 0; i < levelData.grid_height; i++)
        {
            for (int j = 0; j < levelData.grid_width; j++)
            {
             //   Debug.Log("cellObject[" + i + "," + j + "] = " + cellObjects[i, j] + " " + cellObjects[i, j].position);
                if (cellObjects[i, j] == currentTransform)
                {
                    row = i;
                    col = j;
                   // Debug.Log("Tespit edilen row ve col = " + i + " " + j);
                    break;
                }
            }
            if (row != -1) break;
        }
    }

    private Cell FindCell(int row, int col, List<Cell> linkedList)
    {
        foreach (Cell cell in linkedList)
        {
            if (cell.Row == row && cell.Col == col)
            {
                return cell;
            }
        }
        return null; // Belirtilen row ve col değerlerine sahip hücre bulunamadı.
    }
}