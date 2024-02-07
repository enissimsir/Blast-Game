using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameView : MonoBehaviour
{
    private static GameView _instance;

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



}

