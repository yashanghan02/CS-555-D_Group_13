using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static GameManager instance;
    public int Age;
    public int Score;
    public int bestScore;

    private void Awake()
    {
        instance = this;
        Time.timeScale = 0;
    }
    public void startGame()
    {
        Time.timeScale = 1;
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public int TrySetHighScore()
    {
        int value = PlayerPrefs.GetInt("BestScore");
        if (value < Score)
        {
            PlayerPrefs.SetInt("BestScore", Score);
            value = Score;
        }
        return value;
        
    }

    public void UpdateScore(int value)
    {
        Score += value;
        UIManager.instance.AddPoints(Score);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
