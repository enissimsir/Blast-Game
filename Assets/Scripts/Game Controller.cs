using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private static GameController _instance;

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

    // Diğer GameController fonksiyonları buraya eklenebilir

    // Yeni sahneyi yüklemek için metot
    public void LoadNewScene()
    {
        Debug.Log("oeooe");
        int currentLevel;
        if (GameModel.Instance != null)
        {
            currentLevel = GameModel.Instance.currentLevel;
            SceneManager.LoadScene(currentLevel);
        }

        
    }
}
