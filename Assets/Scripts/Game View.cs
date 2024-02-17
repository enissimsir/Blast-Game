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
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

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

    public Sprite greenSprite;
    public Sprite blueSprite;
    public Sprite redSprite;
    public Sprite yellowSprite;

    public Sprite tntSprite;

    [System.NonSerialized] public Transform[,] cellObjects;
    [System.NonSerialized] public String[,] objectType;

    private List<Cell> cells; // In this list, there are all cells in the grid (the visible ones, unvisible ones, boxes, tnts etc.)
    private int totalGridSize; // This is not the grid size in the json files. This value includes the cells in the pool

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

    public Text levelText;

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

    // A method to update the button on the main menu
    public void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + level.ToString();
        }
    }

    // This method reads the values in the json files and calls the method that creates the grid
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
            GenerateGrid(cellObjects);
        }
        else
        {
            Debug.LogError("Level file not found: " + filePath);
        }
    }

    // This method creates the grid by using levelData values (the values readen from the json files)
    private void GenerateGrid(Transform[,] cellObjects)
    {
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

        // Here, the neighbor cells of all cells are determined
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

        // Detect groups bigger than 5x5 to turn them into tnt state
        DetectCellGroupsBiggerThan5();
    }

    // Create the right object based on the values on levelData
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

    // This method finds connected cells by BFS (Breadth-First Search) Algorithm
    // This method takes the starting cell as an argument and creates a list with linked cells of the same color and returns it
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
                if ((neighbor.Type.Contains(startCell.Type) || startCell.Type.Contains(neighbor.Type)) && neighbor.Row < levelData.grid_height && !visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }
        return connectedCells;
    }

    // If the user drags the cell, this method gets called from Tile script
    public void MoveTheCell(Transform currentTransform,int moveAngle)
    {
        int row, col, newRow = -1, newCol = -1;
        LocationFinder(out row, out col,currentTransform);
        if (objectType[row,col].Contains("red tile(Clone)") || objectType[row, col].Contains("blue tile(Clone)") || objectType[row, col].Contains("green tile(Clone)") || objectType[row, col].Contains("yellow tile(Clone)"))
        {
            if (moveAngle <= 135 && moveAngle >= 45) // The user dragged into top
            {
                if (row < levelData.grid_height - 1)
                {
                    newRow = row + 1;
                    newCol = col;
                    PositionSwap(row, col, newRow, newCol);
                }
            }
            else if (moveAngle <= -135 || moveAngle >= 135) // The user dragged into left
            {
                if (col > 0)
                {
                    newRow = row;
                    newCol = col - 1;
                    PositionSwap(row, col, newRow, newCol);
                }
            }
            else if (moveAngle <= 45 && moveAngle >= -45) // The user dragged into right
            {
                if (col < levelData.grid_width - 1)
                {
                    newRow = row;
                    newCol = col + 1;
                    PositionSwap(row, col, newRow, newCol);
                }
            }
        }
        DetectCellGroupsBiggerThan5();
    }

    // A method to swap 2 cells
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

    // The mothod to blast a cell when it's tapped. It gets called from Tile script
    public void Blast(Transform currentTransform)
    {
        int row, col;
        LocationFinder(out row, out col, currentTransform);

        //Checking if it is a tile cell
        if (objectType[row, col].Contains("tile(Clone)"))
        {
            Cell currentCell = FindCell(row, col, cells);
            List<Cell> connectedCells = FindConnectedCells(currentCell);
            if (connectedCells.Count > 1)
            {
                foreach (Cell cell in connectedCells)
                {
                    if (cellObjects[cell.Row, cell.Col] == currentTransform && objectType[cell.Row, cell.Col].Contains("tnt") && objectType[cell.Row, cell.Col].Contains("tile(Clone)"))
                    {
                        SpriteRenderer spriteRenderer = cellObjects[cell.Row, cell.Col].gameObject.GetComponent<SpriteRenderer>();
                        spriteRenderer.sprite = tntSprite;
                        cell.Type = "tnt";
                        objectType[cell.Row, cell.Col] = "tnt";
                    }
                    else
                    {
                        cellObjects[cell.Row, cell.Col].gameObject.SetActive(false);
                        cell.Type = "empty";
                        objectType[cell.Row, cell.Col] = "empty";
                    }
                }
                BlastBox(connectedCells);
                DropCellsIntoEmptyCells();
                ControlIfGameFinished();
            }
        } 
        // Checking if it is a tnt cell
        else if(objectType[row, col].Contains("tnt"))
        {
            Cell currentCell = FindCell(row, col, cells);
            TNTExplotion(currentCell);
            DropCellsIntoEmptyCells();
            ControlIfGameFinished();
        }
    }

    // Destroy the cells inside the tnt explotion area
    private void TNTExplotion(Cell tntCell)
    {
        int rowStart, colStart, rowEnd, colEnd;
        if (tntCell.Row < 2) rowStart = 0;
        else rowStart = tntCell.Row - 2;
        if (tntCell.Col < 2) colStart = 0;
        else colStart = tntCell.Col - 2;
        
        if(tntCell.Row > levelData.grid_height-2) rowEnd = levelData.grid_height;
        else rowEnd = tntCell.Row + 2;
        if(tntCell.Col > levelData.grid_width-2) colEnd = levelData.grid_width;
        else colEnd = tntCell.Col + 2;

        for (int i = rowStart; i<rowEnd;i++)
        {
            for(int j = colStart; j<colEnd;j++)
            {
                if (objectType[i, j] == "box(Clone)") _boxCount--;
                else if (objectType[i, j] == "stone(Clone)") _stoneCount--;
                else if (objectType[i, j] == "vase(Clone)") _vaseCount--;

                cellObjects[i,j].gameObject.SetActive(false);
                Cell cellToBlast = FindCell(i, j, cells);
                cellToBlast.Type = "empty";
                objectType[i, j] = "empty";
            }
        }
    }

    private void BlastBox(List<Cell> connectedCells)
    {
        foreach(Cell cell in connectedCells)
        {
            foreach(Cell neighbor in cell.Neighbors)
            {
                if(neighbor.Type =="box(Clone)")
                {
                    cellObjects[neighbor.Row,neighbor.Col].gameObject.SetActive(false);
                    neighbor.Type = "empty";
                    objectType[neighbor.Row, neighbor.Col] = "empty";
                    _boxCount--;
                }
            }
        }
    }

    private void ControlIfGameFinished()
    {
        Debug.Log("box count = " + _boxCount);
        if(_boxCount==0 && _stoneCount==0 && _vaseCount == 0)
        {
            cells.Clear();
            foreach (Transform child in gridObject.transform)
            {
                Destroy(child.gameObject);
            }
            GameModel.Instance.currentLevel++;
            GameController.Instance.buttonObject.SetActive(true);
            UpdateLevelText(GameModel.Instance.currentLevel);
        }
    }

    private void DropCellsIntoEmptyCells()
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
        DetectCellGroupsBiggerThan5();
    }

    // It detects the cell groups bigger than 5 cells and converts them into tnt state by another method
    private void DetectCellGroupsBiggerThan5()
    {
        List<Cell> cellListDownTop = new List<Cell>();
        cellListDownTop = cells.OrderBy(cell => cell.Row).ToList();
        for(int i = 0;i<levelData.grid_height*levelData.grid_height;i++)
        {
            if (cellListDownTop[i].Type.Contains("tile(Clone)")) //zaten tnt durumunda mı kontrolü
            {
                List<Cell> connectedCells = FindConnectedCells(cellListDownTop[i]);
                if (connectedCells.Count >= 5)
                {
                    ConvertIntoTNTMode(connectedCells);
                }
            }
        }
    }

    private void ConvertIntoTNTMode(List<Cell> connectedCells)
    {
        foreach(Cell cell in connectedCells)
        {
            SpriteRenderer spriteRenderer = cellObjects[cell.Row,cell.Col].gameObject.GetComponent<SpriteRenderer>();
            

            if (spriteRenderer != null)
            {
                if (cell.Type.Contains("red"))
                {
                    spriteRenderer.sprite = redSprite;
                    cell.Type = "tnt red tile(Clone)";
                }
                else if (cell.Type.Contains("green"))
                {
                    spriteRenderer.sprite = greenSprite;
                    cell.Type = "tnt green tile(Clone)";
                }
                else if (cell.Type.Contains("blue"))
                {
                    spriteRenderer.sprite = blueSprite;
                    cell.Type = "tnt blue tile(Clone)";
                }
                else if (cell.Type.Contains("yellow"))
                {
                    spriteRenderer.sprite = yellowSprite;
                    cell.Type = "tnt yellow tile(Clone)";
                }
                objectType[cell.Row, cell.Col] = cell.Type;
            }
            else
            {
                Debug.LogError("Error code 1!");
            }
        }
    }

    // Takes a transform variable as an argument and returns the row and col values of that cell in the grid
    private void LocationFinder(out int row, out int col, Transform currentTransform)
    {
        row = -1;
        col = -1;
        for (int i = 0; i < levelData.grid_height; i++)
        {
            for (int j = 0; j < levelData.grid_width; j++)
            {
                if (cellObjects[i, j] == currentTransform)
                {
                    row = i;
                    col = j;
                    break;
                }
            }
            if (row != -1) break;
        }
    }

    // Takes row and col values as an argument and returns the Cell class variable
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