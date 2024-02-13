using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Reflection;

public class GameView : MonoBehaviour
{
    private static GameView _instance;
    [System.NonSerialized] public LevelData levelData;

    private GridLayoutGroup gridLayout;
    public GameObject gridObject;

    public GameObject[] colorPrefabs;
    public GameObject boxTilePrefab;

    [System.NonSerialized] public Transform[,] cellObjects;


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
        cellObjects = new Transform[levelData.grid_height, levelData.grid_width];

        // Determining the edge sizes of cells in the grid
        gridLayout = gridObject.GetComponent<GridLayoutGroup>();
        RectTransform gridRectTransform = gridObject.GetComponent<RectTransform>();
        float gridWidth = gridRectTransform.sizeDelta.x;
        float cellEdgeLength = gridWidth / levelData.grid_width;
        gridRectTransform.sizeDelta = new Vector2(cellEdgeLength * levelData.grid_width, cellEdgeLength * levelData.grid_height);
        gridLayout.cellSize = new Vector2(cellEdgeLength, cellEdgeLength);

        // Önce gridi temizle
        foreach (Transform child in gridObject.transform)
        {
            Destroy(child.gameObject);
        }

        int index = 0;
        for (int row = 0; row < levelData.grid_height; row++)
        {
            for (int col = 0; col < levelData.grid_width; col++)
            {
                // Instantiate the tile prefab
                GameObject cellObject = ChooseTheNewCell(levelData.grid[levelData.grid.Count-1-index]);

                // Set gridObject as parent
                cellObject.transform.SetParent(gridObject.transform, false);

                // Adjust Tile's position
                RectTransform cellRectTransform = cellObject.GetComponent<RectTransform>();
                cellRectTransform.anchoredPosition = new Vector2(col * cellEdgeLength, row * cellEdgeLength);
                cellRectTransform.sizeDelta = new Vector2(cellEdgeLength, cellEdgeLength);
                // Debug.Log(cellRectTransform.anchoredPosition);

                cellObjects[row, col] = cellObject.transform;
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

    /*
    private void GenerateGrid()
    {
        cellObjects = new Transform[levelData.grid_height, levelData.grid_width];

        // Determining the edge sizes of cells in the grid
        gridLayout = gridObject.GetComponent<GridLayoutGroup>();
        RectTransform gridRectTransform = gridObject.GetComponent<RectTransform>();
        float gridWidth = gridRectTransform.sizeDelta.x;
        float cellEdgeLength = gridWidth / levelData.grid_width;
        gridRectTransform.sizeDelta = new Vector2(cellEdgeLength * levelData.grid_width, cellEdgeLength * levelData.grid_height);
        gridLayout.cellSize = new Vector2(cellEdgeLength, cellEdgeLength);

        // Önce gridi temizle
        foreach (Transform child in gridObject.transform)
        {
            Destroy(child.gameObject);
        }

        int index = 0;
        for (int row = levelData.grid_height - 1; row >= 0; row--)
        {
            for (int col = 0; col < levelData.grid_width; col++)
            {
                Debug.Log("row, col = " + row + col);
                GameObject cellObject = ChooseTheNewCell(levelData.grid, index);

                // Set gridObject as parent
                cellObject.transform.SetParent(gridObject.transform, false);

                // Adjust Tile's position
                RectTransform cellRectTransform = cellObject.GetComponent<RectTransform>();
                cellRectTransform.anchoredPosition = new Vector2(col * cellEdgeLength, (levelData.grid_height - 1 - row) * cellEdgeLength);
                cellRectTransform.sizeDelta = new Vector2(cellEdgeLength, cellEdgeLength);

                cellObjects[row, col] = cellObject.transform;
                index++;
            }
        }
    }

    private GameObject ChooseTheNewCell(List<String> grid, int index)
    {
        Debug.Log("grid[" + index + "] = " + grid[index]);
        if (grid[index] == "r")
        {
            return Instantiate(colorPrefabs[0]);
        }else if (grid[index] == "g")
        {
            return Instantiate(colorPrefabs[1]);
        }
        else if (grid[index] == "b")
        {
            return Instantiate(colorPrefabs[2]);
        }
        else if (grid[index] == "y")
        {
            int randomIndex = UnityEngine.Random.Range(0, colorPrefabs.Length);
            return Instantiate(colorPrefabs[randomIndex]);
        }
        else if (grid[index] == "rand")
        {
            return null;
        }else if (grid[index] == "t")
        {
            return null;
        }
        else if (grid[index] == "rov")
        {
            return null;
        }
        else if (grid[index] == "roh")
        {
            return null;
        }
        else if (grid[index] == "bo")
        {
            return Instantiate(boxTilePrefab);
        }
        else if (grid[index] == "s")
        {
            return null;
        }
        else if (grid[index] == "v")
        {
            return null;
        }
        return null;
    } */


    [Serializable]
    public class LevelData
    {
        public int level_number;
        public int grid_width;
        public int grid_height;
        public int move_count;
        public List<string> grid;
    }
}