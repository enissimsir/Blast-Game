using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class GameView : MonoBehaviour
{
    private static GameView _instance;
    [System.NonSerialized] public LevelData levelData;

    private GridLayoutGroup gridLayout;
    public GameObject gridObject;
    public GameObject tilePrefab;

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
            GenerateGrid();

        }
        else
        {
            Debug.LogError("Level file not found: " + filePath);
        }

    }

    private void GenerateGrid()
    {
        cellObjects = new Transform[10, 10];

        // Grid içerisindeki hücrelerin kenar boylarını belirleme
        gridLayout = gridObject.GetComponent<GridLayoutGroup>();
        RectTransform gridRectTransform = gridObject.GetComponent<RectTransform>();
        float gridWidth = gridRectTransform.sizeDelta.x;
        float cellEdgeLength = gridWidth / levelData.grid_width;
        gridRectTransform.sizeDelta = new Vector2(gridRectTransform.sizeDelta.x, cellEdgeLength * levelData.grid_height);
        gridLayout.cellSize = new Vector2(cellEdgeLength, cellEdgeLength);

        // Önce gridi temizle
        foreach (Transform child in gridObject.transform)
        {
            Destroy(child.gameObject);
        }

        for (int row = 0; row < levelData.grid_height; row++)
        {
            for (int col = 0; col < levelData.grid_width; col++)
            {
                // Tile prefabını instantiate et ve grid içinde konumlandır
                GameObject cellObject = Instantiate(tilePrefab, gridObject.transform);
                // Tile'ın pozisyonunu ve boyutunu ayarla
                cellObject.transform.localPosition = Vector3.zero;
                cellObject.transform.localScale = Vector3.one;

                cellObjects[row, col] = cellObject.transform;
            }
        }
    }

    /*
    private void GenerateGrid()
    {
        // Grid'i oluştur
        for (int x = 0; x < levelData.grid_width; x++)
        {
            for (int y = 0; y < levelData.grid_height; y++)
            {
                // Hücre pozisyonunu belirle
                Vector3 cellPosition = new Vector3(x, y, 0);
                // Hücreyi oluştur
                GameObject cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                // Oluşturulan hücreyi GridManager altına ekle (isteğe bağlı)
                cell.transform.parent = transform;
            }
        }
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

