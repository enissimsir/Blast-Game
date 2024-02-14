using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Reflection;
using System.Drawing;



public class GameView : MonoBehaviour
{
    private static GameView _instance;
    [System.NonSerialized] public LevelData levelData;

    private GridLayoutGroup gridLayout;
    public GameObject gridObject;

    public GameObject[] colorPrefabs;
    public GameObject boxTilePrefab;

    [System.NonSerialized] public Transform[,] cellObjects;
    [System.NonSerialized] public String[,] objectType;

    private List<Cell> cells;

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
        cellObjects = new Transform[20, 20];
        objectType = new String[20,20];
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

        gridRectTransform.sizeDelta = new Vector2(desiredCellEdge * levelData.grid_width, desiredCellEdge * levelData.grid_height);
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
                objectType[row,col] = levelData.grid[index];

                Cell newCell = new Cell(row, col, levelData.grid[index]);
                List<Cell> neighborsOfNewCell = new List<Cell>();
                cells.Add(newCell);
                foreach (Cell cell in cells)
                {
                    if(cell.Row == row)
                    {
                        if(cell.Col == col-1 || cell.Col == col + 1)
                        {
                            neighborsOfNewCell.Add(cell);
                        }
                    }
                    else if(cell.Col == col)
                    {
                        if(cell.Row == row-1 || cell.Row == row + 1)
                        {
                            neighborsOfNewCell.Add(cell);
                        }
                    }
                }
                newCell.Neighbors = neighborsOfNewCell;

                index++;
            }
        }
     //   Debug.Log("00 = " + cellObjects[0, 0].gameObject.name);
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
            return null;
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
        int row, col, newRow, newCol;
        LocationFinder(out row, out col,currentTransform);
        if (objectType[row,col]=="r" || objectType[row, col] == "b" || objectType[row, col] == "g" || objectType[row, col] == "y")
        {
            if (moveAngle <= 135 && moveAngle >= 45)
            {
                if (row < levelData.grid_height - 1)
                {
                    newRow = row + 1;
                    newCol = col;
                    positionSwap(row, col, newRow, newCol);
                }
            }
            else if (moveAngle <= -135 || moveAngle >= 135)
            {
                if (col > 0)
                {
                    newRow = row;
                    newCol = col - 1;
                    positionSwap(row, col, newRow, newCol);
                }
            }
            else if (moveAngle <= 45 && moveAngle >= -45)
            {
                if (col < levelData.grid_width - 1)
                {
                    newRow = row;
                    newCol = col + 1;
                    positionSwap(row, col, newRow, newCol);
                }
            }
        }
    }

    private void positionSwap(int row1, int col1, int row2, int col2)
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
        objectType[row2 , col2] = objectType[row1, col1];
    }

    public void Blast(Transform currentTransform)
    {
        int row, col;
        LocationFinder(out row, out col, currentTransform);
        Cell currentCell = new Cell(row, col, objectType[row,col]);
        List<Cell> connectedCells = new List<Cell>();
        connectedCells = FindConnectedCells(currentCell);
        foreach(Cell cell in connectedCells)
        {
            Debug.Log("row = " + row + "col = " + col);
        }
    }

    private List<Cell> FindConnectedCells(Cell startCell)
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

    
}