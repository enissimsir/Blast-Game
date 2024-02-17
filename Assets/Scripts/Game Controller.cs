using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private static GameController _instance;
    public GameObject buttonObject;

    public static GameController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameController>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(GameController).Name);
                    _instance = singletonObject.AddComponent<GameController>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }

    public void LoadTheLevel()
    {
        buttonObject.SetActive(false);

        int currentLevel;
        if (GameModel.Instance != null)
        {
            currentLevel = GameModel.Instance.currentLevel;
            //    SceneManager.LoadScene(currentLevel);
            GameView.Instance.LoadTheNextLevel(currentLevel);
        }

        
    }
}
