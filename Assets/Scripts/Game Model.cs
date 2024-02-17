using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel : MonoBehaviour
{
    private static GameModel _instance;
    [System.NonSerialized] public int currentLevel;

    public static GameModel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameModel>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(GameModel).Name);
                    _instance = singletonObject.AddComponent<GameModel>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }

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
        currentLevel = 1;
        GameView.Instance.UpdateLevelText(1);
    }

}